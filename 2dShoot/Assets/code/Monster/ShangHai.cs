using UnityEngine;

public class ShangHai : MonoBehaviour
{
    public float damageAmount = 10f;     // 伤害值
    public string playerTag = "Player";  // 玩家标签
    public bool damageOnlyOnce = true;   // 是否只造成一次伤害

    private bool hasDamaged = false;     // 标记是否已经造成伤害
    private GameObject lastDamagedPlayer; // 记录上次伤害的玩家

    // 碰撞检测
    void OnCollisionEnter(Collision collision)
    {
        if (hasDamaged && damageOnlyOnce) return;

        if (collision.gameObject.CompareTag(playerTag))
        {
            DamagePlayer(collision.gameObject);
        }
    }

    // 触发器检测
    void OnTriggerEnter(Collider other)
    {
        if (hasDamaged && damageOnlyOnce) return;

        if (other.CompareTag(playerTag))
        {
            DamagePlayer(other.gameObject);
        }
    }

    // 造成伤害
    void DamagePlayer(GameObject player)
    {
        // 检查是否已经对同一个玩家造成伤害
        if (damageOnlyOnce && lastDamagedPlayer == player) return;

        Xue health = player.GetComponent<Xue>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);

            // 记录伤害状态
            hasDamaged = true;
            lastDamagedPlayer = player;

            Debug.Log($"{gameObject.name} 对 {player.name} 造成 {damageAmount} 点伤害");
        }
    }

    // 可选：如果玩家在碰撞体内停留，防止重复伤害
    void OnCollisionStay(Collision collision)
    {
        // 这里可以根据需求决定是否处理持续碰撞
    }

    void OnTriggerStay(Collider other)
    {
        // 这里可以根据需求决定是否处理持续触发
    }

    // 重置伤害状态
    public void ResetDamage()
    {
        hasDamaged = false;
        lastDamagedPlayer = null;
        Debug.Log($"{gameObject.name} 伤害状态已重置");
    }

    // 设置伤害值
    public void SetDamage(float newDamage)
    {
        damageAmount = newDamage;
    }

    // 检查是否已经造成伤害
    public bool HasDamaged()
    {
        return hasDamaged;
    }

    // 获取最后伤害的玩家
    public GameObject GetLastDamagedPlayer()
    {
        return lastDamagedPlayer;
    }
}