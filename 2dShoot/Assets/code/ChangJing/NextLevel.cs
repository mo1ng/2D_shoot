using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [Header("条件设置")]
    public int requiredHunValue = 10;      // 需要的Hun数值
    public string playerTag = "Player";     // 玩家标签

    [Header("动画设置")]
    public Animator targetAnimator;          // 目标动画控制器
    public string animBoolName = "Isok";     // 动画布尔参数名

    [Header("场景设置")]
    public NextS nextS;                       // 引用NextS组件

    private bool hasTriggered = false;        // 防止重复触发

    private void OnTriggerEnter(Collider other)
    {
        // 如果已经触发过，不再处理
        if (hasTriggered) return;

        // 检查碰撞对象是否为玩家
        if (other.CompareTag(playerTag))
        {
            // 获取玩家身上的HunDisplay组件
            HunDisplay hunDisplay = other.GetComponent<HunDisplay>();

            // 检查Hun数值是否达标
            if (hunDisplay != null && hunDisplay.Hun >= requiredHunValue)
            {
                hasTriggered = true;

                // 触发动画
                if (targetAnimator != null)
                {
                    targetAnimator.SetBool(animBoolName, true);
                }
            }
        }
    }

    /// <summary>
    /// 帧事件调用的方法：跳转场景
    /// 在动画事件中调用此方法
    /// </summary>
    public void LoadNextScene()
    {
        if (nextS != null)
        {
            nextS.LoadScene();
        }
    }
}