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
    public int magazineSize = 15;      // 弹匣容量
    public int bulletsInMagazine = 15; // 当前弹匣子弹数
    public float reloadTime = 1.5f;    // 换弹时间

    [Header("UI组件")]
    public Text ammoText;              // 弹药显示文本
    public Slider reloadSlider;        // 换弹进度条（可选）
    public GameObject reloadUI;        // 换弹提示UI（可选）

    [Header("音效")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    private Mouse mouse;
    private Keyboard keyboard;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 初始化UI
        UpdateAmmoDisplay();

        // 隐藏换弹UI（如果有）
        if (reloadUI != null) reloadUI.SetActive(false);
        if (reloadSlider != null) reloadSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        // 处理换弹进度
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;

            // 更新换弹进度条
            if (reloadSlider != null)
            {
                reloadSlider.value = reloadTimer / reloadTime;
            }

            // 检查换弹是否完成
            if (reloadTimer >= reloadTime)
            {
                FinishReload();
            }

            return; // 换弹时不能射击
        }

        // 射击输入
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (bulletsInMagazine > 0)
            {
                Shoot();
            }
            else
            {
                // 弹匣空了，播放空枪声
                PlaySound(emptySound);
                Debug.Log("弹匣空了！按R换弹");

                // 自动开始换弹
                StartReload();
            }
        }

        // 换弹输入
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            // 如果弹匣不满且不在换弹中，开始换弹
            if (bulletsInMagazine < magazineSize && !isReloading)
            {
                StartReload();
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 消耗子弹
        bulletsInMagazine--;
        UpdateAmmoDisplay();

        // 播放射击音效
        PlaySound(shootSound);

        // 创建子弹
        Vector3 spawnPos = firePoint.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // 确定发射方向
        Vector3 shootDirection = GetShootDirection();

        // 设置子弹移动
        bullet.AddComponent<BulletController>().Setup(shootDirection, bulletSpeed);
        bullet.transform.forward = shootDirection;

        // 自动销毁
        Destroy(bullet, 3f);

        Debug.Log($"射击！剩余弹药: {bulletsInMagazine}/{magazineSize}");
    }

    Vector3 GetShootDirection()
    {
        float yRotation = transform.eulerAngles.y;

        if (Mathf.Approximately(yRotation, 0f) || Mathf.Approximately(yRotation, 360f))
            return Vector3.right;
        else if (Mathf.Approximately(Mathf.Abs(yRotation), 180f))
            return Vector3.left;
        else
            return Vector3.right;
    }

    void StartReload()
    {
        if (isReloading || bulletsInMagazine >= magazineSize) return;

        Debug.Log("开始换弹...");
        isReloading = true;
        reloadTimer = 0f;

        // 播放换弹音效
        PlaySound(reloadSound);

        // 显示换弹UI
        if (reloadUI != null) reloadUI.SetActive(true);
        if (reloadSlider != null)
        {
            reloadSlider.gameObject.SetActive(true);
            reloadSlider.maxValue = reloadTime;
            reloadSlider.value = 0f;
        }

        // 更新弹药显示为换弹中
        if (ammoText != null)
            ammoText.text = "换弹中...";
    }

    void FinishReload()
    {
        // 装满弹匣
        bulletsInMagazine = magazineSize;
        isReloading = false;

        // 隐藏换弹UI
        if (reloadUI != null) reloadUI.SetActive(false);
        if (reloadSlider != null) reloadSlider.gameObject.SetActive(false);

        // 更新弹药显示
        UpdateAmmoDisplay();

        Debug.Log($"换弹完成！弹药: {bulletsInMagazine}/{magazineSize}");
    }

    void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{bulletsInMagazine}/{magazineSize}";
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 公开方法供其他脚本调用
    public int GetCurrentAmmo() => bulletsInMagazine;
    public int GetMaxAmmo() => magazineSize;
    public bool IsReloading() => isReloading;

    // 添加弹药（比如捡到弹药包）
    public void AddAmmo(int amount)
    {
        bulletsInMagazine = Mathf.Clamp(bulletsInMagazine + amount, 0, magazineSize);
        UpdateAmmoDisplay();
        Debug.Log($"获得{amount}发弹药，当前: {bulletsInMagazine}/{magazineSize}");
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