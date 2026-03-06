using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FangDiLei : MonoBehaviour

{
    [Header("预制体设置")]
    public GameObject bombPrefab;      // 炸弹预制体
    public Transform spawnPoint;       // 生成位置

    [Header("基础属性")]
    public int maxBombs = 3;           // 最大炸弹数量
    public float rechargeTime = 15f;   // 恢复时间（秒）

    [Header("触发设置")]
    public TriggerButton triggerButton = TriggerButton.RightMouse; // 触发按键

    [Header("3D UI设置")]
    public TextMesh bombCount3DText;      // 炸弹数量的3D文本
    public TextMesh recharge3DText;       // 恢复进度的3D文本

    public string countFormat = "{0}/{1}";      // 数量显示格式
    public string rechargeFormat = "{0:P0}";    // 进度显示格式

    // 如果3D文本是模型的一部分，可能需要设置相对位置
    public Vector3 uiOffset = Vector3.zero;      // UI相对位置的偏移量
    public Transform uiParent;                    // UI的父对象（通常是模型本身）

    // 触发按键枚举
    public enum TriggerButton
    {
        RightMouse,    // 鼠标右键
        MiddleMouse    // 鼠标中键
    }

    // 私有变量
    private int currentBombs;
    private float rechargeTimer;
    private List<GameObject> placedBombs = new List<GameObject>();

    void Start()
    {
        currentBombs = maxBombs;
        rechargeTimer = 0f;

        // 初始化3D UI
        Initialize3DUI();
        UpdateCountUI();
        UpdateRechargeUI();

        Debug.Log("炸弹系统初始化完成");
    }

    // 初始化3D UI
    void Initialize3DUI()
    {
        // 如果指定了UI父对象，可以设置UI的相对位置
        if (uiParent != null)
        {
            if (bombCount3DText != null)
            {
                bombCount3DText.transform.SetParent(uiParent);
                bombCount3DText.transform.localPosition = uiOffset;
            }
            if (recharge3DText != null)
            {
                recharge3DText.transform.SetParent(uiParent);
                // 可以设置不同的偏移量，这里让恢复文本在数量文本下方
                recharge3DText.transform.localPosition = uiOffset + new Vector3(0, -0.5f, 0);
            }
        }
    }

    void Update()
    {
        // 检测按键输入
        if (CheckTriggerButton() && currentBombs > 0)
        {
            PlaceBomb();
        }

        // 处理炸弹恢复
        HandleBombRecharge();

        // 实时更新恢复进度UI
        if (currentBombs < maxBombs)
        {
            UpdateRechargeUI();
        }

        // 确保3D UI始终面向摄像机（如果需要）
        // FaceCamera();
    }

    // 如果需要UI始终面向摄像机（像世界空间画布一样）
    void FaceCamera()
    {
        if (bombCount3DText != null && Camera.main != null)
        {
            bombCount3DText.transform.rotation = Quaternion.LookRotation(
                bombCount3DText.transform.position - Camera.main.transform.position
            );
        }
        if (recharge3DText != null && Camera.main != null)
        {
            recharge3DText.transform.rotation = Quaternion.LookRotation(
                recharge3DText.transform.position - Camera.main.transform.position
            );
        }
    }

    // 检测触发按键
    bool CheckTriggerButton()
    {
        if (Mouse.current == null) return false;

        switch (triggerButton)
        {
            case TriggerButton.RightMouse:
                return Mouse.current.rightButton.wasPressedThisFrame;

            case TriggerButton.MiddleMouse:
                return Mouse.current.middleButton.wasPressedThisFrame;

            default:
                return false;
        }
    }

    void HandleBombRecharge()
    {
        if (currentBombs < maxBombs)
        {
            rechargeTimer += Time.deltaTime;

            if (rechargeTimer >= rechargeTime)
            {
                currentBombs++;
                rechargeTimer = 0f;
                UpdateCountUI();
                UpdateRechargeUI();
                Debug.Log($"炸弹已恢复，当前数量: {currentBombs}/{maxBombs}");
            }
        }
        else
        {
            rechargeTimer = 0f;
        }
    }

    void PlaceBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogError("请设置炸弹预制体！");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject newBomb = Instantiate(bombPrefab, position, Quaternion.identity);
        placedBombs.Add(newBomb);

        currentBombs--;

        UpdateCountUI();
        UpdateRechargeUI();

        Debug.Log($"安放炸弹成功，剩余数量: {currentBombs}/{maxBombs}");
    }

    // 更新数量UI
    void UpdateCountUI()
    {
        if (bombCount3DText != null)
        {
            bombCount3DText.text = string.Format(countFormat, currentBombs, maxBombs);

            // 根据数量改变文本颜色
            if (currentBombs == 0)
                bombCount3DText.color = Color.red;
            else if (currentBombs < maxBombs / 2)
                bombCount3DText.color = Color.yellow;
            else
                bombCount3DText.color = Color.green;
        }
    }

    // 更新恢复进度UI
    void UpdateRechargeUI()
    {
        if (recharge3DText != null)
        {
            if (currentBombs >= maxBombs)
            {
                // 炸弹已满
                recharge3DText.text = "FULL";
                recharge3DText.color = Color.green;
            }
            else
            {
                // 显示恢复进度百分比
                float progress = GetRechargeProgress();
                recharge3DText.text = string.Format(rechargeFormat, progress);

                // 根据进度改变颜色
                if (progress < 0.33f)
                    recharge3DText.color = Color.red;
                else if (progress < 0.66f)
                    recharge3DText.color = Color.yellow;
                else
                    recharge3DText.color = Color.green;
            }
        }
    }

    // 获取恢复进度（0-1）
    float GetRechargeProgress()
    {
        if (currentBombs >= maxBombs) return 0f;
        return Mathf.Clamp01(rechargeTimer / rechargeTime);
    }

    // ========== 公开方法 ==========

    public void AddBomb(int amount = 1)
    {
        currentBombs = Mathf.Clamp(currentBombs + amount, 0, maxBombs);
        UpdateCountUI();
        UpdateRechargeUI();
    }

    public void RemoveBomb(int amount = 1)
    {
        currentBombs = Mathf.Clamp(currentBombs - amount, 0, maxBombs);
        UpdateCountUI();
        UpdateRechargeUI();
    }

    public void ResetBombs()
    {
        currentBombs = maxBombs;
        rechargeTimer = 0f;
        UpdateCountUI();
        UpdateRechargeUI();
    }

    public int GetCurrentBombs()
    {
        return currentBombs;
    }

    public void SetMaxBombs(int newMax)
    {
        maxBombs = newMax;
        currentBombs = Mathf.Min(currentBombs, maxBombs);
        UpdateCountUI();
        UpdateRechargeUI();
    }

    public void SetRechargeTime(float newTime)
    {
        rechargeTime = Mathf.Max(0.1f, newTime);
    }

    public void DestroyAllBombs()
    {
        foreach (GameObject bomb in placedBombs)
        {
            if (bomb != null) Destroy(bomb);
        }
        placedBombs.Clear();
    }

    // 显示/隐藏3D UI
    public void Show3DUI(bool show)
    {
        if (bombCount3DText != null)
            bombCount3DText.gameObject.SetActive(show);
        if (recharge3DText != null)
            recharge3DText.gameObject.SetActive(show);
    }

    // 设置UI相对位置
    public void SetUIPosition(Vector3 position)
    {
        if (bombCount3DText != null)
            bombCount3DText.transform.localPosition = position;
        if (recharge3DText != null)
            recharge3DText.transform.localPosition = position + new Vector3(0, -0.5f, 0);
    }

    void LateUpdate()
    {
        placedBombs.RemoveAll(bomb => bomb == null);
    }
}