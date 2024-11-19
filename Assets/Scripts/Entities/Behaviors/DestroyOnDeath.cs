using System;
using UnityEngine;

public class DestroyOnDeath : MonoBehaviour
{
    private HealthSystem healthSystem;
    private Rigidbody2D rigidBody;

    private void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        rigidBody = GetComponent<Rigidbody2D>();
        healthSystem.OnDeath += OnDeath;
    }

    private void OnDeath()
    {
        rigidBody.velocity = Vector2.zero;
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {//rendere.color.a=0.3f �ȵǴ� ���� ����ü�� �� �����ؼ� �ִ���,.?
            Color color = renderer.color;
            color.a = 0.3f;
            renderer.color = color;
        }
        //��ü�������·�s
        foreach (Behaviour behaviour in GetComponentsInChildren<Behaviour>())
        {
            behaviour.enabled = false;
        }

        Destroy(gameObject, 2f);
    }
}