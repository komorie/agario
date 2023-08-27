using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    NetworkManager network;

    public void StartButtonClick()
    {

/*        //confirmuUI를 띄워서 매칭 중이라고 표시하고, 서버에 접속
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmUI"));
        ConfirmUI confirmUI = go.GetComponent<ConfirmUI>();
        confirmUI.Init("매칭 중입니다.");

        //NetworkManager 소환(오브젝트 직접 생성)



*/

        SceneManager.LoadScene("GameScene");
        NetworkManager.Instance.Connect();
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
