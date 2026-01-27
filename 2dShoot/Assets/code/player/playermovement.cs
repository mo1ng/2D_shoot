using UnityEngine;
using UnityEngine.InputSystem;

public class playermovement : MonoBehaviour

{
    [SerializeField] private float moveSpeed = 5f;
    private Keyboard keyboard;

    void Start()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard == null) return;

        Vector3 moveDirection = Vector3.zero;

        // 收集所有移动输入
        if (keyboard.dKey.isPressed) moveDirection += Vector3.right;    // X轴正方向
        if (keyboard.aKey.isPressed) moveDirection += Vector3.left;     // X轴负方向
        if (keyboard.wKey.isPressed) moveDirection += Vector3.forward;  // Z轴正方向
        if (keyboard.sKey.isPressed) moveDirection += Vector3.back;     // Z轴负方向

        // 优先设置A/D键的朝向
        if (keyboard.dKey.isPressed)
        {
            // 面向X轴正方向
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (keyboard.aKey.isPressed)
        {
            // 面向X轴负方向
            transform.rotation = Quaternion.Euler(0, -180, 0);
        }

        // 应用移动
        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        }
    }
}