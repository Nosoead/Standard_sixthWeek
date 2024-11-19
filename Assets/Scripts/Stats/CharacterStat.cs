using UnityEngine;

public enum StatsChangeType
{   //������� �����ų���� --> �������� �ǹ̵� �ְ� �ǹ�(��������)���� �ǹ̵� �ְ� �̷� ����
    Add, // 0
    Multiple, // 1
    Override // 2
}

// ������ ����ó�� ����� �� �ְ� ������ִ� Attribute
// ������ �����̳ʶ�� �����ϸ� ����
[System.Serializable]
public class CharacterStat
{
    public StatsChangeType statsChangeType;
    [Range(0, 100)] public int maxHealth;
    [Range(0f, 20f)] public float speed;
    public AttackSO attackSO;
}