using System;
using UnityEngine;

public class TopDownAnimationController : AnimationController
{
    private readonly int IsWalking = Animator.StringToHash("IsWalking");//static으로 되있음
    private static readonly int IsHit = Animator.StringToHash("IsHit");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private readonly float magnitudeThreshold = 0.5f;
    private HealthSystem healthSystem;
    
    protected override void Awake()
    {
        base.Awake();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        controller.OnAttackEvent += Attacking;
        controller.OnMoveEvent += Move;

        if (healthSystem != null)
        {
            healthSystem.OnDamage += Hit;
            healthSystem.OnInvincibilityEnd += InvincibilityEnd;
        }
    }

    private void Move(Vector2 obj)
    {
        animator.SetBool(IsWalking, obj.magnitude > magnitudeThreshold);
    }

    private void Attacking(AttackSO obj)
    {
        animator.SetTrigger(Attack);
    }

    private void Hit()
    {
        animator.SetBool(IsHit, true);
    }

    private void InvincibilityEnd()
    {
        animator.SetBool(IsHit, false);
    }
}