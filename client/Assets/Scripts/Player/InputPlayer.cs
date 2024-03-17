using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPlayer : Player
{
    private PlayerInputActions inputActions;
    private InputAction moveAction;
    private InputAction beamAction;
    private InputAction stealthAction;

    protected override void Awake()
    {
        base.Awake();   
        inputActions = InputActionsWrapper.inputActions;
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        beamAction = inputActions.Player.Beam;  
        stealthAction = inputActions.Player.Stealth;   
    }

    private void OnEnable()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        beamAction.performed += OnBeamPerformed;
        stealthAction.performed += OnStealthPerformed;

        if(GameScene.isMulti)
        {
            packetReceiver.OnBroadcastEatFood += RecvEatFood;
            packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer;
            packetReceiver.OnBroadcastBeamHit += RecvBeamHit;
            packetReceiver.OnBroadcastStealth += RecvStealth;
        }
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        beamAction.performed -= OnBeamPerformed;
        stealthAction.performed -= OnStealthPerformed;

        if (GameScene.isMulti)
        {
            packetReceiver.OnBroadcastEatFood -= RecvEatFood;
            packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
            packetReceiver.OnBroadcastBeamHit -= RecvBeamHit;  
            packetReceiver.OnBroadcastStealth -= RecvStealth;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // �浹�� ��ü�� 'Food'
        if (other.TryGetComponent(out Food food) == true)
        {
            if (eater.TryEatFood(food) && GameScene.isMulti) //�Ա� �õ� -> �ѹ��� ��Ŷ ������
            {
                packetSender.SendEatPacket(food.FoodId);
                Debug.Log("Send Eat!");
            }
        }
        if (other.TryGetComponent(out Player prey) == true) //��� �÷��̾�� �Ÿ� ������ ���ƴ��� Ȯ��
        {
            if (eater.TryEatPlayer(prey) && GameScene.isMulti) //�Ա� �õ� -> �ѹ��� ��Ŷ ������
            {
/*                Debug.Log("Send Eat!");*/
                packetSender.SendEatPlayerPacket(PlayerId, prey.PlayerId); 
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveVec = mover.Move(mover.WallCalculate(inputVector)); //�����̰� ������ ���� ����
        if (mover.IsDirChanged() && GameScene.isMulti)
        {
            packetSender.SendMovePacket(moveVec); //���� �ٲ����� ��Ŷ ������
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        if(inputVector.magnitude < 0.6f) //����� ��Ʈ�ѷ����� �Է��� ���ϸ� 0 ó��
        {
            inputVector = Vector2.zero;
        }
        else
        {
            inputVector = Vector3.Normalize(inputVector);
            dir = inputVector;
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
    }

    private void OnBeamPerformed(InputAction.CallbackContext context)
    {
        StartCoroutine(beamAttack.BeamCharge(dir, Radius));
    }

    private void OnStealthPerformed(InputAction.CallbackContext context)
    {
        stealth.StealthStart();
        if (GameScene.isMulti && packetSender != null) { packetSender.SendStealthPacket(PlayerId); }
    }

    private void RecvEatFood(S_BroadcastEatFood p)
    {
        if (p.playerId == PlayerId)
        {
            eater.EatFoodComplete(p);
        }
    }
    private void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        if (p.predatorId == PlayerId)
        {
            eater.EatPlayerComplete(p);
        }
    }

    private void RecvBeamHit(S_BroadcastBeamHit p)
    {
        List<int> playerIds = new List<int>();

        foreach (S_BroadcastBeamHit.HitPlayer player in p.hitPlayers) playerIds.Add(player.playerId);
        beamAttack.BeamHit(playerIds);
    }

    private void RecvStealth(S_BroadcastStealth p)
    {
        if (p.userId == PlayerId)
        {
            stealth.StealthStart();
        }
    }

    public void ToggleInputActions()
    {
        if (moveAction.enabled && beamAction.enabled)
        {
            moveAction.Disable();
            beamAction.Disable();
        }
        else
        {
            moveAction.Enable();
            beamAction.Enable();
        }
    }
}
