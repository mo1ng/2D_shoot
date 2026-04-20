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

    // 新增：旋转设置（Z轴对准玩家）
    [Header("旋转设置（Z轴对准玩家）")]
    public float maxRotationAngle = 60f;  // 最大旋转角度（超过此角度不再继续偏移）
    public float rotationSpeed = 180f;    // 旋转速度（度/秒）
    public bool smoothRotation = true;    // 是否平滑旋转

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

        // 更新旋转（Z轴对准玩家）
        UpdateRotation();

        MoveToTargetZ();

        SetShootDirection();

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

    // 更新旋转：让Z轴正方向对准玩家，但限制最大偏移角度
    void UpdateRotation()
    {
        if (target == null) return;

        // 计算从当前物体指向玩家的水平方向
        Vector3 directionToPlayer = target.position - transform.position;
        directionToPlayer.y = 0; // 只在水平面上旋转

        if (directionToPlayer == Vector3.zero)
            return;

        // 获取当前Z轴正方向
        Vector3 currentForward = transform.forward;

        // 计算当前方向与目标方向的角度差
        float angleToPlayer = Vector3.Angle(currentForward, directionToPlayer);

        // 如果角度差超过最大允许角度，限制目标方向
        if (angleToPlayer > maxRotationAngle)
        {
            // 计算旋转方向（左转还是右转）
            float sign = Mathf.Sign(Vector3.Cross(currentForward, directionToPlayer).y);
            // 限制在最大角度范围内
            directionToPlayer = Quaternion.Euler(0, maxRotationAngle * sign, 0) * currentForward;
        }

        // 应用旋转
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            if (smoothRotation)
            {
                // 平滑旋转（使用Slerp）
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime / 90f // 归一化速度
                );
            }
            else
            {
                // 直接旋转
                transform.rotation = targetRotation;
            }
        }
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

    void SetShootDirection()
    {
        // 根据当前物体的朝向来设置射击方向
        // 物体的Z轴正方向就是射击方向
        shootDirection = transform.forward;

        // 确保射击方向是水平方向（Y轴为0）
        shootDirection.y = 0;
        shootDirection.Normalize();
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

        // 子弹沿物体的Z轴方向（前方）发射
        rb.linearVelocity = shootDirection * bulletSpeed;

        // 让子弹的朝向与飞行方向一致
        bullet.transform.forward = shootDirection;

        bullet.AddComponent<DestroyOnCollision>();

        Destroy(bullet, 5f);

        Debug.Log($"发射子弹，方向: {shootDirection}，速度: {bulletSpeed}");
    }

    void CreateFirePoint()
    {
        GameObject point = new GameObject("FirePoint");
        point.transform.SetParent(transform);
        point.transform.localPosition = new Vector3(0, 0, 0.5f); // 放在物体前方
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

        // 绘制旋转范围（扇形）
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.forward;

        // 左边界
        Vector3 leftBoundary = Quaternion.Euler(0, -maxRotationAngle, 0) * forward;
        // 右边界
        Vector3 rightBoundary = Quaternion.Euler(0, maxRotationAngle, 0) * forward;

        Gizmos.DrawRay(transform.position, leftBoundary * 5f);
        Gizmos.DrawRay(transform.position, rightBoundary * 5f);

        // 绘制扇形弧线
        int segments = 20;
        float angleStep = maxRotationAngle * 2f / segments;
        Vector3 previousPoint = transform.position + leftBoundary * 5f;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -maxRotationAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = transform.position + direction * 5f;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
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