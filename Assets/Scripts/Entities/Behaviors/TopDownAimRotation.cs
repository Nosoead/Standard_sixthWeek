using System;
using System.IO;
using UnityEngine;

public class TopDownAimRotation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer armRenderer;
    [SerializeField] private Transform armPivot;

    [SerializeField] private SpriteRenderer characterRenderer;

    private TopDownController controller;

    private void Awake()
    {
        controller = GetComponent<TopDownController>();
    }

    private void Start()
    {
        controller.OnLookEvent += OnAim;
    }

    private void OnAim(Vector2 direction)
    {
        RotateArm(direction);
    }

    private void RotateArm(Vector2 direction)
    {
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //절댓값이 90도보다 크면 2,3사분면
        characterRenderer.flipX = Mathf.Abs(rotZ) > 90f;
        // flipY하는 것 추가
        armRenderer.flipY = characterRenderer.flipX;
        armPivot.rotation = Quaternion.Euler(0, 0, rotZ);
        
    }
}
