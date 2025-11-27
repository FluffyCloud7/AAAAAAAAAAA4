using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Кнопка Continue (перетащи её в инспекторе)
    public GameObject continueButton;

    void Start()
    {
        // Если есть сохранённая контрольная точка — показываем кнопку Continue
        if (PlayerPrefs.HasKey("checkpoint_id"))
            continueButton?.SetActive(true);
        else
            continueButton?.SetActive(false);
    }

    // Новая игра — очищает сохранения и запускает первую игровую сцену
    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Продолжить — загрузка следующей сцены, игрок появится в последней контрольной точке
    public void ContinueGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Твоя старая кнопка PLAY (если хочешь, можешь заменить на NewGame)
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}