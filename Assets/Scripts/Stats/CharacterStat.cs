using UnityEngine;

public enum StatsChangeType
{   //순서대로 적용시킬거임 --> 숫자적인 의미도 있고 의미(버프순서)적인 의미도 있고 이런 느낌
    Add, // 0
    Multiple, // 1
    Override // 2
}

// 데이터 폴더처럼 사용할 수 있게 만들어주는 Attribute
// 데이터 컨테이너라고 생각하면 좋음
[System.Serializable]
public class CharacterStat
{
    public StatsChangeType statsChangeType;
    [Range(0, 100)] public int maxHealth;
    [Range(0f, 20f)] public float speed;
    public AttackSO attackSO;
}