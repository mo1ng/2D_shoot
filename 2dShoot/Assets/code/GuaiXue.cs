using UnityEngine;

public class GuaiXue : MonoBehaviour

{
    public float health = 100f;        // 血量
    public float bulletDamage = 20f;   // 子弹伤害

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ZiDan"))
        {
            TakeDamage();
            Destroy(collision.gameObject); // 销毁子弹
        }
    }

    void TakeDamage()
    {
        health -= bulletDamage;

        if (health <= 0)
        {
            Destroy(gameObject); // 血量归零，销毁自己
        }
    }
}