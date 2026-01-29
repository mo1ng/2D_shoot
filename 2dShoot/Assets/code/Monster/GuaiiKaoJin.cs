using UnityEngine;
using System.Collections;
public class GuaiiKaoJin : MonoBehaviour


{
    [Header("基础设置")]
    public float moveSpeed = 5f;
    public float stopDistance = 2f;
    public string targetTag = "Player";
    public bool movementEnabled = true;

    [Header("爆炸设置")]
    public float explosionRadius = 5f;
    public float playerPushForce = 15f;
    public float pushDuration = 0.3f;
    public float prepareTime = 1.5f;

    [Header("颜色渐变效果")]
    public GameObject colorChangeObject; // 公开需要变色的物体
    public Color startColor = Color.white;
    public Color warningColor = Color.red;
    public float pulseSpeed = 4f;

    private Transform target;
    private bool hasExploded = false;
    private float prepareTimer = 0f;
    private Material material;
    private Vector3 originalScale;

    void Start()
    {
        // 如果没有指定变色物体，就用自身
        if (colorChangeObject == null)
        {
            colorChangeObject = gameObject;
        }

        // 获取材质
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

        if (distance > stopDistance&& movementEnabled==true)
        {
            // 靠近玩家
            transform.position += (target.position - transform.position).normalized * moveSpeed * Time.deltaTime;
        }
        else if (!hasExploded)
        {
            // 更新准备爆炸效果
            prepareTimer += Time.deltaTime;
            UpdateVisualEffect();

            if (prepareTimer >= prepareTime)
            {
                Explode();
            }
        }
    }

    void UpdateVisualEffect()
    {
        if (material != null && colorChangeObject != null)
        {
            float t = prepareTimer / prepareTime;
            movementEnabled = false;
            // 颜色渐变
            material.color = Color.Lerp(startColor, warningColor, t);

            // 脉冲缩放效果
            float pulse = Mathf.Sin(t * Mathf.PI * pulseSpeed) * 0.15f * t + 1f;
            colorChangeObject.transform.localScale = originalScale * pulse;
        }
    }

    void Explode()
    {
        hasExploded = true;
        

        // 最终变成红色
        if (material != null)
        {
            material.color = warningColor;
        }

        Debug.Log("爆炸！");

        // 推开附近物体
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

    void OnDrawGizmosSelected()
    {
        // 爆炸范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // 停止距离
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}