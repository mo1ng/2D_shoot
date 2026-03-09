using UnityEngine;

public class UIfollow : MonoBehaviour
{
    [Header("目标设置")]
    [SerializeField] private GameObject targetObject;      // 要显示/隐藏的目标物体
    [SerializeField] private Transform followTransform;    // 要跟随位置的物体

    [Header("位置偏移")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    [Header("显示设置")]
    [SerializeField] private bool startHidden = true;       // 开始时是否隐藏

    [Header("平滑跟随")]
    [SerializeField] private bool enableSmoothFollow = true; // 是否启用平滑跟随
    [SerializeField] private float smoothTime = 0.1f;        // 平滑跟随时间

    [Header("旋转锁定")]
    [SerializeField] private bool lockRotation = true;       // 是否锁定旋转
    [SerializeField] private Vector3 fixedRotation = Vector3.zero; // 固定的旋转角度

    [Header("距离检测")]
    [SerializeField] private float showDistance = 10f;       // 显示距离
    [SerializeField] private float checkInterval = 0.2f;     // 检测间隔

    private bool isVisible = false;
    private Transform playerTransform;
    private float checkTimer;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 smoothPosition;
    private Quaternion originalRotation;

    void Start()
    {
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 初始隐藏
        if (targetObject != null && startHidden)
        {
            targetObject.SetActive(false);
            isVisible = false;
        }
        else
        {
            isVisible = true;
        }

        // 初始化
        if (targetObject != null)
        {
            smoothPosition = targetObject.transform.position;
            originalRotation = targetObject.transform.rotation;
        }
    }

    void Update()
    {
        // Tab键控制（按下切换）
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isVisible = !isVisible;
            UpdateVisibility();
        }

        // 如果可见，进行跟随和距离检测
        if (isVisible)
        {
            // 更新位置（带平滑效果）
            UpdatePosition();

            // 应用旋转锁定
            if (lockRotation && targetObject != null)
            {
                targetObject.transform.rotation = Quaternion.Euler(fixedRotation);
            }

            // 距离检测
            if (playerTransform != null)
            {
                checkTimer += Time.deltaTime;
                if (checkTimer >= checkInterval)
                {
                    checkTimer = 0f;
                    UpdateVisibility();
                }
            }
        }
    }

    void UpdatePosition()
    {
        if (targetObject == null || followTransform == null) return;

        Vector3 targetPosition = followTransform.position + positionOffset;

        if (enableSmoothFollow)
        {
            // 平滑跟随
            smoothPosition = Vector3.SmoothDamp(
                smoothPosition,
                targetPosition,
                ref currentVelocity,
                smoothTime
            );
            targetObject.transform.position = smoothPosition;
        }
        else
        {
            // 直接跟随
            targetObject.transform.position = targetPosition;
            smoothPosition = targetPosition;
        }
    }

    void UpdateVisibility()
    {
        if (targetObject == null) return;

        bool shouldShow = isVisible;

        // 距离判断
        if (shouldShow && playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, targetObject.transform.position);
            shouldShow = dist <= showDistance;
        }

        if (targetObject.activeSelf != shouldShow)
        {
            targetObject.SetActive(shouldShow);

            // 显示时重置位置和旋转
            if (shouldShow && followTransform != null)
            {
                smoothPosition = followTransform.position + positionOffset;
                targetObject.transform.position = smoothPosition;

                if (lockRotation)
                {
                    targetObject.transform.rotation = Quaternion.Euler(fixedRotation);
                }
            }
        }
    }

    // 公共方法
    public void Show()
    {
        isVisible = true;
        UpdateVisibility();
    }

    public void Hide()
    {
        isVisible = false;
        UpdateVisibility();
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        UpdateVisibility();
    }

    public void SetFixedRotation(Vector3 rotation) => fixedRotation = rotation;
}