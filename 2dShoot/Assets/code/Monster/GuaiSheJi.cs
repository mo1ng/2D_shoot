using UnityEngine;
using System.Collections;

public class GuaiSheJi : MonoBehaviour
{
    [Header("Ŀ������")]
    public string targetTag = "Player";

    [Header("�ƶ�����")]
    public float moveSpeed = 3f;
    public float stopAndShootThreshold = 0.5f; // ֹͣ�ƶ��������������ֵ����ͬ��ֵ��

    [Header("�������")]
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

        // 1. ��Player��Z���ƶ���ֱ���ﵽ��ֵ
        MoveToTargetZ();

        // 2. ����Player�����������÷���
        SetDirection();

        // 3. ����Ƿ�����ֵ��Χ�ڣ����������
        CheckShootingCondition();

        // 4. �����ֻ����ֵ��Χ�ڣ�
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
            fireTimer = 0f; // ���������Χʱ���ü�ʱ��
        }
    }

    void MoveToTargetZ()
    {
        float zDiff = Mathf.Abs(transform.position.z - target.position.z);

        // ֻ���ھ��������ֵʱ���ƶ�
        if (zDiff > stopAndShootThreshold)
        {
            Vector3 newPos = transform.position;
            newPos.z = Mathf.MoveTowards(newPos.z, target.position.z, moveSpeed * Time.deltaTime);
            transform.position = newPos;

            // ������Ϣ�������ƶ�
            if (Time.frameCount % 60 == 0)
                Debug.Log($"������Z���ƶ�����ǰ��ֵ: {zDiff:F2}����ֵ: {stopAndShootThreshold}");
        }
        else
        {
            // �Ѿ�������ֵ��Χ��ֹͣ�ƶ�
            if (Time.frameCount % 60 == 0)
                Debug.Log($"�Ѵﵽ��ֵ��Χ��ֹͣ�ƶ�����ֵ: {zDiff:F2}");
        }
    }

    void SetDirection()
    {
        // �ж�Player����߻����ұ�
        if (target.position.x > transform.position.x)
        {
            // Player���ұ�
            transform.eulerAngles = new Vector3(0, 0, 0);
            shootDirection = Vector3.right;
        }
        else
        {
            // Player�����
            transform.eulerAngles = new Vector3(0, 180, 0);
            shootDirection = Vector3.left;
        }
    }

    void CheckShootingCondition()
    {
        // ���Z���ֵ�Ƿ�����ֵ��
        float zDiff = Mathf.Abs(transform.position.z - target.position.z);
        canShoot = zDiff <= stopAndShootThreshold;
    }

    void FireBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("δ�����ӵ�Ԥ���壡");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("δ���÷���㣡");
            return;
        }

        // �����ӵ�
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // ����Rigidbody���
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        // �ر������������ӵ�����
        rb.useGravity = false;

        // ���ӵ�һ���ٶ�
        rb.linearVelocity = shootDirection * bulletSpeed;

        // �����ӵ�����
        bullet.transform.forward = shootDirection;

        // ������ײ�������
        bullet.AddComponent<DestroyOnCollision>();

        // 5����Զ�����
        Destroy(bullet, 5f);

        Debug.Log($"���������: {shootDirection}���ٶ�: {bulletSpeed}");
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
            Debug.Log("��⵽Player���뷶Χ");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            isTargetInRange = false;
            canShoot = false; // �뿪��Χ�������
            target = null;
            Debug.Log("Player�뿪��Χ");
        }
    }

    void OnDrawGizmosSelected()
    {
        // ��ʾֹͣ�ƶ����������ֵ��Χ
        if (target != null)
        {
            Gizmos.color = canShoot ? Color.green : Color.yellow;
            Vector3 center = transform.position;
            Vector3 lineStart = center - Vector3.forward * stopAndShootThreshold;
            Vector3 lineEnd = center + Vector3.forward * stopAndShootThreshold;
            Gizmos.DrawLine(lineStart, lineEnd);

            // ������ֵ��Χ�ķ���
            float boxSize = stopAndShootThreshold * 2;
            Gizmos.DrawWireCube(center, new Vector3(2f, 2f, boxSize));
        }

        // ��ʾ���䷽��
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, shootDirection * 3f);
        }
    }
}

// �ӵ���ײ���ٽű�
public class DestroyOnCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}