using UnityEngine;

public class EToSwitch : MonoBehaviour

{
    public GameObject P;
    public MonoBehaviour targetScript;  // 指定要控制的脚本
    public GameObject W;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 禁用P的特定脚本
            if (targetScript != null)
                targetScript.enabled = false;

            // 显示W
            if (W != null) W.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 启用P的特定脚本
            if (targetScript != null)
                targetScript.enabled = true;

            // 隐藏W
            if (W != null) W.SetActive(false);
        }
    }
}