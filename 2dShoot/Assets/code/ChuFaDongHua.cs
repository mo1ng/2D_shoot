using UnityEngine;

public class ChuFaDongHua : MonoBehaviour

{
    public GameObject targetObject;
    public string boolName = "ISOK";
    public float destroyDelay = 0.1f; // 延迟销毁时间

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 设置动画参数
            if (targetObject != null)
            {
                Animator animator = targetObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool(boolName, true);
                }
            }

            // 延迟销毁自身
            Destroy(gameObject, destroyDelay);
        }
    }
}