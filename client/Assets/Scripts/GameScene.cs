using UnityEngine;

public class GameScene : MonoBehaviour
{
    public static bool IsMulti { get; set; } = false;

    private void Awake()
    {
        if(IsMulti) { NetworkManager.Instance.Connect(); }
    }
}
