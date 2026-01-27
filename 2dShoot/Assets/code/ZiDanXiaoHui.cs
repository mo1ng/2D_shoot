using UnityEngine;

public class ZiDanXiaoHui : MonoBehaviour

{
    void OnCollisionEnter(Collision collision)
    {
        // 碰到任何物体都销毁自己
        Destroy(gameObject);
    }
}
