using UnityEngine;
using UnityEngine.InputSystem;

//�÷��̾ �����̴� ��ü�� �̵� ó���� ����ϴ� ������Ʈ
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
        if(packetSender != null) MoveVectorChangeEvent += packetSender.SendMovePacket; //��Ƽ �÷��̾��, ���� �����ϴ� �÷��̾��� ��츸 �̵� ��Ŷ �����ϵ��� ���
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
