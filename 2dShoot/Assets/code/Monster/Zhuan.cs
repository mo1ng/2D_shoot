using UnityEngine;

public class Zhuan : MonoBehaviour
{
    [Header("目标设置")]
    public string targetTag = "Player"; // 目标物体的Tag
    public float maxAngle = 60f; // 最大偏移角度

    [Header("旋转设置")]
    public float rotationSpeed = 5f; // 旋转速度（平滑跟随）
    public bool smoothRotation = true; // 是否平滑旋转

    private Transform targetTransform;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        // 如果目标丢失，重新查找
        if (targetTransform == null)
        {
            FindPlayer();
            return;
        }

        // 计算从当前物体指向目标的方向
        Vector3 directionToTarget = targetTransform.position - transform.position;
        directionToTarget.y = 0; // 只在水平面上旋转（如果需要包含Y轴，注释这行）

        // 计算当前物体的正轴方向（Z轴正方向）
        Vector3 currentForward = transform.forward;

        // 计算当前方向与目标方向的角度差
        float angleToTarget = Vector3.Angle(currentForward, directionToTarget);

        // 如果角度差大于最大允许角度，限制目标方向
        if (angleToTarget > maxAngle)
        {
            // 将目标方向限制在最大角度范围内
            directionToTarget = Vector3.RotateTowards(
                currentForward,
                directionToTarget,
                maxAngle * Mathf.Deg2Rad,
                0
            );
        }

        // 如果目标方向不是零向量，进行旋转
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            if (smoothRotation)
            {
                // 平滑旋转
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                // 直接旋转
                transform.rotation = targetRotation;
            }
        }
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null)
        {
            targetTransform = player.transform;
        }
    }

    // 在Inspector中显示当前角度（用于调试）
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || targetTransform == null)
            return;

        // 绘制当前Z轴方向
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);

        // 绘制指向玩家的方向
        Vector3 toPlayer = targetTransform.position - transform.position;
        toPlayer.y = 0;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, toPlayer.normalized * 5f);

        // 绘制最大角度范围
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -maxAngle, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, maxAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * 5f);
        Gizmos.DrawRay(transform.position, rightBoundary * 5f);

        // 绘制扇形范围
        int segments = 30;
        float angleStep = maxAngle * 2f / segments;
        Vector3 previousPoint = transform.position + leftBoundary * 5f;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -maxAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Vector3 currentPoint = transform.position + direction * 5f;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}
