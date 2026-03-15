using UnityEngine;
public class HunDisplay : MonoBehaviour
{
    public int Hun;
    public TextMesh text;

    void Start()
    {
        UpdateDisplay();
    }
    void Update()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        text.text = "Soul: " + Hun.ToString();
    }
}