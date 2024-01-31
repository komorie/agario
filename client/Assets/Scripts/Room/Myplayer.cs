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
        // 충돌한 객체가 'Food'
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
        if (other.TryGetComponent(out preyPlayer) == true) //상대 플레이어랑 겹쳤다
        {
            Debug.Log($"거리: {Vector3.Distance(preyPlayer.transform.position, transform.position)}");
            if (preyPlayer.Radius < Radius && Vector3.Distance(preyPlayer.transform.position, transform.position) < Radius)
            {
                SendEatPlayerPacket(preyPlayer.PlayerId); //포식 시도 패킷
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
        //IsMoving이면 MoveVector에 따라서 이동 
        if (isMoving)
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }

    }



    //이 방법의 문제점이 평범한 플레이 방식이면 주기적으로 보내는 것보다 패킷을 덜 보낼 수도 있지만
    //키보드를 일부러 연타하거나 하면 패킷을 엄청나게 많이 보내고 받게 될텐데, 그것에 관한 처리는 나중에 서버에 대한 배움이 깊어지면 생각해 보자
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>(); 

        //moveVector에 context에서 vector2값 가져오기
        if (isMoving == false || MoveVector != context.ReadValue<Vector2>()) //이동방향 달라지면 
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
