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
    public int maxBullets = 90;        // 最大子弹携带量
    public int totalBullets = 30;      // 总子弹数（弹匣+备用）
    public float reloadTime = 1.5f;    // 换弹时间

    [Header("UI组件")]
    public Text ammoText;              // 弹药显示文本（显示弹匣内子弹/总子弹）
    public Text totalAmmoText;         // 总弹药显示文本（显示总子弹/最大子弹）
    public Slider reloadSlider;        // 换弹进度条（可选）
    public GameObject reloadUI;        // 换弹提示UI（可选）

    [Header("音效")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    public AudioClip pickUpSound;      // 捡起子弹音效

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

        // 初始化子弹总数
        UpdateTotalBullets();

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

                // 检查是否有备用弹药
                if (totalBullets > 0)
                {
                    StartReload();
                }
                else
                {
                    Debug.Log("没有备用弹药了！");
                }
            }
        }

        // 换弹输入
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            // 如果弹匣不满、有备用弹药且不在换弹中，开始换弹
            if (bulletsInMagazine < magazineSize && totalBullets > 0 && !isReloading)
            {
                StartReload();
            }
        }
    }

    // 触发器检测捡起子弹
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZiDanBu"))
        {
            PickUpAmmo();
            Destroy(other.gameObject); // 销毁子弹物品
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ZiDanBu"))
        {
            PickUpAmmo();
            Destroy(other.gameObject); // 销毁子弹物品
        }
    }

    void PickUpAmmo()
    {
        // 增加一梭子子弹（一个弹匣的容量）
        int oldTotal = totalBullets;
        int bulletsToAdd = magazineSize; // 一梭子就是弹匣容量

        totalBullets = Mathf.Clamp(totalBullets + bulletsToAdd, 0, maxBullets);

        UpdateTotalBullets();
        UpdateAmmoDisplay(); // 这里也要刷新第一个UI文本

        int added = totalBullets - oldTotal;
        if (added > 0)
        {
            // 播放捡起音效
            PlaySound(pickUpSound);

            Debug.Log($"捡到一梭子弹！增加{added}发，当前备用: {totalBullets}/{maxBullets}");

            // 如果弹匣是空的，自动开始换弹
            if (bulletsInMagazine == 0 && !isReloading)
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
        if (isReloading || bulletsInMagazine >= magazineSize || totalBullets <= 0) return;

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
            ammoText.text = "Reloading...";
    }

    void FinishReload()
    {
        // 计算可以装填的子弹数量
        int bulletsToReload = Mathf.Min(magazineSize - bulletsInMagazine, totalBullets);

        // 装填子弹
        bulletsInMagazine += bulletsToReload;
        totalBullets -= bulletsToReload;

        isReloading = false;

        // 隐藏换弹UI
        if (reloadUI != null) reloadUI.SetActive(false);
        if (reloadSlider != null) reloadSlider.gameObject.SetActive(false);

        // 更新弹药显示
        UpdateAmmoDisplay();
        UpdateTotalBullets();

        Debug.Log($"换弹完成！弹药: {bulletsInMagazine}/{magazineSize}, 备用: {totalBullets}");
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
    public int GetTotalAmmo() => totalBullets;
    public int GetMaxTotalAmmo() => maxBullets;
    public bool IsReloading() => isReloading;

    // 添加弹药（比如捡到弹药包）
    public void AddAmmo(int amount)
    {
        int oldTotal = totalBullets;
        totalBullets = Mathf.Clamp(totalBullets + amount, 0, maxBullets);

        UpdateTotalBullets();
        UpdateAmmoDisplay(); // 这里也要刷新第一个UI文本

        int added = totalBullets - oldTotal;
        if (added > 0)
        {
            Debug.Log($"获得{added}发弹药，当前: {totalBullets}/{maxBullets}");
        }
    }

    // 添加总弹药显示UI的方法（可以在编辑器中调用）
    public void SetTotalAmmoUI(Text uiText)
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