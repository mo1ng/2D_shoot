using UnityEngine;
using System.Collections.Generic;

public class DiLeiJinGu : MonoBehaviour

{
    [Header("禁锢设置")]
    public float immobilizeDuration = 3f;   // 禁锢持续时间
    public string enemyTag = "Enemy";       // 敌人标签

    [Header("效果范围")]
    public float effectRadius = 5f;         // 作用范围半径

    [Header("触发效果")]
    public GameObject triggerObject;        // 触发时显示的对象
    public ParticleSystem particleEffect;   // 粒子效果

    [Header("刚体设置")]
    public bool useGravity = true;          // 是否使用重力
    public bool isKinematic = false;        // 是否运动学

    private bool hasActivated = false;      // 是否已经激活
    private Rigidbody rb;                   // 自身刚体
    private int activeEffectsCount = 0;     // 当前激活的效果数量
    private bool shouldDestroy = false;     // 是否需要销毁

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

        // 隐藏触发物体（如果需要）
        if (triggerObject != null)
        {
            triggerObject.SetActive(false);
        }

        // 停止粒子效果（如果已设置）
        if (particleEffect != null)
        {
            particleEffect.Stop();
        }
    }

    void Update()
    {
        // 检查是否需要销毁（当所有效果都结束后且标记为需要销毁时）
        if (shouldDestroy && activeEffectsCount <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 使用物理碰撞检测
        if (!hasActivated && collision.gameObject.CompareTag(enemyTag))
        {
            ActivateImmobilizeEffect();
        }
    }

    void ActivateImmobilizeEffect()
    {
        if (hasActivated) return;
        hasActivated = true;

        // 显示触发物体
        if (triggerObject != null)
        {
            triggerObject.SetActive(true);
        }

        // 播放粒子效果
        if (particleEffect != null)
        {
            particleEffect.Play();
        }

        // 在作用范围内查找所有敌人
        Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius);
        int affectedEnemies = 0;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                // 增加活跃效果计数
                activeEffectsCount++;
                affectedEnemies++;

                // 开始禁锢效果协程
                StartCoroutine(ImmobilizeEnemy(col.gameObject, immobilizeDuration));
            }
        }

        Debug.Log($"禁锢了 {affectedEnemies} 个敌人，持续 {immobilizeDuration} 秒");

        if (affectedEnemies > 0)
        {
            // 标记需要销毁，但会等到所有效果结束后
            shouldDestroy = true;

            // 禁用自身的碰撞体和渲染器，但保持物体存在
            DisableSelfComponents();
        }
        else
        {
            // 如果没有影响到任何敌人，直接销毁
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator ImmobilizeEnemy(GameObject enemy, float duration)
    {
        if (enemy == null) yield break;

        // 保存敌人的原始位置
        Vector3 originalPosition = enemy.transform.position;

        // 禁锢开始时间
        float startTime = Time.time;

        // 禁锢循环：持续锁定敌人的位置
        while (Time.time - startTime < duration)
        {
            if (enemy != null)
            {
                // 强制锁定敌人的位置到原始位置
                enemy.transform.position = originalPosition;
            }
            else
            {
                // 如果敌人被销毁了，跳出循环
                break;
            }
            yield return null;
        }

        // 禁锢结束后，减少活跃效果计数
        activeEffectsCount--;

        if (enemy != null)
        {
            Debug.Log($"解除敌人禁锢: {enemy.name}");
        }
    }

    void DisableSelfComponents()
    {
        // 禁用碰撞体
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // 禁用渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        // 禁用刚体
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // 如果有子物体，也禁用它们的渲染器
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer childRenderer in childRenderers)
        {
            childRenderer.enabled = false;
        }
    }

    // 手动触发禁锢效果（可选）
    public void ManualActivate()
    {
        if (!hasActivated)
        {
            ActivateImmobilizeEffect();
        }
    }

    // 在场景视图中可视化作用范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}