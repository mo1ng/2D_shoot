using UnityEngine;

public class GuaiZhuangJi : MonoBehaviour
{
    [Header("追击参数")]
    public string playerTag = "Player";
    public float moveSpeed = 3f;
    public float acceleration = 2f;
    public float maxSpeed = 10f;

    [Header("追击范围")]
    public float chaseRadius = 10f;     // 追击范围半径
    public float stopDistance = 2f;     // 距离玩家多近时停止追击
    public float maxChaseHeight = 2f;   // 最大追击高度（Y轴）

    [Header("冻结参数")]
    public int freezeLevel = 1;          // 冻结等级：1=1秒，2=2秒，3=3秒
    public float freezeDuration = 2f;    // 保留原始冻结时间作为备选

    [Header("销毁设置")]
    public bool destroyOnTouch = true;   // 碰到玩家时是否销毁自身
    public float destroyDelay = 0f;      // 销毁延迟时间（秒）

    [Header("旋转设置（Z轴对准玩家）")]
    public float maxRotationAngle = 60f; // 最大旋转角度（超过此角度不再继续偏移）
    public float rotationSpeed = 180f;   // 旋转速度（度/秒）
    public bool smoothRotation = true;   // 是否平滑旋转

    public TextMesh speedText;           // 显示速度的TextMesh

    private Transform player;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isChasing = false;
    private bool playerTooHigh = false;
    private playermovement playerMovement; // 引用你的玩家移动脚本
    private bool hasTouchedPlayer = false; // 防止重复触发

    void Start()
    {
        currentSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        CheckPlayerHeight();

        if (playerTooHigh)
        {
            isChasing = false;
            return;
        }

        float distance = GetHorizontalDistance();

        // 判断是否在追击范围内
        if (distance <= chaseRadius && !isChasing)
        {
            isChasing = true;
        }

        // 判断是否停止追击（距离小于停止距离）
        if (distance <= stopDistance && isChasing)
        {
            isChasing = false;
        }

        // 更新旋转（Z轴对准玩家）
        if (player != null)
        {
            UpdateRotation();
        }

        // 更新速度显示
        UpdateSpeedText();
    }

    void FixedUpdate()
    {
        if (isChasing && player != null && !playerTooHigh && rb != null)
        {
            ChasePlayer();
        }
        else if (rb != null && !isChasing)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    // 更新旋转：让Z轴正方向对准玩家，但限制最大偏移角度
    void UpdateRotation()
    {
        // 计算从当前物体指向玩家的水平方向
        Vector3 directionToPlayer = player.position - transform.position;
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

    void CheckPlayerHeight()
    {
        if (player == null) return;
        playerTooHigh = player.position.y > maxChaseHeight;
    }

    float GetHorizontalDistance()
    {
        if (player == null) return float.MaxValue;

        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);

        return Vector3.Distance(playerPos, myPos);
    }

    void ChasePlayer()
    {
        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = (playerPos - myPos).normalized;

        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        Vector3 velocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
        rb.linearVelocity = velocity;
    }

    void UpdateSpeedText()
    {
        if (speedText != null)
        {
            speedText.text = "Speed" + currentSpeed.ToString("F1");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasTouchedPlayer)
        {
            CheckPlayerHeight();
            if (playerTooHigh) return;

            if (player == null)
            {
                player = other.transform;
            }

            // 获取玩家身上的PlayerMovement脚本并冻结
            if (playerMovement == null)
            {
                playerMovement = other.GetComponent<playermovement>();
            }

            if (playerMovement != null)
            {
                hasTouchedPlayer = true; // 标记已触碰，防止重复触发

                // 根据freezeLevel调用不同的冻结方法
                switch (freezeLevel)
                {
                    case 1:
                        playerMovement.FreezeLevel1();
                        break;
                    case 2:
                        playerMovement.FreezeLevel2();
                        break;
                    case 3:
                        playerMovement.FreezeLevel3();
                        break;
                    default:
                        // 默认使用level1
                        playerMovement.FreezeLevel1();
                        break;
                }

                // 如果启用了销毁功能，销毁自身
                if (destroyOnTouch)
                {
                    DestroySelf();
                }
            }
        }
    }

    // 销毁自身的方法
    void DestroySelf()
    {
        if (destroyDelay > 0)
        {
            // 如果有延迟，先禁用碰撞器和渲染器，然后延迟销毁
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;

            Destroy(gameObject, destroyDelay);
        }
        else
        {
            // 立即销毁
            Destroy(gameObject);
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<playermovement>();
        }
    }

    public void ResetState()
    {
        isChasing = false;
        playerTooHigh = false;
        currentSpeed = moveSpeed;
        hasTouchedPlayer = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 追击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // 停止距离范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // 高度限制线
        Gizmos.color = Color.blue;
        Vector3 heightLineStart = transform.position + Vector3.up * maxChaseHeight;
        Gizmos.DrawLine(heightLineStart - Vector3.right * 5f, heightLineStart + Vector3.right * 5f);
        Gizmos.DrawLine(heightLineStart - Vector3.forward * 5f, heightLineStart + Vector3.forward * 5f);

        // 绘制旋转范围（扇形）
        if (Application.isPlaying && player != null)
        {
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
}