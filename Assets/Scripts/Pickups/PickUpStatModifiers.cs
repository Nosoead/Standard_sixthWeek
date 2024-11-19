using System.Collections.Generic;
using UnityEngine;

public class PickUpStatModifiers : PickUpItem
{
    [SerializeField] List<CharacterStat> statsModifier = new List<CharacterStat>();
    protected override void OnPickedUp(GameObject go)
    {
        CharacterStatsHandler statHandler = go.GetComponent<CharacterStatsHandler>();

        foreach (CharacterStat modifier in statsModifier)
        {
            statHandler.AddStatModifier(modifier);
        }
        
        //최대 체력을 올리거나 체력을 회복하는 경우
        HealthSystem healthSystem = go.GetComponent<HealthSystem>();
        healthSystem.ChangeHealth(0);
    }

}
