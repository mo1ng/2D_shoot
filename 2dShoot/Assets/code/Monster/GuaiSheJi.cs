using UnityEngine;
using System.Collections;

public class GuaiSheJi : MonoBehaviour
{
    [Header("目标设置")]
    public string targetTag = "Player";

    [Header("移动设置")]
    public float moveSpeed = 3f;
    public float stopAndShootThreshold = 0.5f; // 停止移动和允许射击的阈值（相同数值）

    [Header("射击设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 15f;

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

        // 1. 向Player的Z轴移动，直到达到阈值
        MoveToTargetZ();

        // 2. 根据Player在左还是右设置方向
        SetDirection();

        // 3. 检查是否在阈值范围内（允许射击）
        CheckShootingCondition();

        // 4. 射击（只在阈值范围内）
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
            fireTimer = 0f; // 不在射击范围时重置计时器
        }
    }

    void MoveToTargetZ()
    {
        float zDiff = Mathf.Abs(transform.position.z - target.position.z);

        // 只有在距离大于阈值时才移动
        if (zDiff > stopAndShootThreshold)
        {
            Vector3 newPos = transform.position;
            newPos.z = Mathf.MoveTowards(newPos.z, target.position.z, moveSpeed * Time.deltaTime);
            transform.position = newPos;

            // 调试信息：正在移动
            if (Time.frameCount % 60 == 0)
                Debug.Log($"正在向Z轴移动，当前差值: {zDiff:F2}，阈值: {stopAndShootThreshold}");
        }
        else
        {
            // 已经进入阈值范围，停止移动
            if (Time.frameCount % 60 == 0)
                Debug.Log($"已达到阈值范围，停止移动，差值: {zDiff:F2}");
        }
    }

    void SetDirection()
    {
        // 判断Player在左边还是右边
        if (target.position.x > transform.position.x)
        {
            // Player在右边
            transform.eulerAngles = new Vector3(0, 0, 0);
            shootDirection = Vector3.right;
        }
        else
        {
            // Player在左边
            transform.eulerAngles = new Vector3(0, 180, 0);
            shootDirection = Vector3.left;
        }
    }

    void CheckShootingCondition()
    {
        // 检查Z轴差值是否在阈值内
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

        // 创建子弹
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // 添加Rigidbody组件
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        // 关闭重力，避免子弹下落
        rb.useGravity = false;

        // 给子弹一个速度
        rb.velocity = shootDirection * bulletSpeed;

        // 设置子弹朝向
        bullet.transform.forward = shootDirection;

        // 添加碰撞销毁组件
        bullet.AddComponent<DestroyOnCollision>();

        // 5秒后自动销毁
        Destroy(bullet, 5f);

        Debug.Log($"射击！方向: {shootDirection}，速度: {bulletSpeed}");
    }

    void CreateFirePoint()
    {
        GameObject point = new GameObject("FirePoint");
        point.transform.SetParent(transform);
        point.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
        firePoint = point.transform;
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
            canShoot = false; // 离开范围后不能射击
            target = null;
            Debug.Log("Player离开范围");
        }
    }

    void OnDrawGizmosSelected()
    {
        // 显示停止移动和射击的阈值范围
        if (target != null)
        {
            Gizmos.color = canShoot ? Color.green : Color.yellow;
            Vector3 center = transform.position;
            Vector3 lineStart = center - Vector3.forward * stopAndShootThreshold;
            Vector3 lineEnd = center + Vector3.forward * stopAndShootThreshold;
            Gizmos.DrawLine(lineStart, lineEnd);

            // 绘制阈值范围的方框
            float boxSize = stopAndShootThreshold * 2;
            Gizmos.DrawWireCube(center, new Vector3(2f, 2f, boxSize));
        }

        // 显示发射方向
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, shootDirection * 3f);
        }
    }
}

// 子弹碰撞销毁脚本
public class DestroyOnCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}