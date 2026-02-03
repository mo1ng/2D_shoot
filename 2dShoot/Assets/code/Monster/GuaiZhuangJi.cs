using UnityEngine;
using System.Collections;

public class GuaiZhuangJi : MonoBehaviour
{
    [Header("��������")]
    public string playerTag = "Player";
    public float moveSpeed = 3f;
    public float acceleration = 2f;
    public float maxSpeed = 10f;

    [Header("��������")]
    public float chaseRadius = 10f;     // ׷��Χ�뾶
    public float pushDistance = 2f;     // �ƿ�����
    public float maxChaseHeight = 2f;   // ���׷�ٸ߶ȣ�Y�ᣩ

    [Header("�ƿ�����")]
    public float pushForce = 15f;      // �ƿ�����
    public float pushDuration = 0.3f;  // �ƿ�����ʱ��
    public float pushUpward = 0.3f;    // ��������
    public float pushCooldown = 0.5f;  // �ƿ���ȴʱ��

    private Transform player;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isChasing = false;
    private bool canPush = true;       // �Ƿ�����ƿ�
    private bool playerTooHigh = false; // ����Ƿ�̫��

    void Start()
    {
        currentSpeed = moveSpeed;

        // ��ȡ�����ĸ���
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // �������
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // �����Ҹ߶�
        CheckPlayerHeight();

        if (playerTooHigh)
        {
            // ���̫�ߣ�ֹͣ׷��
            isChasing = false;
            return;
        }

        // ����ˮƽ���루����Y�ᣩ
        float distance = GetHorizontalDistance();

        // ����Ƿ���׷��Χ��
        if (distance <= chaseRadius && !isChasing)
        {
            isChasing = true;
        }

        // ����Ƿ�Ӧ���ƿ�
        if (distance <= pushDistance && isChasing && canPush)
        {
            PushPlayer();
        }
    }

    void FixedUpdate()
    {
        if (isChasing && player != null && !playerTooHigh && rb != null)
        {
            // �������
            if (player.position.x > transform.position.x)
                transform.eulerAngles = new Vector3(0, 0, 0);
            else
                transform.eulerAngles = new Vector3(0, 180, 0);

            // ׷�����
            ChasePlayer();
        }
        else if (rb != null && !isChasing)
        {
            // ����׷��ʱֹͣ
            rb.linearVelocity = Vector3.zero;
        }
    }

    // �����Ҹ߶�
    void CheckPlayerHeight()
    {
        if (player == null) return;

        playerTooHigh = player.position.y > maxChaseHeight;
    }

    // ����ˮƽ���루����Y�ᣩ
    float GetHorizontalDistance()
    {
        if (player == null) return float.MaxValue;

        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);

        return Vector3.Distance(playerPos, myPos);
    }

    void ChasePlayer()
    {
        // ����ˮƽ���򣨺���Y�ᣩ
        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = (playerPos - myPos).normalized;

        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);

        // ʹ�ø����ƶ���ˮƽ�ƶ���
        Vector3 velocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
        rb.linearVelocity = velocity;
    }

    // �ƿ����
    void PushPlayer()
    {
        if (!canPush || player == null || playerTooHigh) return;

        canPush = false;

        // �ƿ����
        StartCoroutine(PushPlayerCoroutine());
    }

    IEnumerator PushPlayerCoroutine()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("���û��CharacterController�����");
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

        // �ƿ���ɺ󣬵ȴ���ȴʱ��
        yield return new WaitForSeconds(pushCooldown);
        canPush = true;
    }

    // �������
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // ������ײ���
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            // ���߶�
            CheckPlayerHeight();
            if (playerTooHigh) return;

            // ��ײ�����ʱҲ�ƿ�
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
            rb.linearVelocity = Vector3.zero;
        }
    }

    // �ڳ�������ʾ��Χ
    void OnDrawGizmosSelected()
    {
        // ׷��Χ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // �ƿ���Χ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pushDistance);

        // �߶�������
        Gizmos.color = Color.blue;
        Vector3 heightLineStart = transform.position + Vector3.up * maxChaseHeight;
        Gizmos.DrawLine(heightLineStart - Vector3.right * 5f, heightLineStart + Vector3.right * 5f);
        Gizmos.DrawLine(heightLineStart - Vector3.forward * 5f, heightLineStart + Vector3.forward * 5f);
    }
}