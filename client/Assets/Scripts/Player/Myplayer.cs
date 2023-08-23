using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Myplayer : Player
{
    NetworkManager network;

    void Start()
    {
        network = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
/*        StartCoroutine(CoSendPacket());*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //C_Move 패킷 보내기 반복(나 여기로 이동했다)
    private IEnumerator CoSendPacket()
    {
        while(true)
        {
            C_Move movePacket = new C_Move();
            movePacket.posX = transform.position.x;
            movePacket.posY = transform.position.y;
            movePacket.posZ = 0;

            network.Send(movePacket.Write());

            yield return new WaitForSeconds(0.25f);
        }
    }
}
