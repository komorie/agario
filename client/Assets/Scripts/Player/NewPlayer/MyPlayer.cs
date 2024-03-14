using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayer : Player
{
    private PlayerInputActions inputActions;
    private InputAction moveAction;

    private NewMover mover;
    private PlayerPacketSender packetSender;

    private Vector2 inputVector;

    private void Awake()
    {
        PlayerMover = GetComponent<Mover>();
        PlayerEater = GetComponent<Eater>();
        PacketSender = GetComponent<PlayerPacketSender>();

        inputActions = InputActionsWrapper.inputActions;
        inputActions.Enable();
        moveAction = inputActions.Player.Move;

        mover = GetComponent<NewMover>();
        packetSender = GetComponent<PlayerPacketSender>();  
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

    // Update is called once per frame
    void Update()
    {
        Vector2 moveVec = mover.Move(mover.WallCalculate(inputVector)); //움직이고 움직인 벡터 리턴
        if (mover.IsDirChanged() && packetSender != null)
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


}
