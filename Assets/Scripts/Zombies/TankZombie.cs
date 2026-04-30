using UnityEngine;

public class TankZombie : ZombieBase
{
    public override float PatrolSpeed => 2f;
    public override float ChaseSpeed => 3f;
    public override float AttackSpeedMultiplier => 0.7f;


}
