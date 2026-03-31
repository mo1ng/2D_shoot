using UnityEngine;

public class ZiDanXiaoHui : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // 如果碰撞到的物体tag是"UI"，则忽略碰撞，不销毁也不做任何处理
        if (collision.gameObject.CompareTag("UI")|| collision.gameObject.CompareTag("Player"))
        {
            // 可选：让子弹穿过UI物体
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            return;
        }

        // 碰到其他任何物体都销毁自己
        Destroy(gameObject);
    }
}