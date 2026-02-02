using UnityEngine;

public class ShangHai : MonoBehaviour

{
    public float damageAmount = 10f;     // ÉËº¦Öµ
    public string playerTag = "Player";  // Íæ¼Ò±êÇ©

    // Åö×²¼ì²â
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            Xue health = collision.gameObject.GetComponent<Xue>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }

    // ´¥·¢Æ÷¼ì²â
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Xue health = other.GetComponent<Xue>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }
}