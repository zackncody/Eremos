using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float moveSpeed;

    private float distanceToTargetToDestroyProjectile = 1f;


    void Update()
    {
        Vector3 moveDirNormalized = (target.position - transform.position).normalized;
        transform.position += moveDirNormalized * moveSpeed;

        if(Vector3.Distance(transform.position, target.position) < distanceToTargetToDestroyProjectile)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeProjectile(Transform target, float moveSpeed)
    {
        this.target = target;
        this.moveSpeed = moveSpeed;
    }
}
