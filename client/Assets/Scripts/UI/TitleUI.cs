using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public void StartButtonClick()
    {
        SceneManager.LoadSceneAsync("GameScene"); //���� ȭ������ �̵�
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
