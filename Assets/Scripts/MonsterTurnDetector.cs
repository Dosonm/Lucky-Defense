using UnityEngine;

public class MonsterTurnDetector : MonoBehaviour
{
    private Monster monster;
    private Checkpoint lastCheckpoint;
  

    private void Awake()
    {
        monster = GetComponentInParent<Monster>();
    }

    public void ResetCheckpoint()
    {
        lastCheckpoint = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Checkpoint checkpoint))
            return;

        if (checkpoint == lastCheckpoint)
            return;

        lastCheckpoint = checkpoint;
        monster.rb.linearVelocity = checkpoint.direction.normalized * monster.speed;

        if(checkpoint.isFlip)
            monster.sr.flipX = !monster.sr.flipX;
    }
}
