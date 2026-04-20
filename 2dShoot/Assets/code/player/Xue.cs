using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Xue : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public TextMesh healthText;
    public float reloadDelay = 2f;

    // 新增功能相关变量（改为浮点数）
    public bool isInLight = false;       // 是否在光照范围内
    public float damagePerSecond = 3f;   // 每秒掉血量（不在光照时）
    public float healPerSecond = 1f;     // 每秒回血量（在光照时）

    private float timer = 0f;            // 计时器

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    void Update()
    {
        // 每秒触发一次生命值变化
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0f;

            if (isInLight)
            {
                // 在光照中：回血
                currentHealth += healPerSecond;
                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }
            }
            else
            {
                // 不在光照中：掉血
                currentHealth -= damagePerSecond;
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    Die();
                }
            }

            UpdateHealthDisplay();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthDisplay();
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        }
    }

    void Die()
    {
        StartCoroutine(ReloadScene());
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("XueBu"))
        {
            currentHealth += 25f;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            UpdateHealthDisplay();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("HPLevel2"))
        {
            maxHealth = 180f;
            currentHealth = 180f;
            UpdateHealthDisplay();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("HPLevel3"))
        {
            maxHealth = 200f;
            currentHealth = 200f;
            UpdateHealthDisplay();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("light"))   // 进入光照区域
        {
            isInLight = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("light"))   // 离开光照区域
        {
            isInLight = false;
        }
    }
}