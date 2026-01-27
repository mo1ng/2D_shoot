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

        // 检查是否在地面
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 轻微的向下力，确保贴地
        }

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

        // 应用重力
        velocity.y += gravity * Time.deltaTime;

        // 应用垂直速度
        controller.Move(velocity * Time.deltaTime);
    }
}