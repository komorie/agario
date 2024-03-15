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
        SceneManager.LoadSceneAsync("GameScene"); //게임 화면으로 이동
    }

    public void ExitButtonClick()
    {
        Destroy(gameObject);
    }
}
