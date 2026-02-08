using UnityEngine;

[System.Serializable]
public class DialogueItem
{
    public string characterName;
    [TextArea(3, 10)]
    public string dialogueContent;
    public float displayTime = 2f;
    public float charDelay = 0.05f;

    // 可选构造函数
    public DialogueItem(string name, string content, float time = 2f, float delay = 0.05f)
    {
        characterName = name;
        dialogueContent = content;
        displayTime = time;
        charDelay = delay;
    }
}