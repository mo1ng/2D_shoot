using UnityEngine;

public class cameraP : MonoBehaviour

{
    [SerializeField] private Transform player;
    [SerializeField] private float xOffset = 0f;    // X轴偏移
    [SerializeField] private float zOffset = -10f;  // Z轴偏移，默认-10
    [SerializeField] private float smoothSpeed = 5f;  // 平滑跟随速度

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // 自动查找Player
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 计算目标位置：保持自己的Y轴，同步X和Z轴并加上偏移
        Vector3 targetPosition = new Vector3(
            player.position.x + xOffset,
            transform.position.y,  // 保持自己原来的Y轴
            player.position.z + zOffset
        );

        // 平滑移动
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothSpeed * Time.deltaTime
        );
    }

    // 可选：可以在编辑器中实时调整偏移
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && player != null)
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