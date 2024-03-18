using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public void SingleButtonClick()
    {
        GameScene.isMulti = false;
        SceneManager.LoadSceneAsync("GameScene"); //���� ȭ������ �̵�
    }

    public void MultiButtonClick()
    {
        GameScene.isMulti = true;
        Instantiate(Resources.Load<GameObject>("Prefabs/IPInputUI"));
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
