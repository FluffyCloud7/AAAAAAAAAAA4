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

        // Устанавливаем аватарку для ПЕРВОЙ строчки
        UpdatePortrait();

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            // ИЗМЕНЕНО: берем .text из структуры
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex].text);
            isTyping = false;
        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
            // Обновляем аватарку при переходе на следующую строчку
            UpdatePortrait();
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

        // ИЗМЕНЕНО: перебираем символы из dialogueData.dialogueLines[dialogueIndex].text
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex].text)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }
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
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
    }

    // НОВЫЙ МЕТОД: Меняет аватарку. Если у строчки нет своей аватарки, ставит общую из дефолта
    private void UpdatePortrait()
    {
        Sprite currentPortrait = dialogueData.dialogueLines[dialogueIndex].portrait;

        if (currentPortrait != null)
        {
            portraitImage.sprite = currentPortrait;
        }
        else
        {
            portraitImage.sprite = dialogueData.npcPortrait; // Запасной вариант
        }
    }
}