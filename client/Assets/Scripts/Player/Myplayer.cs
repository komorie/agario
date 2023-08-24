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

    protected override void Update()
    {
        //IsMoving�̸� MoveVector�� ���� �̵� 
        if (IsMoving)
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;  
        }

    }   

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        //moveVector�� context���� vector2�� ��������
        if(IsMoving == false || MoveVector != context.ReadValue<Vector2>()) //�̵����� �޶����� �� �̵��߾��  
        {
            MoveVector = context.ReadValue<Vector2>();
            SendMovePacket();
            IsMoving = true;
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveVector = Vector2.zero;
        IsMoving = false;
        SendMovePacket();
    }

    private void SendMovePacket()
    {
        C_Move movePacket = new C_Move();

        movePacket.dirX = MoveVector.x;
        movePacket.dirY = MoveVector.y;
        movePacket.posX = transform.position.x;
        movePacket.posY = transform.position.y;
        movePacket.posZ = 0;

        network.Send(movePacket.Write());
        
    }
}
