using UnityEngine;
using System.Collections;

public class GuaiZhuangJi : MonoBehaviour
{
    [Header("基础设置")]
    public string playerTag = "Player";
    public float moveSpeed = 3f;
    public float acceleration = 2f;
    public float maxSpeed = 10f;

    [Header("区域设置")]
    public float chaseRadius = 10f;     // 追逐范围半径
    public float pushDistance = 2f;     // 推开距离
    public float maxChaseHeight = 2f;   // 最大追踪高度（Y轴）

    [Header("推开设置")]
    public float pushForce = 15f;      // 推开力度
    public float pushDuration = 0.3f;  // 推开持续时间
    public float pushUpward = 0.3f;    // 向上推力
    public float pushCooldown = 0.5f;  // 推开冷却时间

    private Transform player;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isChasing = false;
    private bool canPush = true;       // 是否可以推开
    private bool playerTooHigh = false; // 玩家是否太高

    void Start()
    {
        currentSpeed = moveSpeed;

        // 获取自身的刚体
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // 查找玩家
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // 检查玩家高度
        CheckPlayerHeight();

        if (playerTooHigh)
        {
            // 玩家太高，停止追踪
            isChasing = false;
            return;
        }

        // 计算水平距离（忽略Y轴）
        float distance = GetHorizontalDistance();

        // 检查是否在追逐范围内
        if (distance <= chaseRadius && !isChasing)
        {
            isChasing = true;
        }

        // 检查是否应该推开
        if (distance <= pushDistance && isChasing && canPush)
        {
            PushPlayer();
        }
    }

    void FixedUpdate()
    {
        if (isChasing && player != null && !playerTooHigh && rb != null)
        {
            // 朝向玩家
            if (player.position.x > transform.position.x)
                transform.eulerAngles = new Vector3(0, 0, 0);
            else
                transform.eulerAngles = new Vector3(0, 180, 0);

            // 追逐玩家
            ChasePlayer();
        }
        else if (rb != null && !isChasing)
        {
            // 不在追逐时停止
            rb.velocity = Vector3.zero;
        }
    }

    // 检查玩家高度
    void CheckPlayerHeight()
    {
        if (player == null) return;

        playerTooHigh = player.position.y > maxChaseHeight;
    }

    // 计算水平距离（忽略Y轴）
    float GetHorizontalDistance()
    {
        if (player == null) return float.MaxValue;

        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);

        return Vector3.Distance(playerPos, myPos);
    }

    void ChasePlayer()
    {
        // 计算水平方向（忽略Y轴）
        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = (playerPos - myPos).normalized;

        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);

        // 使用刚体移动（水平移动）
        Vector3 velocity = new Vector3(direction.x * currentSpeed, rb.velocity.y, direction.z * currentSpeed);
        rb.velocity = velocity;
    }

    // 推开玩家
    void PushPlayer()
    {
        if (!canPush || player == null || playerTooHigh) return;

        canPush = false;

        // 推开玩家
        StartCoroutine(PushPlayerCoroutine());
    }

    IEnumerator PushPlayerCoroutine()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("玩家没有CharacterController组件！");
            canPush = true;
            yield break;
        }

        Vector3 pushDir = (player.position - transform.position).normalized;
        pushDir.y = pushUpward;

        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            float force = Mathf.Lerp(pushForce, 0, elapsed / pushDuration);
            cc.Move(pushDir * force * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 推开完成后，等待冷却时间
        yield return new WaitForSeconds(pushCooldown);
        canPush = true;
    }

    // 查找玩家
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // 物理碰撞检测
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            // 检查高度
            CheckPlayerHeight();
            if (playerTooHigh) return;

            // 碰撞到玩家时也推开
            if (player == null)
            {
                player = collision.transform;
            }

            if (canPush)
            {
                PushPlayer();
            }
        }
    }

    public void ResetState()
    {
        isChasing = false;
        canPush = true;
        playerTooHigh = false;
        currentSpeed = moveSpeed;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }

    // 在场景中显示范围
    void OnDrawGizmosSelected()
    {
        // 追逐范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // 推开范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pushDistance);

        // 高度限制线
        Gizmos.color = Color.blue;
        Vector3 heightLineStart = transform.position + Vector3.up * maxChaseHeight;
        Gizmos.DrawLine(heightLineStart - Vector3.right * 5f, heightLineStart + Vector3.right * 5f);
        Gizmos.DrawLine(heightLineStart - Vector3.forward * 5f, heightLineStart + Vector3.forward * 5f);
    }
}