using UnityEngine;

// Структура, которая объединяет текст строки и её индивидуальную аватарку
[System.Serializable]
public struct DialogueLine
{
    [TextArea(3, 10)]
    public string text;       // Текст конкретной строчки
    public Sprite portrait;   // Аватарка конкретно для этой строчки (если пусто — возьмется дефолтная)
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait; // Дефолтная аватарка NPC

    [Header("Настройки текста")]
    public float typingSpeed = 0.05f;

    [Header("Звук голоса")]
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [Header("Строки диалога (Текст + Эмоция)")]
    public DialogueLine[] dialogueLines; // ЗАМЕНЕНО: теперь это массив структур вместо обычных строк

    [Header("Авто-прогресс")]
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
}