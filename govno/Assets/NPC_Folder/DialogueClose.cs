using UnityEngine;

public class DialogueCloseButton : MonoBehaviour
{
    public void ClickClose()
    {
        // Вызываем закрытие у того NPC, который сейчас активен
        NPC.CloseActiveDialogue();
    }
}