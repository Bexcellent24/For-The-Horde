using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
   #region Inspector Fields

    [Header("Grid Settings")]
    [Tooltip("Height of the tower in cells.")]
    [SerializeField] private int sizeX = 5;
    [SerializeField] private int sizeZ = 5;
    [SerializeField] private float tileSize = 2f;
    [SerializeField] private Transform parent;
    
    [Header("Tiles")]
    public List<WFCTile> allTiles;
    
    [Header("Border Settings")]
    [Tooltip("Tile to use for the border around the map")]
    [SerializeField] private WFCTile emptyTile;
    [Tooltip("Add border around the generated map")]
    [SerializeField] private bool addBorder = true;
    #endregion

    #region Private Fields

    private Cell[,,] grid;
    private GameObject[,,] instantiatedTiles;
    
    private const int height = 1;
    
    public Action OnMapGenerated;

    #endregion

    #region Nested Types
    
    // represents a single cell in the WFC grid.
    // stores possible tiles it can collapse into.
    private class Cell
    {
        public HashSet<WFCTile> possible;

        public Cell(IEnumerable<WFCTile> tiles)
        {
            possible = new HashSet<WFCTile>(tiles);
        }

        public int PossibleCount => possible.Count;
    }

    #endregion

    #region Public API
    
    
    // the placer calls this when its time to do the deed
    public void Generate()
    {
        InitializeGrid(); //get grid ready
        ApplyBoundaryConstraints(); //applying rules so it looks like a real thing
        StartCoroutine(RunWFC());  //do ittttttttt
    }

    #endregion

    #region Grid Setup

    private void InitializeGrid()
    {
        grid = new Cell[sizeX, height, sizeZ];
        instantiatedTiles = new GameObject[sizeX, height, sizeZ];

        if (parent == null) parent = transform;

        // Clear previous children
        foreach (Transform child in parent) DestroyImmediate(child.gameObject);

        // Fill grid with all possible tiles
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < sizeZ; z++)
                    grid[x, y, z] = new Cell(allTiles);
    }
    
    // Prevent edges from connecting to non-existent neighbours.
    private void ApplyBoundaryConstraints()
    {
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < sizeZ; z++)
                {
                    var cell = grid[x, y, z];
                    cell.possible.RemoveWhere(t =>
                        (x == 0 && t.GetSocket(Direction.West) != SocketType.None) ||
                        (x == sizeX - 1 && t.GetSocket(Direction.East) != SocketType.None) ||
                        (z == 0 && t.GetSocket(Direction.South) != SocketType.None) ||
                        (z == sizeZ - 1 && t.GetSocket(Direction.North) != SocketType.None)
                    );
                }
    }

    #endregion

    #region WFC Algorithm

    private IEnumerator RunWFC()
    {
        var propagationQueue = new Queue<Vector3Int>();

        while (true)
        {
            var candidates = GetLowestEntropyCells();
            if (candidates.Count == 0)
            {
                InstantiateResult();
                
                // Generate border after main generation is complete
                if (addBorder && emptyTile != null)
                    GenerateBorder();
                
                OnMapGenerated?.Invoke();
                yield break;
                
            }

            // Collapse random candidate
            Vector3Int chosenPos = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            var chosenCell = grid[chosenPos.x, chosenPos.y, chosenPos.z];
            WFCTile pickedTile = chosenCell.possible.ElementAt(UnityEngine.Random.Range(0, chosenCell.PossibleCount));
            chosenCell.possible = new HashSet<WFCTile> { pickedTile };

            propagationQueue.Enqueue(chosenPos);

            // Instantiate immediately
            InstantiateTile(chosenPos, pickedTile);
            yield return new WaitForSeconds(0.01f); 

            // Propagation
            while (propagationQueue.Count > 0)
            {
                Vector3Int current = propagationQueue.Dequeue();
                PropagateConstraints(current, propagationQueue);
            }

            yield return null;
        }
    }

    private List<Vector3Int> GetLowestEntropyCells()
    {
        int minOptions = int.MaxValue;
        var candidates = new List<Vector3Int>();

        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < sizeZ; z++)
                {
                    var cell = grid[x, y, z];
                    if (cell.PossibleCount == 0)
                        Debug.LogWarning($"Contradiction at {x},{y},{z}");
                    else if (cell.PossibleCount > 1)
                    {
                        if (cell.PossibleCount < minOptions)
                        {
                            minOptions = cell.PossibleCount;
                            candidates.Clear();
                            candidates.Add(new Vector3Int(x, y, z));
                        }
                        else if (cell.PossibleCount == minOptions)
                            candidates.Add(new Vector3Int(x, y, z));
                    }
                }

        return candidates;
    }

    private void PropagateConstraints(Vector3Int current, Queue<Vector3Int> queue)
    {
        var currentCell = grid[current.x, current.y, current.z];

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector3Int neighborPos = current + DirectionUtils.DirectionToOffset(dir);
            if (!IsInBounds(neighborPos)) continue;

            var neighborCell = grid[neighborPos.x, neighborPos.y, neighborPos.z];
            var allowed = new HashSet<WFCTile>();

            foreach (var curTile in currentCell.possible)
            {
                SocketType socketA = curTile.GetSocket(dir);

                foreach (var neighTile in neighborCell.possible)
                {
                    SocketType socketB = neighTile.GetSocket(DirectionUtils.Opposite(dir));
                    if (TileUtils.AreSocketsCompatible(socketA, socketB))
                        allowed.Add(neighTile);
                }
            }

            if (allowed.Count == 0)
            {
                Debug.LogWarning($"Contradiction at {neighborPos}");
                continue;
            }

            if (allowed.Count < neighborCell.PossibleCount)
            {
                neighborCell.possible = allowed;
                queue.Enqueue(neighborPos);
            }
        }
    }

    #endregion

    #region Border Generation

    private void GenerateBorder()
    {
        Transform p = parent != null ? parent : transform;

        // Generate border tiles around the perimeter
        for (int x = -1; x <= sizeX; x++)
        {
            for (int z = -1; z <= sizeZ; z++)
            {
                // Skip the inner area (the already generated map)
                if (x >= 0 && x < sizeX && z >= 0 && z < sizeZ)
                    continue;

                // Calculate world position for border tile
                Vector3 worldPos = new Vector3(x * tileSize, 0, z * tileSize);
                
                // Instantiate border tile
                GameObject borderTile = Instantiate(emptyTile.prefab, p.TransformPoint(worldPos), p.rotation, p);
                
                // Optional: Add a component or tag to identify border tiles
                borderTile.name = $"BorderTile_({x},{z})";
            }
        }
    }

    #endregion

    #region Tile Instantiation

    private void InstantiateResult()
    {
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < sizeZ; z++)
                {
                    var cell = grid[x, y, z];
                    if (cell.PossibleCount == 1)
                        InstantiateTile(new Vector3Int(x, y, z), cell.possible.First());
                }
    }

    private void InstantiateTile(Vector3Int pos, WFCTile tile)
    {
        // Destroy existing tile if any
        if (instantiatedTiles[pos.x, pos.y, pos.z] != null)
            Destroy(instantiatedTiles[pos.x, pos.y, pos.z]);

        // Parent for the tile
        Transform p = parent != null ? parent : transform;

        // Calculate position in world space
        Vector3 worldPos = new Vector3(pos.x * tileSize, pos.y * tileSize, pos.z * tileSize);

        // Instantiate tile at position with no rotation (or parent rotation if desired)
        instantiatedTiles[pos.x, pos.y, pos.z] = 
            Instantiate(tile.prefab, p.TransformPoint(worldPos), p.rotation, p);
    }

    #endregion

    #region Utilities

    private bool IsInBounds(Vector3Int v) => v.x >= 0 && v.x < sizeX && v.y >= 0 && v.y < height && v.z >= 0 && v.z < sizeZ;
    #endregion
}