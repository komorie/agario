using UnityEngine;
using UnityEngine.InputSystem;

//플레이어가 움직이는 물체의 이동 처리를 담당하는 컴포넌트
public class KeyMover : Mover
{
    private DefaultInputActions inputActions;
    private PlayerPacketSender packetSender;  
    private bool isMoving = false;
    private Vector2 inputVector;

    public InputAction MoveAction { get; set; }

    private void Awake()
    {
        inputActions = new DefaultInputActions();
        inputActions.Enable();
        MoveAction = inputActions.Player.Move;
        packetSender = GetComponent<PlayerPacketSender>();
    }
    private void OnEnable()
    {
        MoveAction.performed += OnMovePerformed;
        MoveAction.canceled += OnMoveCanceled;
        if(packetSender != null) MoveVectorChangeEvent += packetSender.SendMovePacket; //멀티 플레이어에서, 직접 조작하는 플레이어일 경우만 이동 패킷 전송하도록 등록
    }

    private void OnDisable()
    {
        MoveAction.performed -= OnMovePerformed;
        MoveAction.canceled -= OnMoveCanceled;
        if (packetSender != null) MoveVectorChangeEvent -= packetSender.SendMovePacket;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (other.CompareTag("Wall"))
        {
            if (TouchingColliders.Count == 0)
            {
                MoveVector = inputVector;
                OnMoveVectorChanged(MoveVector);
            }
        }
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
            OnMoveVectorChanged(MoveVector);
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
        OnMoveVectorChanged(MoveVector);
    }
}
