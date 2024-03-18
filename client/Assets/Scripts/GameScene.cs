using System;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    public static bool isMulti = false;

    private void Start()
    {
        if (isMulti) 
        { 
            NetworkManager.Instance.Connect(NetworkManager.connectingAddress);
        }
    }

}
