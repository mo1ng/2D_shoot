using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [Header("控制物体")]
    public GameObject showObject;      // 要显示的物体
    public GameObject hideObject;      // 要隐藏的物体
    public GameObject rigidbodyObject; // 要添加刚体的物体

    [Header("刚体参数")]
    public float mass = 10f;          // 质量
    public float gravityMultiplier = 10f; // 重力倍数

    // 显示物体
    public void ShowTargetObject()
    {
        if (showObject != null) showObject.SetActive(true);
    }

    // 隐藏物体
    public void HideTargetObject()
    {
        if (hideObject != null) hideObject.SetActive(false);
    }

    // 添加重型刚体（带增强重力）
    public void AddHeavyRigidbody()
    {
        if (rigidbodyObject == null) return;

        // 添加或获取刚体组件
        Rigidbody rb = rigidbodyObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = rigidbodyObject.AddComponent<Rigidbody>();
        }

        // 设置参数
        rb.mass = mass;
        rb.useGravity = false; // 禁用Unity默认重力

        // 移除旧的 HeavyGravity 组件（如果有）
        HeavyGravity oldGravity = rigidbodyObject.GetComponent<HeavyGravity>();
        if (oldGravity != null)
        {
            Destroy(oldGravity);
        }

        // 添加新的自定义重力
        HeavyGravity newGravity = rigidbodyObject.AddComponent<HeavyGravity>();
        newGravity.gravityMultiplier = gravityMultiplier;

        Debug.Log($"物体 {rigidbodyObject.name} 已添加刚体：质量={mass}，重力倍数={gravityMultiplier}");
    }
}

// 自定义重力组件
public class HeavyGravity : MonoBehaviour
{
    public float gravityMultiplier = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // 应用自定义倍数的重力
            Vector3 gravityForce = new Vector3(0, -9.81f * gravityMultiplier, 0);
            rb.AddForce(gravityForce, ForceMode.Acceleration);
        }
    }
}