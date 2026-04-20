using UnityEngine;
using System.Collections;

public class GuaiiKaoJin : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float stopDistance = 2f;
    public string targetTag = "Player";
    public float explosionRadius = 5f;
    public float playerPushForce = 15f;
    public float pushDuration = 0.3f;
    public float prepareTime = 1.5f;
    public float explosionDamage = 50f;
    public GameObject colorChangeObject;
    public Color startColor = Color.white;
    public Color warningColor = Color.red;

    // 爆炸倒计时TextMesh
    public TextMesh timerText;

    // 新增：旋转设置（Z轴对准玩家）
    [Header("旋转设置（Z轴对准玩家）")]
    public float maxRotationAngle = 60f;  // 最大旋转角度（超过此角度不再继续偏移）
    public float rotationSpeed = 180f;    // 旋转速度（度/秒）
    public bool smoothRotation = true;    // 是否平滑旋转

    private Transform target;
    private bool hasExploded = false;
    private float prepareTimer = 0f;
    private Material material;
    private Vector3 originalScale;
    private bool hasDamagedPlayer = false;
    private bool canMove = true;  // 控制是否可以移动

    void Start()
    {
        if (colorChangeObject == null)
        {
            colorChangeObject = gameObject;
        }

        Renderer renderer = colorChangeObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            if (material != null)
            {
                material.color = startColor;
            }
        }

        originalScale = colorChangeObject.transform.localScale;
    }

    void Update()
    {
        if (target == null || hasExploded) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // 当可以移动时，更新旋转并移动
        if (distance > stopDistance && canMove)
        {
            // 更新旋转（Z轴对准玩家）
            UpdateRotation();

            // 向玩家移动
            transform.position += (target.position - transform.position).normalized * moveSpeed * Time.deltaTime;
        }
        else if (!hasExploded)
        {
            // 到达停止距离，开始准备爆炸
            prepareTimer += Time.deltaTime;
            UpdateVisualEffect();
            UpdateTimerText();

            if (prepareTimer >= prepareTime)
            {
                Explode();
            }
        }
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

    void UpdateVisualEffect()
    {
        if (material != null && colorChangeObject != null)
        {
            float t = prepareTimer / prepareTime;
            material.color = Color.Lerp(startColor, warningColor, t);
            colorChangeObject.transform.localScale = originalScale * (Mathf.Sin(t * Mathf.PI * 4f) * 0.15f * t + 1f);

            // 当开始准备爆炸时，禁止移动
            if (t > 0)
            {
                canMove = false;
            }
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            float remainingTime = prepareTime - prepareTimer;
            timerText.text = "Bomb: " + remainingTime.ToString("F1") + "s";
        }
    }

    void Explode()
    {
        hasExploded = true;
        canMove = false;  // 确保爆炸后不能移动

        if (material != null)
        {
            material.color = warningColor;
        }

        CreateExplosionCollider();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                StartCoroutine(PushPlayer(col.gameObject));
            }
            else if (col.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * playerPushForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject, 1f);
    }

    void CreateExplosionCollider()
    {
        GameObject explosionCollider = new GameObject("ExplosionCollider");
        explosionCollider.transform.SetParent(transform);
        explosionCollider.transform.localPosition = Vector3.zero;

        SphereCollider sphereCollider = explosionCollider.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = explosionRadius;

        MonsterExplosionDamage explosionDamageScript = explosionCollider.AddComponent<MonsterExplosionDamage>();
        explosionDamageScript.damageAmount = explosionDamage;
        explosionDamageScript.playerTag = targetTag;
        explosionDamageScript.sourceObject = this;

        Rigidbody rb = explosionCollider.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void MarkPlayerAsDamaged()
    {
        hasDamagedPlayer = true;
    }

    public bool HasDamagedPlayer()
    {
        return hasDamagedPlayer;
    }

    IEnumerator PushPlayer(GameObject player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null) yield break;

        Vector3 pushDir = (player.transform.position - transform.position).normalized;
        pushDir.y = 0.3f;

        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            float force = Mathf.Lerp(playerPushForce, 0, elapsed / pushDuration);
            cc.Move(pushDir * force * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && target == null)
        {
            target = other.transform;
        }
    }

    // 可视化调试：在Scene视图中显示旋转范围
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

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