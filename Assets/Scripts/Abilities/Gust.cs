using UnityEngine;
using System.Collections;

public class Gust : Ability
{
    public override bool UseCharges
    {
        get
        {
            return false;
        }
    }


    public override int Cost
    {
        get
        {
            return 5;
        }
    }

    public override float GeneralDamage
    {
        get
        {
            return 2f;
        }
    }

    public float Force
    {
        get { return 5f; }
    }

    public float Range
    {
        get { return 10f; }
    }

    public float MaxAngle
    {
        get { return 15f; }
    }

    public override void ExecuteAbility()
    {
        ExecuteAbility(false);
    }

    public void ExecuteAbility(bool downwards)
    {
        var castDir = downwards ? (new Vector3(0f, 1, 0f)) : Owner.targetScanDir.normalized;

        foreach (Player p in GameManager.Instance.players)
        {
            if (p == Owner) continue; // Don't influence self just yet

            // Get vector from owner to other player
            var tetherVector = Owner.transform.position - p.transform.position;

            // If out of range, ignore
            if (tetherVector.sqrMagnitude > Range * Range) continue;

            // If there's something in the way, ignore player
            if (Physics.Linecast(Owner.transform.position, p.transform.position)) continue;

            // If the other player is not in the cone of influence, ignore
            if (Vector3.Angle(castDir, tetherVector) > MaxAngle) continue;

            p.SendMessage("ApplyExternalForce", castDir * Force);
        }

        Owner.SendMessage("ApplyExternalForce", castDir * Force * -1f);
    }
}
