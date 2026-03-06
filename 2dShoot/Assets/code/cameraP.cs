using UnityEngine;

public class cameraP : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float xOffset = 0f;    // X轴偏移
    [SerializeField] private float zOffset = -10f;  // Z轴偏移，默认-10
    [SerializeField] private float smoothSpeed = 5f;  // 平滑跟随速度

    [Header("边缘跟随设置")]
    [SerializeField] private bool enableEdgeFollow = true;  // 是否启用边缘跟随
    [SerializeField] private float edgeThreshold = 2f;      // 边缘阈值，玩家离边缘多少距离时相机开始移动
    [SerializeField] private Vector2 viewportSize = new Vector2(10f, 6f);  // 相机的视野范围大小

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastPlayerPosition;
    private bool isPlayerMoving = false;

    void Start()
    {
        // 自动查找Player
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 计算目标位置的基础值（完全跟随）
        Vector3 baseTargetPosition = new Vector3(
            player.position.x + xOffset,
            transform.position.y,
            player.position.z + zOffset
        );

        Vector3 finalTargetPosition;

        if (enableEdgeFollow)
        {
            // 使用边缘跟随逻辑
            finalTargetPosition = CalculateEdgeFollowPosition(baseTargetPosition);

            // 更新玩家移动状态
            isPlayerMoving = Vector3.Distance(player.position, lastPlayerPosition) > 0.01f;
            if (isPlayerMoving)
            {
                lastPlayerPosition = player.position;
            }
        }
        else
        {
            // 使用原始跟随逻辑
            finalTargetPosition = baseTargetPosition;
        }

        // 平滑移动
        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalTargetPosition,
            ref velocity,
            smoothSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// 计算边缘跟随的目标位置
    /// </summary>
    private Vector3 CalculateEdgeFollowPosition(Vector3 baseTargetPosition)
    {
        // 计算相机中心应该在世界空间中的位置
        Vector3 cameraCenter = new Vector3(
            transform.position.x - xOffset,
            transform.position.y,
            transform.position.z - zOffset
        );

        // 计算玩家相对于相机中心的位置偏移
        Vector3 playerOffset = player.position - cameraCenter;

        // 计算水平和垂直方向上的边界
        float horizontalHalf = viewportSize.x * 0.5f;
        float verticalHalf = viewportSize.y * 0.5f;

        // 初始化目标偏移为0（相机不动）
        Vector3 targetOffset = Vector3.zero;

        // 检查水平方向是否超出边缘
        if (playerOffset.x > horizontalHalf - edgeThreshold)
        {
            // 玩家太靠右，相机需要向右移动
            targetOffset.x = playerOffset.x - (horizontalHalf - edgeThreshold);
        }
        else if (playerOffset.x < -horizontalHalf + edgeThreshold)
        {
            // 玩家太靠左，相机需要向左移动
            targetOffset.x = playerOffset.x + (horizontalHalf - edgeThreshold);
        }

        // 检查垂直方向是否超出边缘（基于Z轴）
        if (playerOffset.z > verticalHalf - edgeThreshold)
        {
            // 玩家太靠上（Z轴正方向），相机需要向上移动
            targetOffset.z = playerOffset.z - (verticalHalf - edgeThreshold);
        }
        else if (playerOffset.z < -verticalHalf + edgeThreshold)
        {
            // 玩家太靠下（Z轴负方向），相机需要向下移动
            targetOffset.z = playerOffset.z + (verticalHalf - edgeThreshold);
        }

        // 返回调整后的目标位置
        return new Vector3(
            cameraCenter.x + targetOffset.x + xOffset,
            transform.position.y,
            cameraCenter.z + targetOffset.z + zOffset
        );
    }

    // 可视化辅助，在Scene视图中显示边缘范围
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // 计算相机中心位置
        Vector3 cameraCenter = Application.isPlaying ?
            new Vector3(transform.position.x - xOffset, player.position.y, transform.position.z - zOffset) :
            new Vector3(player.position.x, player.position.y, player.position.z);

        // 绘制边缘范围
        Gizmos.color = Color.yellow;
        Vector3 rectSize = new Vector3(viewportSize.x, 0.5f, viewportSize.y);
        Gizmos.DrawWireCube(cameraCenter, rectSize);

        // 绘制触发边缘
        Gizmos.color = Color.green;
        Vector3 triggerSize = new Vector3(viewportSize.x - edgeThreshold * 2, 0.5f, viewportSize.y - edgeThreshold * 2);
        Gizmos.DrawWireCube(cameraCenter, triggerSize);

        // 添加文字说明
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(cameraCenter + Vector3.up * 2, "相机视野范围");
        UnityEditor.Handles.Label(cameraCenter + Vector3.up * 1.5f, $"边缘阈值: {edgeThreshold}");
#endif
    }

    // 可选：可以在编辑器中实时调整偏移
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && player != null && !enableEdgeFollow)
        {
            Vector3 targetPosition = new Vector3(
                player.position.x + xOffset,
                transform.position.y,
                player.position.z + zOffset
            );
            transform.position = targetPosition;
        }
    }
#endif
}