using UnityEngine;

public class IsFrozenShow : MonoBehaviour
{
    [Header("玩家移动脚本引用")]
    public playermovement playerMovement; // 直接拖拽playermovement脚本进来

    [Header("冻结等级对应的显示物体")]
    public GameObject frozenLevel1Object; // 冻结等级1为true时显示
    public GameObject frozenLevel2Object; // 冻结等级2为true时显示
    public GameObject frozenLevel3Object; // 冻结等级3为true时显示

    [Header("调试选项")]
    public bool showDebugLogs = false; // 是否显示调试日志

    private bool lastIsFrozen1 = false;
    private bool lastIsFrozen2 = false;
    private bool lastIsFrozen3 = false;

    // 属性名称常量
    private const string IS_FROZEN_1 = "IsFrozen1";
    private const string IS_FROZEN_2 = "IsFrozen2";
    private const string IS_FROZEN_3 = "IsFrozen3";

    void Start()
    {
        // 检查是否拖拽了playerMovement引用
        if (playerMovement == null)
        {
            Debug.LogError("IsFrozenShow: 请将playermovement脚本拖拽到playerMovement字段！");
            return;
        }

        // 初始化所有物体为隐藏状态
        SetObjectActive(frozenLevel1Object, false);
        SetObjectActive(frozenLevel2Object, false);
        SetObjectActive(frozenLevel3Object, false);

        if (showDebugLogs) Debug.Log("IsFrozenShow 初始化完成");
    }

    void Update()
    {
        if (playerMovement == null) return;

        // 检查冻结等级1的状态
        bool isFrozen1 = GetIsFrozenValue(IS_FROZEN_1);
        if (isFrozen1 != lastIsFrozen1)
        {
            lastIsFrozen1 = isFrozen1;
            SetObjectActive(frozenLevel1Object, isFrozen1);
            if (showDebugLogs) Debug.Log($"冻结等级1状态变为: {isFrozen1}");
        }

        // 检查冻结等级2的状态
        bool isFrozen2 = GetIsFrozenValue(IS_FROZEN_2);
        if (isFrozen2 != lastIsFrozen2)
        {
            lastIsFrozen2 = isFrozen2;
            SetObjectActive(frozenLevel2Object, isFrozen2);
            if (showDebugLogs) Debug.Log($"冻结等级2状态变为: {isFrozen2}");
        }

        // 检查冻结等级3的状态
        bool isFrozen3 = GetIsFrozenValue(IS_FROZEN_3);
        if (isFrozen3 != lastIsFrozen3)
        {
            lastIsFrozen3 = isFrozen3;
            SetObjectActive(frozenLevel3Object, isFrozen3);
            if (showDebugLogs) Debug.Log($"冻结等级3状态变为: {isFrozen3}");
        }
    }

    // 通过反射获取isFrozen属性的值
    private bool GetIsFrozenValue(string propertyName)
    {
        var property = playerMovement.GetType().GetProperty(propertyName);
        if (property != null)
        {
            return (bool)property.GetValue(playerMovement);
        }

        // 如果没有属性，尝试获取字段
        var field = playerMovement.GetType().GetField(propertyName);
        if (field != null)
        {
            return (bool)field.GetValue(playerMovement);
        }

        Debug.LogError($"IsFrozenShow: 在playermovement中找不到 {propertyName}");
        return false;
    }

    // 安全地设置物体显示状态
    private void SetObjectActive(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    // 公开方法：手动刷新显示（可在需要时调用）
    public void RefreshDisplay()
    {
        if (playerMovement == null) return;

        bool isFrozen1 = GetIsFrozenValue(IS_FROZEN_1);
        bool isFrozen2 = GetIsFrozenValue(IS_FROZEN_2);
        bool isFrozen3 = GetIsFrozenValue(IS_FROZEN_3);

        SetObjectActive(frozenLevel1Object, isFrozen1);
        SetObjectActive(frozenLevel2Object, isFrozen2);
        SetObjectActive(frozenLevel3Object, isFrozen3);

        lastIsFrozen1 = isFrozen1;
        lastIsFrozen2 = isFrozen2;
        lastIsFrozen3 = isFrozen3;

        if (showDebugLogs) Debug.Log("手动刷新显示完成");
    }

    // 在Inspector中显示调试信息 - 修复版本
    void OnDrawGizmosSelected()
    {
        if (playerMovement != null)
        {
            // 使用Handles来显示文字，避免GUILayout的布局问题
            UnityEditor.Handles.BeginGUI();

            // 创建GUI样式
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;

            // 直接在屏幕上绘制文字，不使用BeginArea/EndArea
            GUI.Label(new Rect(10, 10, 200, 20), $"Level 1: {lastIsFrozen1}", style);
            GUI.Label(new Rect(10, 30, 200, 20), $"Level 2: {lastIsFrozen2}", style);
            GUI.Label(new Rect(10, 50, 200, 20), $"Level 3: {lastIsFrozen3}", style);

            UnityEditor.Handles.EndGUI();
        }
    }
}