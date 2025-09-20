using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _speed = 10f;       
    private float _damage = 20f;     
    private Transform target;
    

    public void SetTarget(Transform t, float bulletSpeed, float bulletDamage)
    {
        target = t;
        _speed = bulletSpeed;
        _damage = bulletDamage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * _speed * Time.deltaTime;

        // simple hit detection
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            Health h = target.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(_damage);
            }
            Destroy(gameObject);
        }
    }
}