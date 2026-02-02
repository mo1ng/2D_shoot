using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;  // 重要：添加这个引用
public class FangDiLei : MonoBehaviour

{
    [Header("预制体设置")]
    public GameObject bombPrefab;      // 炸弹预制体
    public Transform spawnPoint;       // 生成位置
    
    [Header("数量限制")]
    public int maxBombs = 3;           // 最大炸弹数量
    public float rechargeTime = 15f;   // 回复时间（秒）
    
    [Header("UI设置")]
    public Text bombCountText;         // 炸弹数量Text
    public Text rechargeText;          // 回复进度Text（新增）
    public string countFormat = "地雷: {0}/{1}";      // 数量显示格式
    public string rechargeFormat = "回复: {0:P0}";    // 进度显示格式
    
    // 私有变量
    private int currentBombs;
    private float rechargeTimer;
    private List<GameObject> placedBombs = new List<GameObject>();

    void Start()
    {
        currentBombs = maxBombs;
        rechargeTimer = 0f;
        
        // 初始化UI
        UpdateCountUI();
        UpdateRechargeUI();
        
        Debug.Log("地雷系统初始化完成");
    }

    void Update()
    {
        // 检测鼠标右键（新版Input System写法）
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && currentBombs > 0)
        {
            PlaceBomb();
        }
        
        // 处理炸弹回复
        HandleBombRecharge();
        
        // 实时更新回复进度UI
        if (currentBombs < maxBombs)
        {
            UpdateRechargeUI();
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
                Debug.Log($"炸弹回复！当前数量: {currentBombs}/{maxBombs}");
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
            Debug.LogError("错误：炸弹预制体未设置！");
            return;
        }
        
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject newBomb = Instantiate(bombPrefab, position, Quaternion.identity);
        placedBombs.Add(newBomb);
        
        currentBombs--;
        
        UpdateCountUI();
        UpdateRechargeUI();
        
        Debug.Log($"放置地雷成功！剩余数量: {currentBombs}/{maxBombs}");
    }
    
    // 更新数量UI
    void UpdateCountUI()
    {
        if (bombCountText != null)
        {
            bombCountText.text = string.Format(countFormat, currentBombs, maxBombs);
            
            // 根据数量改变颜色
            if (currentBombs == 0)
                bombCountText.color = Color.red;
            else if (currentBombs < maxBombs / 2)
                bombCountText.color = Color.yellow;
            else
                bombCountText.color = Color.green;
        }
    }
    
    // 更新回复进度UI（新增）
    void UpdateRechargeUI()
    {
        if (rechargeText != null)
        {
            if (currentBombs >= maxBombs)
            {
                // 炸弹已满
                rechargeText.text = "已满";
                rechargeText.color = Color.green;
            }
            else
            {
                // 计算进度百分比
                float progress = GetRechargeProgress();
                rechargeText.text = string.Format(rechargeFormat, progress);
                
                // 根据进度改变颜色
                if (progress < 0.33f)
                    rechargeText.color = Color.red;
                else if (progress < 0.66f)
                    rechargeText.color = Color.yellow;
                else
                    rechargeText.color = Color.green;
            }
        }
    }
    
    // 获取回复进度（0-1）
    float GetRechargeProgress()
    {
        if (currentBombs >= maxBombs) return 0f;
        return Mathf.Clamp01(rechargeTimer / rechargeTime);
    }
    
    // ========== 公共方法 ==========
    
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
    
    void LateUpdate()
    {
        placedBombs.RemoveAll(bomb => bomb == null);
    }
    
}
