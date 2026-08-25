using UnityEngine;

public class MonsterStat : MonoBehaviour
{
    public int MaxHp;
    public int CurrentHp;

    public System.Action onHealthChanged;
    public System.Action<GameObject> onDeath;

    private bool isDead;

    void Awake()
    {
        ResetStat();
    }

    public void ResetStat()
    {
        isDead = false;
        CurrentHp = MaxHp;
        onHealthChanged?.Invoke();
    }

    public virtual void TakeDamage(int damage, bool iscrit)
    {
        if (isDead)
            return;

        int totalDmg = damage;
        if(iscrit)
            totalDmg *= 2;

        CurrentHp -= totalDmg;
        onHealthChanged?.Invoke();

        Vector2 textPosition = (Vector2)transform.position + new Vector2(.4f, -.2f);
        DamageTextManager.instance.CreateDamageText(textPosition ,totalDmg,iscrit);

        if (CurrentHp <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        onDeath?.Invoke(gameObject);
        //Death();
    }
}
