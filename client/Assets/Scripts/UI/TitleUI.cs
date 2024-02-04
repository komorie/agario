using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public void StartButtonClick()
    {
        SceneManager.LoadSceneAsync("GameScene"); //게임 화면으로 이동
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
