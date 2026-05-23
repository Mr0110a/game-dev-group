using UnityEngine;

public class NormalZombie : ZombieBase
{
    // Standard zombie — uses all base values
    // No overrides needed yet; this is the reference behavior
    //----------------------------------temporary-------------------------------------------------
    protected override void Update()
    {
        base.Update();

        // TEMP DEBUG — press K to instantly kill exploder, remove before final build
        if (Input.GetKeyDown(KeyCode.K))
            Die();
    }

    //----------------------------------temporary-------------------------------------------------
}