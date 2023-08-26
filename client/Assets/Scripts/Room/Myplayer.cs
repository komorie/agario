using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Myplayer : Player
{
    private NetworkManager network;
    private DefaultInputActions inputActions;
    private InputAction moveAction;
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
            transform.localScale += 0.1f * Vector3.one;  // 크기를 0.1만큼 증가
            Radius = transform.localScale.x * 0.5f; //반지름도 증가  
            SendEatPacket(food.FoodId); //나 먹었소
            food.gameObject.SetActive(false); //먹은 음식은 비활성화
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
            //상대 플레이어 반지름이 나보다 작고, 상대 플레이어와 나의 거리가 나의 반지름보다 작으면
            {
                RoomManager.Instance.Players.Remove(preyPlayer.PlayerId); //상대 플레이어 먹음 -> 제거
                transform.localScale += (preyPlayer.transform.localScale / 2); //상대 플레이어 크기 반만큼 나도 커짐
                Radius = transform.localScale.x * 0.5f; //반지름도 증가   
                Destroy(preyPlayer.gameObject); //상대 플레이어 제거    
                SendEatPlayerPacket(preyPlayer.PlayerId);
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

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>(); 

        //moveVector에 context에서 vector2값 가져오기
        if (isMoving == false || MoveVector != context.ReadValue<Vector2>()) //이동방향 달라지면 나 이동했어요  
        {
            MoveVector = context.ReadValue<Vector2>();
            //짧은 시간동안 MoveVector가 계속 바뀌면 이전 것들은 무시하고 마지막 값으로만 SendMovePacket
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
