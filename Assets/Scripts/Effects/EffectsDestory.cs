using UnityEngine;

public class EffectsDestory : MonoBehaviour
{
    [SerializeField] private float timeUntilDestroy = 4;

    private void Start()
    {
        Destroy(gameObject, timeUntilDestroy);
    }
}
