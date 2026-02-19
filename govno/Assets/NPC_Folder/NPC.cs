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
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            //И сюда бы паузу въебать во вторую часть или
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

        //if (isDialogueActive)
        //{
        //    NextLine();
        //}
       // else
        //{
        //    StartDialogue();
        //}
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
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);

        //Сюда бы паузу въебать...

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if(dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
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

        foreach(char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            //SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicePitch);
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }
        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
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
        //И тут паузу, но тока отжать а
    }
}