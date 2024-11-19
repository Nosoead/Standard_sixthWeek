using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownController : MonoBehaviour
{
    public event Action<Vector2> OnMoveEvent; //Action�� ������ void�� ��ȯ �ƴϸ� Func
    public event Action<Vector2> OnLookEvent;
    public event Action<AttackSO> OnAttackEvent;//�����ٴ� ��Ǹ� �߿��ؼ� ���� ���� �ʿ����
    protected bool IsAttacking { get; set; }

    private float timeSinceLastAttack = float.MaxValue;

    // protected ������Ƽ�� �� ���� : ���� �ٲٰ������� �������°� �� ��ӹ޴� Ŭ�����鵵 �� �� �ְ�!
    protected CharacterStatsHandler stats { get; private set; }

    protected virtual void Awake()
    {
        stats = GetComponent<CharacterStatsHandler>();
    }
    private void Update()
    {
        HandleAttackDelay();
    }

    private void HandleAttackDelay()
    {
        // TODO :: MAGIC NUMBER ����
        if (timeSinceLastAttack < stats.CurrentStat.attackSO.delay)  //�ӽ÷� 0.2f�־���
        {
            timeSinceLastAttack += Time.deltaTime;
        }
        else if (IsAttacking && timeSinceLastAttack >= stats.CurrentStat.attackSO.delay)
        {
            timeSinceLastAttack = 0f;
            CallAttackEvent(stats.CurrentStat.attackSO);
        }
    }


    public void CallMoveEvent(Vector2 direction)
    {
        OnMoveEvent?.Invoke(direction); // ?. ������ ���� ������ ����
    }

    public void CallLookEvent(Vector2 direction)
    {
        OnLookEvent?.Invoke(direction);
    }

    private void CallAttackEvent(AttackSO attackSO)
    {
        OnAttackEvent?.Invoke(attackSO);
    }
}