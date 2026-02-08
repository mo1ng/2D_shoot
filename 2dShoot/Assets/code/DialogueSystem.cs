using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [System.Serializable]
    public class DialogueItem
    {
        public string characterName;
        [TextArea(3, 10)]
        public string dialogueContent;
        public float displayTime = 2f;
        public float charDelay = 0.05f;
    }

    [Header("UI组件")]
    public GameObject dialoguePanel;  // 对话面板
    public Text nameText;             // 人物名字文本（旧的Text）
    public Text contentText;          // 对话内容文本（旧的Text）

    [Header("对话设置")]
    public DialogueItem[] dialogues;  // 对话集
    public bool playOnStart = true;   // 开始时播放
    public bool canSkip = true;       // 可以跳过
    public bool autoAdvance = true;   // 自动播放下一条

    private int currentIndex = 0;     // 当前对话索引
    private bool isTyping = false;    // 是否正在打字
    private Coroutine typingCoroutine; // 打字协程

    void Start()
    {
        if (playOnStart && dialogues != null && dialogues.Length > 0)
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if (canSkip && Input.GetMouseButtonDown(0))
        {
            SkipDialogue();
        }
    }

    // 开始对话
    public void StartDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("没有设置对话内容！");
            return;
        }

        dialoguePanel.SetActive(true);
        currentIndex = 0;
        ShowNextDialogue();
    }

    // 显示下一句对话
    void ShowNextDialogue()
    {
        if (currentIndex >= dialogues.Length)
        {
            EndDialogue();
            return;
        }

        DialogueItem current = dialogues[currentIndex];
        nameText.text = current.characterName;

        // 停止之前的打字协程
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // 开始新的打字协程
        typingCoroutine = StartCoroutine(TypeText(current));
    }

    // 打字机效果
    IEnumerator TypeText(DialogueItem item)
    {
        isTyping = true;
        contentText.text = ""; // 清空内容

        // 逐个字符显示
        foreach (char letter in item.dialogueContent.ToCharArray())
        {
            contentText.text += letter;
            yield return new WaitForSeconds(item.charDelay);
        }

        isTyping = false;

        // 如果自动播放下一条，等待显示时间后继续
        if (autoAdvance)
        {
            yield return new WaitForSeconds(item.displayTime);
            currentIndex++;
            ShowNextDialogue();
        }
    }

    // 跳过对话
    void SkipDialogue()
    {
        if (!dialoguePanel.activeSelf) return;

        if (isTyping)
        {
            // 正在打字：立即显示完整文本
            StopCoroutine(typingCoroutine);
            contentText.text = dialogues[currentIndex].dialogueContent;
            isTyping = false;
        }
        else
        {
            // 已显示完整：显示下一句
            currentIndex++;
            ShowNextDialogue();
        }
    }

    // 手动显示下一句（当autoAdvance为false时使用）
    public void NextDialogue()
    {
        if (!dialoguePanel.activeSelf || isTyping) return;

        currentIndex++;
        ShowNextDialogue();
    }

    // 结束对话
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Debug.Log("对话结束");
    }

    // ========== 外部调用方法 ==========

    // 设置新的对话集
    public void SetDialogues(DialogueItem[] newDialogues)
    {
        dialogues = newDialogues;
        currentIndex = 0;
    }

    // 添加单条对话
    public void AddDialogue(string name, string content, float displayTime = 2f, float charDelay = 0.05f)
    {
        DialogueItem newItem = new DialogueItem
        {
            characterName = name,
            dialogueContent = content,
            displayTime = displayTime,
            charDelay = charDelay
        };

        // 创建新数组并添加对话
        if (dialogues == null)
        {
            dialogues = new DialogueItem[] { newItem };
        }
        else
        {
            DialogueItem[] temp = new DialogueItem[dialogues.Length + 1];
            dialogues.CopyTo(temp, 0);
            temp[dialogues.Length] = newItem;
            dialogues = temp;
        }
    }

    // 清空所有对话
    public void ClearDialogues()
    {
        dialogues = new DialogueItem[0];
        currentIndex = 0;
    }

    // 检查是否正在对话
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeSelf;
    }

    // 获取当前对话进度
    public string GetCurrentProgress()
    {
        return $"当前对话: {currentIndex + 1}/{dialogues.Length}";
    }
}