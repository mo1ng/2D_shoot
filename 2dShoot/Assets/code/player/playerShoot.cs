using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AdvancedShooter : MonoBehaviour
{
    [Header("�ӵ�����")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;

    [Header("��ҩϵͳ")]
    public int magazineSize = 15;      
    public int bulletsInMagazine = 15;
    public int maxBullets = 90;        
    public int totalBullets = 30;      
    public float reloadTime = 1.5f;    

    [Header("UI���")]
    public TextMesh ammoText;              
    public TextMesh totalAmmoText;        
    public Slider reloadSlider;        
    public GameObject reloadUI;        
    [Header("��Ч")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    public AudioClip pickUpSound;      

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
                PlaySound(emptySound);
                
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
            PlaySound(pickUpSound);

            

            if (bulletsInMagazine == 0 && !isReloading)
            {
                StartReload();
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        bulletsInMagazine--;
        UpdateAmmoDisplay();

        PlaySound(shootSound);

        Vector3 spawnPos = firePoint.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        Vector3 shootDirection = GetShootDirection();

        bullet.AddComponent<BulletController>().Setup(shootDirection, bulletSpeed);
        bullet.transform.forward = shootDirection;

        
        Destroy(bullet, 3f);
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

        isReloading = true;
        reloadTimer = 0f;

        
        PlaySound(reloadSound);

        
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

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
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