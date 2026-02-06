using UnityEngine;

public class ZiXuan : MonoBehaviour

{
    [Header("旋转设置")]
    public float rotationSpeed = 30f;      // 旋转速度（度/秒）
    public Vector3 rotationAxis = Vector3.up; // 旋转轴，默认绕Y轴旋转

    [Header("漂浮设置")]
    public float floatHeight = 0.5f;       // 漂浮高度范围
    public float floatSpeed = 1f;          // 漂浮速度
    public bool useSineWave = true;        // 使用正弦波还是简单的上下移动

    private Vector3 startPosition;         // 初始位置
    private float timer = 0f;              // 计时器

    void Start()
    {
        // 记录初始位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 处理旋转
        HandleRotation();

        // 处理漂浮
        HandleFloating();
    }

    void HandleRotation()
    {
        // 绕指定轴旋转
        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime);
    }

    void HandleFloating()
    {
        timer += Time.deltaTime * floatSpeed;

        float yOffset;

        if (useSineWave)
        {
            // 使用正弦波实现平滑的上下漂浮
            yOffset = Mathf.Sin(timer) * floatHeight * 0.5f;
        }
        else
        {
            // 使用简单的三角波
            yOffset = Mathf.PingPong(timer, floatHeight) - (floatHeight * 0.5f);
        }

        // 应用漂浮效果
        Vector3 newPosition = startPosition + Vector3.up * yOffset;
        transform.position = newPosition;
    }

    // 在编辑器中可视化漂浮范围
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector3 topPoint = startPosition + Vector3.up * floatHeight * 0.5f;
            Vector3 bottomPoint = startPosition - Vector3.up * floatHeight * 0.5f;

            Gizmos.DrawLine(topPoint, bottomPoint);
            Gizmos.DrawSphere(topPoint, 0.05f);
            Gizmos.DrawSphere(bottomPoint, 0.05f);
        }
        else
        {
            Gizmos.color = Color.green;
            Vector3 topPoint = transform.position + Vector3.up * floatHeight * 0.5f;
            Vector3 bottomPoint = transform.position - Vector3.up * floatHeight * 0.5f;

            Gizmos.DrawLine(topPoint, bottomPoint);
            Gizmos.DrawSphere(topPoint, 0.05f);
            Gizmos.DrawSphere(bottomPoint, 0.05f);
        }
    }
}