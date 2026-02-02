using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Xue : MonoBehaviour

{
    public float maxHealth = 100f;           // 最大血量
    public float currentHealth;              // 当前血量
    public Text healthText;                  // 显示血量的Text组件
    public float reloadDelay = 2f;           // 死亡后重载延迟

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    // 受到伤害
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

    // 更新血量显示
    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        }
    }

    // 死亡处理
    void Die()
    {
        // 延迟后重新加载当前场景
        StartCoroutine(ReloadScene());
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}