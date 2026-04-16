using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AdvancedShooter : MonoBehaviour
{
    [Header("子弹设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;

    [Header("弹药系统")]
    public int magazineSize = 15;
    public int bulletsInMagazine = 15;
    public int maxBullets = 90;
    public int totalBullets = 30;
    public float reloadTime = 1.5f;

    [Header("UI显示")]
    public TextMesh ammoText;
    public TextMesh totalAmmoText;
    public Slider reloadSlider;
    public GameObject reloadUI;

    [Header("射击扣血")]
    public float shootDamageCost = 5f;  // 每次射击扣除的血量

    private Mouse mouse;
    private Keyboard keyboard;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private Xue healthSystem;  // 引用血量系统

    void Start()
    {
        mouse = Mouse.current;
        keyboard = Keyboard.current;

        // 获取血量系统组件
        healthSystem = GetComponent<Xue>();
        if (healthSystem == null)
        {
            Debug.LogWarning("未找到 Xue 血量系统组件，射击扣血功能将无效");
        }

        UpdateTotalBullets();
        UpdateAmmoDisplay();

        if (reloadUI != null) reloadUI.SetActive(false);
        if (reloadSlider != null) reloadSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;

            if (reloadSlider != null)
            {
                reloadSlider.value = reloadTimer / reloadTime;
            }

            if (reloadTimer >= reloadTime)
            {
                FinishReload();
            }

            return;
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (bulletsInMagazine > 0)
            {
                Shoot();
            }
            else
            {
                if (totalBullets > 0)
                {
                    StartReload();
                }
            }
        }

        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            if (bulletsInMagazine < magazineSize && totalBullets > 0 && !isReloading)
            {
                StartReload();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZiDanBu"))
        {
            PickUpAmmo();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ZiDanBu"))
        {
            PickUpAmmo();
            Destroy(other.gameObject);
        }
    }

    void PickUpAmmo()
    {
        int oldTotal = totalBullets;
        int bulletsToAdd = magazineSize;

        totalBullets = Mathf.Clamp(totalBullets + bulletsToAdd, 0, maxBullets);

        UpdateTotalBullets();
        UpdateAmmoDisplay();

        int added = totalBullets - oldTotal;
        if (added > 0)
        {
            if (bulletsInMagazine == 0 && !isReloading)
            {
                StartReload();
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 扣除血量
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(shootDamageCost);
        }

        bulletsInMagazine--;
        UpdateAmmoDisplay();

        // 使用 firePoint 的位置和正Z轴方向
        Vector3 spawnPos = firePoint.position;
        Vector3 shootDirection = firePoint.forward;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // 设置子弹的方向和速度
        BulletController bulletController = bullet.AddComponent<BulletController>();
        bulletController.Setup(shootDirection, bulletSpeed);

        // 可选：让子弹的旋转也指向射击方向
        bullet.transform.forward = shootDirection;

        // 3秒后销毁子弹
        Destroy(bullet, 3f);
    }

    void StartReload()
    {
        if (isReloading || bulletsInMagazine >= magazineSize || totalBullets <= 0) return;

        isReloading = true;
        reloadTimer = 0f;

        if (reloadUI != null) reloadUI.SetActive(true);
        if (reloadSlider != null)
        {
            reloadSlider.gameObject.SetActive(true);
            reloadSlider.maxValue = reloadTime;
            reloadSlider.value = 0f;
        }

        if (ammoText != null)
            ammoText.text = "Reloading...";
    }

    void FinishReload()
    {
        int bulletsToReload = Mathf.Min(magazineSize - bulletsInMagazine, totalBullets);

        bulletsInMagazine += bulletsToReload;
        totalBullets -= bulletsToReload;

        isReloading = false;

        if (reloadUI != null) reloadUI.SetActive(false);
        if (reloadSlider != null) reloadSlider.gameObject.SetActive(false);

        UpdateAmmoDisplay();
        UpdateTotalBullets();
    }

    void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{bulletsInMagazine}/{totalBullets}";
        }
    }

    void UpdateTotalBullets()
    {
        if (totalAmmoText != null)
        {
            totalAmmoText.text = $"{totalBullets}/{maxBullets}";
        }
    }

    public int GetCurrentAmmo() => bulletsInMagazine;
    public int GetMaxAmmo() => magazineSize;
    public int GetTotalAmmo() => totalBullets;
    public int GetMaxTotalAmmo() => maxBullets;
    public bool IsReloading() => isReloading;

    public void AddAmmo(int amount)
    {
        int oldTotal = totalBullets;
        totalBullets = Mathf.Clamp(totalBullets + amount, 0, maxBullets);

        UpdateTotalBullets();
        UpdateAmmoDisplay();
        int added = totalBullets - oldTotal;
    }

    public void SetTotalAmmoUI(TextMesh uiText)
    {
        totalAmmoText = uiText;
        UpdateTotalBullets();
    }
}

public class BulletController : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    public void Setup(Vector3 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}