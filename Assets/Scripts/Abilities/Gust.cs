using UnityEngine;
using System.Collections;

public class Gust : Ability {
    public override bool UseCharges {
        get {
            return false;
        }
    }


    public override int Cost {
        get {
            return 5;
        }
    }

    public override float GeneralDamage {
        get {
            return 2f;
        }
    }

    public override void ExecuteAbility() {
        
    }
}
