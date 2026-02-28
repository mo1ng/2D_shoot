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

        // 当可以移动时，根据玩家位置设置朝向
        if (distance > stopDistance && canMove)
        {
            // 设置怪物朝向：玩家在左侧时y=0，在右侧时y=-180
            if (target.position.x > transform.position.x)
            {
                // 玩家在右侧，朝向右边（y = 0）
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                // 玩家在左侧，朝向左边（y = -180）
                transform.rotation = Quaternion.Euler(0, -180, 0);
            }

            // 向玩家移动
            transform.position += (target.position - transform.position).normalized * moveSpeed * Time.deltaTime;
        }
        else if (!hasExploded)
        {
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
            material.color = Color.Lerp(startColor, warningColor, t);
            colorChangeObject.transform.localScale = originalScale * (Mathf.Sin(t * Mathf.PI * 4f) * 0.15f * t + 1f);

            // 当开始准备爆炸时，禁止移动
            if (t > 0)
            {
                canMove = false;
            }
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
}