using UnityEngine;

public class NormalZombie : ZombieBase
{
    public override float PatrolSpeed => 3f;
    public override float ChaseSpeed => 4.5f;
    public override float AttackSpeedMultiplier => 1f;

}
