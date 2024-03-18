using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public void SingleButtonClick()
    {
        GameScene.isMulti = false;
        SceneManager.LoadSceneAsync("GameScene"); //게임 화면으로 이동
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
