using UnityEngine;
using System.Collections;

public class GuaiSheJi : MonoBehaviour
{
    public string targetTag = "Player";

    public float moveSpeed = 3f;
    public float stopAndShootThreshold = 0.5f;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 15f;

    public TextMesh fireRateText;

    private Transform target;
    private bool isTargetInRange = false;
    private bool canShoot = false;
    private float fireTimer = 0f;
    private Vector3 shootDirection = Vector3.right;

    void Start()
    {
        if (firePoint == null)
        {
            CreateFirePoint();
        }
    }

    void Update()
    {
        if (!isTargetInRange || target == null) return;

        MoveToTargetZ();

        SetDirection();

        CheckShootingCondition();

        if (canShoot)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= 1f / fireRate)
            {
                FireBullet();
                fireTimer = 0f;
            }
        }
        else
        {
            fireTimer = 0f;
        }

        UpdateFireRateText();
    }

    void MoveToTargetZ()
    {
        float zDiff = Mathf.Abs(transform.position.z - target.position.z);

        if (zDiff > stopAndShootThreshold)
        {
            Vector3 newPos = transform.position;
            newPos.z = Mathf.MoveTowards(newPos.z, target.position.z, moveSpeed * Time.deltaTime);
            transform.position = newPos;

            if (Time.frameCount % 60 == 0)
                Debug.Log($"正在Z轴移动，当前差值: {zDiff:F2}，阈值: {stopAndShootThreshold}");
        }
        else
        {
            if (Time.frameCount % 60 == 0)
                Debug.Log($"已达到阈值范围，停止移动，差值: {zDiff:F2}");
        }
    }

    void SetDirection()
    {
        if (target.position.x > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            shootDirection = Vector3.right;
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            shootDirection = Vector3.left;
        }
    }

    void CheckShootingCondition()
    {
        float zDiff = Mathf.Abs(transform.position.z - target.position.z);
        canShoot = zDiff <= stopAndShootThreshold;
    }

    void FireBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("未设置子弹预制体！");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("未设置发射点！");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;

        rb.linearVelocity = shootDirection * bulletSpeed;

        bullet.transform.forward = shootDirection;

        bullet.AddComponent<DestroyOnCollision>();

        Destroy(bullet, 5f);

        Debug.Log($"发射子弹，方向: {shootDirection}，速度: {bulletSpeed}");
    }

    void CreateFirePoint()
    {
        GameObject point = new GameObject("FirePoint");
        point.transform.SetParent(transform);
        point.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
        firePoint = point.transform;
    }

    void UpdateFireRateText()
    {
        if (fireRateText != null)
        {
            fireRateText.text = "Fire Rate: " + fireRate.ToString("F1") + "/s";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            target = other.transform;
            isTargetInRange = true;
            Debug.Log("检测到Player进入范围");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            isTargetInRange = false;
            canShoot = false;
            target = null;
            Debug.Log("Player离开范围");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = canShoot ? Color.green : Color.yellow;
            Vector3 center = transform.position;
            Vector3 lineStart = center - Vector3.forward * stopAndShootThreshold;
            Vector3 lineEnd = center + Vector3.forward * stopAndShootThreshold;
            Gizmos.DrawLine(lineStart, lineEnd);

            float boxSize = stopAndShootThreshold * 2;
            Gizmos.DrawWireCube(center, new Vector3(2f, 2f, boxSize));
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, shootDirection * 3f);
        }
    }
}

public class DestroyOnCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}