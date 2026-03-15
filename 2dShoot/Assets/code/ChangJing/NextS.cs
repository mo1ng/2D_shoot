using UnityEngine;
using UnityEngine.SceneManagement;

public class NextS : MonoBehaviour
{
    [Header("场景设置")]
    public int sceneBuildIndex = 1;           // 场景构建索引（在Build Settings中的序号）
    public string sceneName = "";             // 或者使用场景名称（优先使用名称）

    /// <summary>
    /// 加载场景的方法
    /// </summary>
    public void LoadScene()
    {
        // 优先使用场景名称
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // 使用场景索引
            SceneManager.LoadScene(sceneBuildIndex);
        }
    }

    /// <summary>
    /// 带延迟的加载（可选）
    /// </summary>
    public void LoadSceneWithDelay(float delay)
    {
        Invoke(nameof(LoadScene), delay);
    }
}