using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public void SingleButtonClick()
    {
        GameScene.IsMulti = false;
        SceneManager.LoadSceneAsync("GameScene"); //���� ȭ������ �̵�
    }

    public void MultiButtonClick()
    {
        GameScene.IsMulti = true;
        SceneManager.LoadSceneAsync("GameScene"); //���� ȭ������ �̵�
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
