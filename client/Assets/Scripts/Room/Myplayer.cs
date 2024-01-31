using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Myplayer : Player
{
    private NetworkManager network;
    private DefaultInputActions inputActions;
    public InputAction moveAction;
    public bool isMoving = false;
    public bool isAttachedOnWall = false;
    private Vector2 inputVector; 

    protected override void Awake()
    {
        base.Awake();

        network = NetworkManager.Instance;
        inputActions = new DefaultInputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
    }

    private void OnEnable()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
    }

    protected override void OnTriggerEnter(Collider other)
    {

        base.OnTriggerEnter(other);

        Food food;
        // �浹�� ��ü�� 'Food'
        if (other.TryGetComponent(out food) == true) 
        { 
            SendEatPacket(food.FoodId);
        }

        if (other.CompareTag("Wall"))
        {
            SendMovePacket();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Player preyPlayer;
        if (other.TryGetComponent(out preyPlayer) == true) //��� �÷��̾�� ���ƴ�
        {
            Debug.Log($"�Ÿ�: {Vector3.Distance(preyPlayer.transform.position, transform.position)}");
            if (preyPlayer.Radius < Radius && Vector3.Distance(preyPlayer.transform.position, transform.position) < Radius)
            {
                SendEatPlayerPacket(preyPlayer.PlayerId); //���� �õ� ��Ŷ
            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);  

        if (other.CompareTag("Wall"))
        {
            if(touchingColliders.Count == 0)
            {
                MoveVector = inputVector;
                SendMovePacket();
            }
        }
    }

    protected override void Update()
    {
        //IsMoving�̸� MoveVector�� ���� �̵� 
        if (isMoving)
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }

    }



    //�� ����� �������� ����� �÷��� ����̸� �ֱ������� ������ �ͺ��� ��Ŷ�� �� ���� ���� ������
    //Ű���带 �Ϻη� ��Ÿ�ϰų� �ϸ� ��Ŷ�� ��û���� ���� ������ �ް� ���ٵ�, �װͿ� ���� ó���� ���߿� ������ ���� ����� ������� ������ ����
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>(); 

        //moveVector�� context���� vector2�� ��������
        if (isMoving == false || MoveVector != context.ReadValue<Vector2>()) //�̵����� �޶����� 
        {
            MoveVector = context.ReadValue<Vector2>();
            isMoving = true;
        }

        if (touchingColliders.Count != 0)
        {
            MoveAttachedOnWall();
        }

        SendMovePacket();

    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        MoveVector = Vector2.zero;
        isMoving = false;
        SendMovePacket();
    }

    private void SendMovePacket()
    {
        C_Move movePacket = new C_Move();

        DateTime now = DateTime.UtcNow;
        float sendTime = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;

        movePacket.time = sendTime;
        movePacket.dirX = MoveVector.x;
        movePacket.dirY = MoveVector.y;
        movePacket.posX = transform.position.x;
        movePacket.posY = transform.position.y;
        movePacket.posZ = 0;

        network.Send(movePacket.Write());
    }

    private void SendEatPacket(int foodId)
    {
        C_EatFood eatPacket = new C_EatFood();  
        eatPacket.foodId = foodId;
        network.Send(eatPacket.Write());
    }

    private void SendEatPlayerPacket(int preyId)
    {
        C_EatPlayer eatPlayerPacket = new C_EatPlayer();
        eatPlayerPacket.predatorId = PlayerId;  
        eatPlayerPacket.preyId = preyId;
        network.Send(eatPlayerPacket.Write());
    }   
}
