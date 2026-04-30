using UnityEngine;

public class RunnerZombie : ZombieBase
{
    public override float PatrolSpeed => 5.5f;
    public override float ChaseSpeed => 8f;
    public override float AttackSpeedMultiplier => 1.2f;


}
