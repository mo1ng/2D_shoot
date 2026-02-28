using UnityEngine;
using UnityEngine.InputSystem;

public class playermovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -20f;

    private Keyboard keyboard;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // 三个独立的冻结状态
    private bool isFrozen1 = false; // 1秒冻结
    private bool isFrozen2 = false; // 2秒冻结
    private bool isFrozen3 = false; // 3秒冻结

    private Coroutine freezeCoroutine1;
    private Coroutine freezeCoroutine2;
    private Coroutine freezeCoroutine3;

    void Start()
    {
        keyboard = Keyboard.current;
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("需要CharacterController组件！");
        }
    }

    void Update()
    {
        if (keyboard == null) return;

        // 如果任何一个冻结状态为true，则不能移动
        if (!isFrozen1 && !isFrozen2 && !isFrozen3)
        {
            HandleMovement();
        }

        HandleGravity();
    }

    void HandleMovement()
    {
        // 检查是否在地面
        isGrounded = controller.isGrounded;

        // 处理移动
        Vector3 moveDirection = Vector3.zero;

        if (keyboard.dKey.isPressed) moveDirection += Vector3.right;    // X轴正方向
        if (keyboard.aKey.isPressed) moveDirection += Vector3.left;     // X轴负方向
        if (keyboard.wKey.isPressed) moveDirection += Vector3.forward;  // Z轴正方向
        if (keyboard.sKey.isPressed) moveDirection += Vector3.back;     // Z轴负方向

        // 应用移动
        if (moveDirection != Vector3.zero)
        {
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }

        // 设置朝向
        if (keyboard.dKey.isPressed)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (keyboard.aKey.isPressed)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
        }

        // 处理跳跃
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void HandleGravity()
    {
        // 检查是否在地面
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 轻微的向下力，确保贴地
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;

        // 应用垂直速度
        controller.Move(velocity * Time.deltaTime);
    }

    // 冻结等级1：1秒
    public void FreezeLevel1()
    {
        // 如果已经有冻结协程在运行，先停止它
        if (freezeCoroutine1 != null)
        {
            StopCoroutine(freezeCoroutine1);
        }
        freezeCoroutine1 = StartCoroutine(FreezeCoroutine1(1f));
    }

    // 冻结等级2：2秒
    public void FreezeLevel2()
    {
        if (freezeCoroutine2 != null)
        {
            StopCoroutine(freezeCoroutine2);
        }
        freezeCoroutine2 = StartCoroutine(FreezeCoroutine2(2f));
    }

    // 冻结等级3：3秒
    public void FreezeLevel3()
    {
        if (freezeCoroutine3 != null)
        {
            StopCoroutine(freezeCoroutine3);
        }
        freezeCoroutine3 = StartCoroutine(FreezeCoroutine3(3f));
    }

    // 冻结等级1的协程
    private System.Collections.IEnumerator FreezeCoroutine1(float duration)
    {
        isFrozen1 = true;
        Debug.Log($"玩家被冻结等级1，持续 {duration} 秒！");

        yield return new WaitForSeconds(duration);

        isFrozen1 = false;
        Debug.Log("玩家冻结等级1解冻！");
        freezeCoroutine1 = null;
    }

    // 冻结等级2的协程
    private System.Collections.IEnumerator FreezeCoroutine2(float duration)
    {
        isFrozen2 = true;
        Debug.Log($"玩家被冻结等级2，持续 {duration} 秒！");

        yield return new WaitForSeconds(duration);

        isFrozen2 = false;
        Debug.Log("玩家冻结等级2解冻！");
        freezeCoroutine2 = null;
    }

    // 冻结等级3的协程
    private System.Collections.IEnumerator FreezeCoroutine3(float duration)
    {
        isFrozen3 = true;
        Debug.Log($"玩家被冻结等级3，持续 {duration} 秒！");

        yield return new WaitForSeconds(duration);

        isFrozen3 = false;
        Debug.Log("玩家冻结等级3解冻！");
        freezeCoroutine3 = null;
    }

    // 获取各个冻结等级的属性
    public bool IsFrozen1
    {
        get { return isFrozen1; }
    }

    public bool IsFrozen2
    {
        get { return isFrozen2; }
    }

    public bool IsFrozen3
    {
        get { return isFrozen3; }
    }

    // 兼容旧的IsFrozen属性（任何冻结状态为true时返回true）
    public bool IsFrozen
    {
        get { return isFrozen1 || isFrozen2 || isFrozen3; }
    }

    // 获取当前激活的冻结等级（如果没有冻结返回0）
    public int CurrentFreezeLevel
    {
        get
        {
            if (isFrozen1) return 1;
            if (isFrozen2) return 2;
            if (isFrozen3) return 3;
            return 0;
        }
    }
}