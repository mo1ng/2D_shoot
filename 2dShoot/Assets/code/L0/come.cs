using UnityEngine;

public class come : MonoBehaviour
{
    [Tooltip("要显示的公开物体")]
    public GameObject objectToShow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectToShow != null)
            {
                objectToShow.SetActive(true);
            }

            gameObject.SetActive(false);
        }
    }
}