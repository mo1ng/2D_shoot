using UnityEngine;

public class back : MonoBehaviour
{
    [Tooltip("目标时间缩放值")]
    public float targetTimeScale = 0.3f;

    [Tooltip("渐变持续时间（秒）")]
    public float transitionDuration = 1.3f;

    private float currentTransitionTime = 0f;
    private float startTimeScale;
    private bool isTransitioning = false;

    private void Start()
    {
        // 确保初始时间缩放为1
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // 检查玩家是否按下右键或中键，立即恢复时间缩放
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            Time.timeScale = 1f;
            isTransitioning = false;
            return;
        }

        // 执行时间缩放渐变（从1到0.3，持续1.3秒）
        if (isTransitioning)
        {
            currentTransitionTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(currentTransitionTime / transitionDuration);

            // 从1渐变到0.3
            Time.timeScale = Mathf.Lerp(1f, targetTimeScale, t);

            if (t >= 1f)
            {
                isTransitioning = false;
            }
        }
    }

    // 开始时间缩放渐变
    public void StartTimeScaleTransition()
    {
        currentTransitionTime = 0f;
        isTransitioning = true;
    }

    private void OnDestroy()
    {
        // 销毁时恢复时间缩放
        Time.timeScale = 1f;
    }
}