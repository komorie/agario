using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    NetworkManager network;

    public void StartButtonClick()
    {

/*        //confirmuUI�� ����� ��Ī ���̶�� ǥ���ϰ�, ������ ����
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmUI"));
        ConfirmUI confirmUI = go.GetComponent<ConfirmUI>();
        confirmUI.Init("��Ī ���Դϴ�.");

        //NetworkManager ��ȯ(������Ʈ ���� ����)



*/

        SceneManager.LoadScene("GameScene");
        NetworkManager.Instance.Connect();
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }
}
