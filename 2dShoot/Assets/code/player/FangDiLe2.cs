using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FangDiLe2 : MonoBehaviour

{
    [Header("Ԥ��������")]
    public GameObject bombPrefab;      // ը��Ԥ����
    public Transform spawnPoint;       // ����λ��

    [Header("��������")]
    public int maxBombs = 3;           // ���ը������
    public float rechargeTime = 15f;   // �ظ�ʱ�䣨�룩

    [Header("��������")]
    public TriggerButton triggerButton = TriggerButton.RightMouse; // ��������

    [Header("UI����")]
    public Text bombCountText;         // ը������Text
    public Text rechargeText;          // �ظ�����Text
    public string countFormat = "{0}/{1}";      // ������ʾ��ʽ
    public string rechargeFormat = "{0:P0}";    // ������ʾ��ʽ

    // ��������ö��
    public enum TriggerButton
    {
        RightMouse,    // ����Ҽ�
        MiddleMouse    // ����м�
    }

    // ˽�б���
    private int currentBombs;
    private float rechargeTimer;
    private List<GameObject> placedBombs = new List<GameObject>();

    void Start()
    {
        currentBombs = maxBombs;
        rechargeTimer = 0f;

        // ��ʼ��UI
        UpdateCountUI();
        UpdateRechargeUI();

        Debug.Log("����ϵͳ��ʼ�����");
    }

    void Update()
    {
        // ��ⰴ������
        if (CheckTriggerButton() && currentBombs > 0)
        {
            PlaceBomb();
        }

        // ����ը���ظ�
        HandleBombRecharge();

        // ʵʱ���»ظ�����UI
        if (currentBombs < maxBombs)
        {
            UpdateRechargeUI();
        }
    }

    // ��ⴥ������
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
                Debug.Log($"ը���ظ�����ǰ����: {currentBombs}/{maxBombs}");
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
            Debug.LogError("����ը��Ԥ����δ���ã�");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject newBomb = Instantiate(bombPrefab, position, Quaternion.identity);
        placedBombs.Add(newBomb);

        currentBombs--;

        UpdateCountUI();
        UpdateRechargeUI();

        Debug.Log($"���õ��׳ɹ���ʣ������: {currentBombs}/{maxBombs}");
    }

    // ��������UI
    void UpdateCountUI()
    {
        if (bombCountText != null)
        {
            bombCountText.text = string.Format(countFormat, currentBombs, maxBombs);

            // ���������ı���ɫ
            if (currentBombs == 0)
                bombCountText.color = Color.red;
            else if (currentBombs < maxBombs / 2)
                bombCountText.color = Color.yellow;
            else
                bombCountText.color = Color.green;
        }
    }

    // ���»ظ�����UI
    void UpdateRechargeUI()
    {
        if (rechargeText != null)
        {
            if (currentBombs >= maxBombs)
            {
                // ը������
                rechargeText.text = "FULL";
                rechargeText.color = Color.green;
            }
            else
            {
                // ������Ȱٷֱ�
                float progress = GetRechargeProgress();
                rechargeText.text = string.Format(rechargeFormat, progress);

                // ���ݽ��ȸı���ɫ
                if (progress < 0.33f)
                    rechargeText.color = Color.red;
                else if (progress < 0.66f)
                    rechargeText.color = Color.yellow;
                else
                    rechargeText.color = Color.green;
            }
        }
    }

    // ��ȡ�ظ����ȣ�0-1��
    float GetRechargeProgress()
    {
        if (currentBombs >= maxBombs) return 0f;
        return Mathf.Clamp01(rechargeTimer / rechargeTime);
    }

    // ========== �������� ==========

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