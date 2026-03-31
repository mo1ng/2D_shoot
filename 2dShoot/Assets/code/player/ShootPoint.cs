using UnityEngine;
using UnityEngine.InputSystem;

public class ShootPoint : MonoBehaviour

{
    public Camera mainCamera;
    public string floorTag = "Floor";
    public LayerMask floorLayer; // 使用LayerMask更高效

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // 如果没有设置LayerMask，自动根据tag设置
        if (floorLayer == 0 && !string.IsNullOrEmpty(floorTag))
        {
            int layer = LayerMask.NameToLayer(floorTag);
            if (layer != -1)
                floorLayer = 1 << layer;
        }
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 使用LayerMask进行射线检测
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayer))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}