using UnityEngine;

public class PickUpHeal : PickUpItem
{
    [SerializeField] int healValue = 10;

    protected override void OnPickedUp(GameObject go)
    {
       HealthSystem healthSystem  = go.GetComponent<HealthSystem>();
        healthSystem.ChangeHealth(healValue);
    }
}