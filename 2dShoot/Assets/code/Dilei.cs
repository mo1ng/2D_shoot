using UnityEngine;

public class Dilei : MonoBehaviour
{
    [Header("爆炸设置")]
    public float explosionForce = 50f;      // 爆炸力
    public float explosionRadius = 5f;      // 爆炸范围
    public float upwardForce = 2f;          // 向上的力
    public string enemyTag = "Enemy";       // 敌人标签

    [Header("刚体设置")]
    public bool useGravity = true;          // 是否使用重力
    public bool isKinematic = false;        // 是否运动学

    private bool hasExploded = false;
    private Rigidbody rb;

    void Start()
    {
        // 获取或添加刚体组件
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // 设置刚体属性
        rb.useGravity = useGravity;
        rb.isKinematic = isKinematic;

        // 确保碰撞体不是触发器（使用物理碰撞）
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;  // 重要：设置为非触发器
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 使用物理碰撞检测
        if (!hasExploded && collision.gameObject.CompareTag(enemyTag))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 在爆炸范围内查找所有敌人
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 计算爆炸方向
                    Vector3 explosionDir = (col.transform.position - transform.position).normalized;

                    // 距离衰减
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    float forceMultiplier = Mathf.Clamp01(1 - (distance / explosionRadius));

                    // 施加力
                    Vector3 force = explosionDir * explosionForce * forceMultiplier;
                    force.y += upwardForce * forceMultiplier;
                    rb.AddForce(force, ForceMode.Impulse);

                    Debug.Log($"对 {col.name} 施加爆炸力: {force.magnitude}");
                }
            }
        }

        // 爆炸后销毁
        Destroy(gameObject);
    }

    // 手动触发爆炸（可选）
    public void ManualExplode()
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    // 在场景视图中可视化爆炸范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}