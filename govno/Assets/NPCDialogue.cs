using UnityEngine;
[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]

public class ScriptDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    //автопрокрутка диалога без нажатия клавиш
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
}
