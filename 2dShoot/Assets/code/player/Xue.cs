using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Xue : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public TextMesh healthText;
    public float reloadDelay = 2f;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
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
            currentHealth += 25;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            UpdateHealthDisplay();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("HPLevel2"))
        {
            maxHealth = 150f;
            currentHealth = 150f;
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
    }
}