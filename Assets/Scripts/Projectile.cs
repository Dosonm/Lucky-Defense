using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitDistance = 0.01f;

    private Summons summons;
    private Monster target;
    private int damage;

    public void Initialize(Summons summons, Monster target, int damage)
    {
        this.summons = summons;
        this.target = target;
        this.damage = damage;
    }

    private void Update()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        Vector2 targetPosition = target.transform.position;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < hitDistance)
        {
            summons.DoDamage(target, damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        target = null;
        PoolManager.Instance.Release(gameObject);
    }
}