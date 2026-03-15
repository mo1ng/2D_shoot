using UnityEngine;

public class BoxXue : MonoBehaviour
{
    // 基础属性
    public float health = 100f;          // 当前血量
    public float bulletDamage = 20f;     // 每次受子弹伤害值

    // 血量显示TextMesh
    public TextMesh healthText;

    // 死亡掉落
    [Header("掉落设置")]
    [Range(0, 1)] public float dropChance = 0.5f;  // 是否掉落物品的概率 (0-1)
    public GameObject spawnOnDestroyA;    // 预制体A
    public GameObject spawnOnDestroyB;    // 预制体B

    void Start()
    {
        UpdateHealthText();
    }

    void Update()
    {
        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + health.ToString("F0");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ZiDan"))
        {
            TakeDamage();
            Destroy(collision.gameObject);
        }
    }

    void TakeDamage()
    {
        health -= bulletDamage;

        if (health <= 0)
        {
            TryDropItem();
            Destroy(gameObject);
        }
    }

    void TryDropItem()
    {
        // 第一步：判断是否掉落物品
        float dropRandom = Random.value;

        if (dropRandom <= dropChance)
        {
            // 第二步：如果掉落，从两个预制体中各50%概率选择
            float itemRandom = Random.value;

            if (itemRandom < 0.5f && spawnOnDestroyA != null)
            {
                Instantiate(spawnOnDestroyA, transform.position, transform.rotation);
            }
            else if (spawnOnDestroyB != null)
            {
                Instantiate(spawnOnDestroyB, transform.position, transform.rotation);
            }
        }
    }
}