using UnityEngine;

public class GuaiXue : MonoBehaviour
{
    // 基础属性
    public float health = 100f;          // 当前血量
    public float bulletDamage = 20f;     // 每次受子弹伤害值

    // 子弹时间（慢动作）效果参数
    public float slowTimeScale = 0.2f;   // 慢动作时的游戏速度（0-1之间）
    public float slowDuration = 0.5f;    // 慢动作持续时间（秒）

    // 白屏效果参数
    public Color whiteColor = Color.white; // 白色颜色值

    // 内部状态变量
    private float originalTimeScale;     // 原始游戏速度（用于恢复）
    private bool isSlowing = false;      // 是否正在慢动作状态
    private float slowTimer = 0f;        // 慢动作剩余时间计时器
    private SpriteRenderer[] childRenderers; // 所有子物体的SpriteRenderer组件
    private Color[] originalColors;      // 原始颜色值数组（用于恢复）

    void Start()
    {
        // 记录原始游戏速度，用于效果结束后恢复
        originalTimeScale = Time.timeScale;

        // 获取所有子物体的SpriteRenderer组件
        childRenderers = GetComponentsInChildren<SpriteRenderer>();

        // 初始化原始颜色数组
        if (childRenderers != null && childRenderers.Length > 0)
        {
            originalColors = new Color[childRenderers.Length];

            // 保存所有子物体的原始颜色
            for (int i = 0; i < childRenderers.Length; i++)
            {
                if (childRenderers[i] != null)
                {
                    originalColors[i] = childRenderers[i].color;
                }
            }
        }
    }

    void Update()
    {
        // 如果处于慢动作状态，更新计时器
        if (isSlowing)
        {
            // 使用不受时间缩放影响的增量时间，确保计时准确
            slowTimer -= Time.unscaledDeltaTime;

            // 计时结束，恢复所有效果
            if (slowTimer <= 0)
            {
                RestoreEffects();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 检测碰撞物体是否是子弹（通过标签判断）
        if (collision.gameObject.CompareTag("ZiDan"))
        {
            TakeDamage();                    // 受到伤害
            Destroy(collision.gameObject);   // 销毁子弹对象
        }
    }

    void TakeDamage()
    {
        // 扣除血量
        health -= bulletDamage;

        // 应用所有受击效果（慢动作+白屏）
        ApplyHitEffects();

        // 检查是否死亡
        if (health <= 0)
        {
            RestoreEffects(); // 恢复效果后再销毁
            Destroy(gameObject);  // 销毁自身游戏对象
        }
    }

    void ApplyHitEffects()
    {
        // 应用慢动作效果
        Time.timeScale = slowTimeScale;
        slowTimer = slowDuration;
        isSlowing = true;

        // 应用白屏效果：将所有子物体的SpriteRenderer变为白色
        if (childRenderers != null)
        {
            for (int i = 0; i < childRenderers.Length; i++)
            {
                if (childRenderers[i] != null)
                {
                    childRenderers[i].color = whiteColor;
                }
            }
        }
    }

    void RestoreEffects()
    {
        // 恢复游戏速度
        Time.timeScale = originalTimeScale;
        isSlowing = false;

        // 恢复所有子物体的原始颜色
        if (childRenderers != null && originalColors != null)
        {
            for (int i = 0; i < childRenderers.Length; i++)
            {
                if (childRenderers[i] != null && i < originalColors.Length)
                {
                    childRenderers[i].color = originalColors[i];
                }
            }
        }
    }

    void OnDestroy()
    {
        // 对象被销毁时，恢复所有效果
        // 防止对象销毁后游戏仍保持慢速
        if (isSlowing)
        {
            Time.timeScale = originalTimeScale;
        }
    }
}