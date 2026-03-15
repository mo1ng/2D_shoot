using UnityEngine;

public class HunCube : MonoBehaviour
{
    public int hunValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HunDisplay hunDisplay = other.GetComponent<HunDisplay>();
            if (hunDisplay != null)
            {
                hunDisplay.Hun += hunValue;
                hunDisplay.UpdateDisplay();
            }
            Destroy(gameObject);
        }
    }
}