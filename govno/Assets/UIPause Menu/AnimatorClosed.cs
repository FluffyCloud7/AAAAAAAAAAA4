using UnityEngine;

public class PauseMenuAnimationEvents : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public void HidePauseMenu()
    {
        pauseMenuUI.SetActive(false);
    }
}