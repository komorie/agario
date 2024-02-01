using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_RoomList;
using static UnityEditor.Experimental.GraphView.GraphView;

//Ÿ �÷��̾ ��Ʈ��ũ�� ���� �����̴� ��ü�� �̵� ó���� ����ϴ� ������Ʈ
public class NetworkMover : Mover
{

    private NewPlayer player;
    private PacketReceiver packetReceiver;
    private bool isLerping = false;
    private Vector3 targetPosition;

    private void Awake()
    {
        player = GetComponent<NewPlayer>(); 
        packetReceiver = PacketReceiver.Instance;   
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastMove += RecvBroadcastMove;
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastMove -= RecvBroadcastMove;
    }
    public void Update()
    {
        if (isLerping) //�ϴ� ��Ŷ�� �ͼ� ������ �����ؾ� �ϴ� ���
        {

            float currentDistance = Vector3.Distance(transform.position, targetPosition); //��� Ŭ�� ������ ���� ��ġ(+���� �� ��ġ)�� �������� �� Ŭ�� �����ؼ� �̵���Ų ��ġ�� �Ÿ� ��

            Vector3 newDes = Vector3.Lerp(transform.position, targetPosition, 0.5f);   //����� ���� ���� ��ġ�� �̵��ϵ�, ���� �������� ����� ���� ��ġ�� ��ǥ ��ġ�� �߰� ��ġ ����

            transform.position = Vector3.MoveTowards(transform.position, newDes, (Speed + currentDistance) * Time.deltaTime); //�Ÿ� ���̸�ŭ�� �������� ���� �ִ� �ӵ��� �ؼ� �ε巴�� ����ȭ. ���� �� �����ϰԴ� ���ϰڴ�.

            targetPosition += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime; //�ϴ� ��� Ŭ���� ���� ������ ���� ��ġ ����

            if (currentDistance < 0.5f) //���� ��ġ�ϴ� �������� ������ ����
            {
                isLerping = false;
            }
        }
        else //���� ���� �Ϸ� ��, �̵� ���Ϳ� �°� ��������� ��
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }
    }

    private void RecvBroadcastMove(S_BroadcastMove p)
    {
        //�� ��Ŷ��� ��ġ ����
        if (player.PlayerId == p.playerId)
        {
            DateTime now = DateTime.UtcNow;
            float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
            float RTT = currentSecond - p.time;

            MoveVector = new Vector2(p.dirX, p.dirY); //�ٸ� �÷��̾��� �̵� ����
            targetPosition = new Vector3(p.posX + p.dirX * Speed * RTT, p.posY + p.dirY * Speed * RTT, p.posZ);
            //�ٸ� �÷��̾��� ���� ��ġ ����
            //Myplayer�� �����Ӵ� dir * Speed * Time.deltaTime ��ŭ ���� ��ġ�� �̵��ϸ� Time.deltaTime�� �����Ӵ� �帥 �ð����̴�. �� dir * Speed�� 1�ʴ� �̵��� �ӵ��� �ȴ�. �� 1�ʴ� �̵��Ÿ��� 20
            //�׷� dir * Speed * RTT�� RTT�� ��ŭ �̵��� ��ġ�̴�. �������� �� ��ġ + dir * Speed * RTT �� ����� ���� ��ġ��� �����ϰ�, �� Ŭ�󿡼� ���� ��ġ�� ������ �ǽ�
            isLerping = true;
        }
    }

}
