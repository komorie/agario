using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public void SingleButtonClick()
    {
        GameScene.IsMulti = false;
        SceneManager.LoadSceneAsync("GameScene"); //게임 화면으로 이동
    }

    public void MultiButtonClick()
    {
        GameScene.IsMulti = true;
        SceneManager.LoadSceneAsync("GameScene"); //게임 화면으로 이동
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
