using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Настройки сцены")]
    [SerializeField] private string gameSceneName = "GameScene"; // Имя сцены с игрой

    // Вызывается при нажатии на кнопку "В БОЙ"
    public void OnPlayButtonPressed()
    {
        Debug.Log("Начало игры");
        SceneManager.LoadScene(gameSceneName);
    }

    // Дополнительный метод для выхода из игры (потребуется в будущем)
    public void OnExitButtonPressed()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
}