using UnityEngine;

public class WallHide : MonoBehaviour
{
    public GameObject upWall;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && upWall != null)
        {
            DisableAllRenderers();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && upWall != null)
        {
            DisableAllRenderers();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && upWall != null)
        {
            EnableAllRenderers();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && upWall != null)
        {
            EnableAllRenderers();
        }
    }

    private void DisableAllRenderers()
    {
        Renderer[] renderers = upWall.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    private void EnableAllRenderers()
    {
        Renderer[] renderers = upWall.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }
}