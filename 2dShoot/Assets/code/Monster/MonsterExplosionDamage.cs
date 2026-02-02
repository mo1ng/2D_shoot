using UnityEngine;

public class MonsterExplosionDamage : MonoBehaviour
{
    public float damageAmount = 10f;
    public string playerTag = "Player";
    public GuaiiKaoJin sourceObject;
    private bool hasDealtDamage = false;

    void Start()
    {
        if (sourceObject == null)
        {
            sourceObject = GetComponentInParent<GuaiiKaoJin>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage) return;

        if (sourceObject != null && sourceObject.HasDamagedPlayer()) return;

        if (other.CompareTag(playerTag))
        {
            Xue health = other.GetComponent<Xue>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                hasDealtDamage = true;

                if (sourceObject != null)
                {
                    sourceObject.MarkPlayerAsDamaged();
                }
            }
        }
    }
}