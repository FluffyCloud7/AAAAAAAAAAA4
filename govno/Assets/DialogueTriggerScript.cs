using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueCharacter
{ public string name;
    public Sprite Icon;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string Line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialoguelines = new List<DialogueLine>();
}

public class DialogueTriggerScript : MonoBehaviour 
{
    public Dialogue dialogue;
}
