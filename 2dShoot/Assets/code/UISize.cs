using UnityEngine;
using UnityEngine.UI;

public class UISize : MonoBehaviour
{
    public int targetWidth = 800;
    public int targetHeight = 600;

    void Start()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(targetWidth, targetHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
        }
    }
}