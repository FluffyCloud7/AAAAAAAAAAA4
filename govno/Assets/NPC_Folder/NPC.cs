using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public Animator dialogueAnimator;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    // Флаг, который защищает от спама кнопкой закрытия, пока панель улетает
    private bool isClosing;

    public static NPC ActiveNPC { get; private set; }

    public bool CanInteract()
    {
        // Не разрешаем взаимодействовать, если диалог прямо сейчас закрывается
        return !isClosing;
    }

    public void Interact()
    {
        if (dialogueData == null || isClosing) return;

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
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E) && !isClosing)
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        ActiveNPC = this;
        isClosing = false;

        dialoguePanel.SetActive(true);

        if (dialogueAnimator != null)
        {
            dialogueAnimator.SetTrigger("Show");
        }

        GamePauseManager.Instance.RequestPause();
        CursorManager.Instance.SetMode(InputMode.UI);

        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        UpdatePortrait();

        StartCoroutine(StartTypeLineWithDelay());
    }

    IEnumerator StartTypeLineWithDelay()
    {
        // Небольшое ожидание появления панели (подгони под длину Dialogue_In, например 0.15–0.2 сек)
        yield return new WaitForSecondsRealtime(0.2f);
        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isClosing) return;

        if (isTyping)
        {
            StopAllCoroutines();
            SoundEffectManager.StopVoice();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex].text);
            isTyping = false;
        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
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

        if (!string.IsNullOrEmpty(dialogueData.voiceSoundGroupName))
        {
            SoundEffectManager.PlayVoice(dialogueData.voiceSoundGroupName, dialogueData.voicePitch, dialogueData.loopVoiceSound);
        }

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex].text)
        {
            dialogueText.text += letter;

            if (!string.IsNullOrEmpty(dialogueData.voiceSoundGroupName) && !dialogueData.loopVoiceSound && letter != ' ')
            {
                SoundEffectManager.PlayVoice(dialogueData.voiceSoundGroupName, dialogueData.voicePitch, false);
            }

            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

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
        if (isClosing) return; // Если уже закрываемся, ничего не делаем
        isClosing = true;

        if (ActiveNPC == this)
        {
            ActiveNPC = null;
        }

        // Немедленно возвращаем управление игроку и убираем паузу
        GamePauseManager.Instance.ReleasePause();
        CursorManager.Instance.SetMode(InputMode.Gameplay);

        // Останавливаем корутины печати текста и глушим звук
        StopAllCoroutines();
        SoundEffectManager.StopVoice();
        isDialogueActive = false;

        if (dialogueAnimator != null)
        {
            // Запускаем анимацию закрытия
            dialogueAnimator.SetTrigger("Hide");
            // Запускаем корутину, которая выключит панель после окончания анимации
            StartCoroutine(DisablePanelAfterAnimation());
        }
        else
        {
            // Если аниматора нет, выключаем мгновенно, как раньше
            dialogueText.SetText("");
            dialoguePanel.SetActive(false);
            isClosing = false;
        }
    }

    IEnumerator DisablePanelAfterAnimation()
    {
        // Даем анимации закрытия проиграться (подгони под длину клипа Dialogue_Out, например 0.2 сек)
        yield return new WaitForSecondsRealtime(0.2f);

        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        isClosing = false; // Сбрасываем флаг, теперь NPC снова готов к диалогам
    }

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
                portraitImage.sprite = dialogueData.npcPortrait;
            }
        }
    }
}