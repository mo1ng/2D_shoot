using UnityEngine;
using System.Collections;
public class DuiHua : MonoBehaviour

{
    public string[] texts;
    public float interval = 2f;  // 统一间隔时间

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        TextMesh textMesh = GetComponent<TextMesh>();

        foreach (string text in texts)
        {
            textMesh.text = text;
            yield return new WaitForSeconds(interval);
        }
    }
}