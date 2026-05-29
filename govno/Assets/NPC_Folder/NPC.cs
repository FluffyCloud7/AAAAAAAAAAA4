using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            return;
        }

        if (!isDialogueActive)
        {
            StartDialogue();
        }
        else
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        dialoguePanel.SetActive(true);
        Debug.Log(dialoguePanel.activeSelf);

        GamePauseManager.Instance.RequestPause();
        CursorManager.Instance.SetMode(InputMode.Dialogue);

        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);

        // Меняем аватарку на стартовую
        UpdatePortrait();

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            SoundEffectManager.StopVoice(); // Останавливаем звук карандаша при пропуске
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex].text);
            isTyping = false;
        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
            UpdatePortrait(); // Меняем аватарку на следующей строчке
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        // Включаем зацикленный рандомный микс звуков перед началом печати
        if (!string.IsNullOrEmpty(dialogueData.voiceSoundGroupName))
        {
            SoundEffectManager.PlayVoice(dialogueData.voiceSoundGroupName, dialogueData.voicePitch, dialogueData.loopVoiceSound);
        }

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex].text)
        {
            dialogueText.text += letter;

            // Если галочка НЕ стоит (это обычный пикающий звук букв): играем на каждый символ кроме пробелов
            if (!string.IsNullOrEmpty(dialogueData.voiceSoundGroupName) && !dialogueData.loopVoiceSound && letter != ' ')
            {
                SoundEffectManager.PlayVoice(dialogueData.voiceSoundGroupName, dialogueData.voicePitch, false);
            }

            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        // Выключаем звук, когда текст полностью напечатался
        SoundEffectManager.StopVoice();
        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        GamePauseManager.Instance.ReleasePause();
        CursorManager.Instance.SetMode(InputMode.Gameplay);

        StopAllCoroutines();
        SoundEffectManager.StopVoice(); // Жестко глушим звук при закрытии
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
    }

    // Метод переключения аватарок
    private void UpdatePortrait()
    {
        if (dialogueData.dialogueLines.Length > dialogueIndex)
        {
            Sprite currentPortrait = dialogueData.dialogueLines[dialogueIndex].portrait;

            if (currentPortrait != null)
            {
                portraitImage.sprite = currentPortrait;
            }
            else
            {
                portraitImage.sprite = dialogueData.npcPortrait; // Если пусто, берем дефолт
            }
        }
    }
}