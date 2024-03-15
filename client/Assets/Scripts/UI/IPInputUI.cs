using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IPInputUI : MonoBehaviour
{
    [SerializeField]
    TMP_InputField ipAddress;

    public void SubmitClick()
    {
        GameScene.isMulti = true;
        NetworkManager.connectingAddress = ipAddress.text; 
        SceneManager.LoadSceneAsync("GameScene"); //���� ȭ������ �̵�
    }

    public void ExitButtonClick()
    {
        Destroy(gameObject);
    }
}
