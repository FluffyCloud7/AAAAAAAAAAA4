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

    // Глобальная ссылка на говорящего в данный момент NPC, чтобы кнопка-крестик знала, кого закрывать
    public static NPC ActiveNPC { get; private set; }

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

    void Update()
    {
        // Если диалог активен, перехватываем нажатие кнопки E для прокрутки или скипа текста
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        // Запоминаем текущего активного NPC
        ActiveNPC = this;

        dialoguePanel.SetActive(true);
        Debug.Log(dialoguePanel.activeSelf);

        GamePauseManager.Instance.RequestPause();

        // Включаем тот самый режим UI, в котором у тебя гарантированно работает кастомный курсор в паузе
        CursorManager.Instance.SetMode(InputMode.UI);

        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);

        // Меняем аватарку на стартовую
        UpdatePortrait();

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
        if (ActiveNPC == this)
        {
            ActiveNPC = null;
        }

        GamePauseManager.Instance.ReleasePause();
        CursorManager.Instance.SetMode(InputMode.Gameplay);

        StopAllCoroutines();
        SoundEffectManager.StopVoice(); // Жестко глушим звук при закрытии
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
    }

    // Этот статический метод дергает скрипт-прослойка DialogueCloseButton, висящий на твоем крестике
    public static void CloseActiveDialogue()
    {
        if (ActiveNPC != null)
        {
            ActiveNPC.EndDialogue();
        }
    }

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