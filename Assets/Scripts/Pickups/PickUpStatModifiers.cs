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
        
        //�ִ� ü���� �ø��ų� ü���� ȸ���ϴ� ���
        HealthSystem healthSystem = go.GetComponent<HealthSystem>();
        healthSystem.ChangeHealth(0);
    }

}
