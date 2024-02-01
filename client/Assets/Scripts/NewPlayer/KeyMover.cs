using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//플레이어가 움직이는 물체의 이동 처리를 담당하는 컴포넌트
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
        if (isMoving == false || MoveVector != context.ReadValue<Vector2>()) //이동방향 달라지면 
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
