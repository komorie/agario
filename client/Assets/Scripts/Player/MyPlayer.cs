using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static S_RoomList;

public class MyPlayer : Player
{
    private PlayerInputActions inputActions;
    private InputAction moveAction;
    private Vector2 inputVector = Vector2.zero;

    protected override void Awake()
    {
        base.Awake();   
        inputActions = InputActionsWrapper.inputActions;
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
    }

    private void OnEnable()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;

        if(GameScene.isMulti)
        {
            packetReceiver.OnBroadcastEatFood += RecvEatFood;
            packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer;
        }
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;

        if (GameScene.isMulti)
        {
            packetReceiver.OnBroadcastEatFood -= RecvEatFood;
            packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 충돌한 객체가 'Food'
        if (other.TryGetComponent(out Food food) == true)
        {
            if (eater.TryEatFood(food) && GameScene.isMulti) //먹기 시도 -> 한번만 패킷 보내기
            {
                packetSender.SendEatPacket(food.FoodId);
                Debug.Log("Send Eat!");
            }
        }
        if (other.TryGetComponent(out Player prey) == true) //상대 플레이어랑 거리 내에서 겹쳤는지 확인
        {
            if (eater.TryEatPlayer(prey) && GameScene.isMulti) //먹기 시도 -> 한번만 패킷 보내기
            {
/*                Debug.Log("Send Eat!");*/
                packetSender.SendEatPlayerPacket(PlayerId, prey.PlayerId); 
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveVec = mover.Move(mover.WallCalculate(inputVector)); //움직이고 움직인 벡터 리턴
        if (mover.IsDirChanged() && GameScene.isMulti)
        {
            packetSender.SendMovePacket(moveVec); //방향 바꼈으면 패킷 보내기
        }
    }

    public void ToggleMoveAction()
    {
        if(moveAction.enabled)
        {
            moveAction.Disable();
        }
        else
        {
            moveAction.Enable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        if(inputVector.magnitude < 0.6f) //모바일 컨트롤러에서 입력이 약하면 0 처리
        {
            inputVector = Vector2.zero;
        }
        else
        {
            inputVector = Vector3.Normalize(inputVector);
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
    }
    private void RecvEatFood(S_BroadcastEatFood p)
    {
        if (p.playerId == PlayerId)
        {
/*            Debug.Log("Receive Eat!");*/
            eater.EatFoodComplete(p);
        }
    }
    private void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        if (p.predatorId == PlayerId)
        {
            Debug.Log("Receive Eat!");
            eater.EatPlayerComplete(p);
        }
    }
}
