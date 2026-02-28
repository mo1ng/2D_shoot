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
    }

    void FixedUpdate()
    {
        if (isChasing && player != null && !playerTooHigh && rb != null)
        {
            // 面向玩家
            if (player.position.x > transform.position.x)
                transform.eulerAngles = new Vector3(0, 0, 0);
            else
                transform.eulerAngles = new Vector3(0, 180, 0);

            ChasePlayer();
        }
        else if (rb != null && !isChasing)
        {
            rb.linearVelocity = Vector3.zero;
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
    }
}