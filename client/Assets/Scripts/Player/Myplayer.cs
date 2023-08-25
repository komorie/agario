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
    }

    private void OnTriggerEnter(Collider other)
    {
        Food food;
        // 충돌한 객체가 'Food'
        if (other.TryGetComponent(out food) == true) 
        { 
            transform.localScale += 0.1f * Vector3.one;  // 크기를 0.1만큼 증가
            Radius = transform.localScale.x * 0.5f; //반지름도 증가  
            SendEatPacket(food.FoodId); //나 먹었소
            food.gameObject.SetActive(false); //먹은 음식은 비활성화
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
        //moveVector에 context에서 vector2값 가져오기
        if(isMoving == false || MoveVector != context.ReadValue<Vector2>()) //이동방향 달라지면 나 이동했어요  
        {
            MoveVector = context.ReadValue<Vector2>();
            //짧은 시간동안 MoveVector가 계속 바뀌면 이전 것들은 무시하고 마지막 값으로만 SendMovePacket
            SendMovePacket();
            isMoving = true;
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
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
}
