using UnityEngine;

public class GameScene : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager.Instance.Connect();
    }
}
