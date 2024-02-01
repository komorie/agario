using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//�÷��̾ �����̴� ��ü�� �̵� ó���� ����ϴ� ������Ʈ
public class KeyMover : Mover
{
    private DefaultInputActions inputActions;
    public InputAction moveAction;
    private bool isMoving = false;
    private Vector2 inputVector;

    private void Awake()
    {
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

    private void Update()
    {
        //IsMoving�̸� MoveVector�� ���� �̵� 
        if (isMoving)
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }

    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();

        //moveVector�� context���� vector2�� ��������
        if (isMoving == false || MoveVector != context.ReadValue<Vector2>()) //�̵����� �޶����� 
        {
            MoveVector = context.ReadValue<Vector2>();
            isMoving = true;
        }

        if (TouchingColliders.Count != 0)
        {
            MoveAttachedOnWall();
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
        MoveVector = Vector2.zero;
        isMoving = false;
    }
}
