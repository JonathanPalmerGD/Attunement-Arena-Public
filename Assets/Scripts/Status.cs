using UnityEngine;
using System.Collections;

public class Status : ScriptableObject
{
	//Bleed:		Damage over time
	//Slowed:		Reduce movement speed
	//Shielded:		Reduce damage taken
	//Sturdy:		Reduce knockback recieved.
	//Empowered:	Amplify damage dealt.

	public Ability Source;
	public Player Affected;
	public ParticleSystem VisualEffect;
	public bool ControlVisual
	{
		set
		{
			if (VisualEffect)
			{ VisualEffect.enableEmission = value; }
		}
		get
		{
			if (VisualEffect)
			{ return VisualEffect.enableEmission; }
			return false;
		}
	}
	public int statusIndex;

	public bool CleanupStatus = true;
	public bool StatusActive = true;

	public enum StatusTypes { Bleed, Slowed, Shielded, Sturdy, Empowered, Bounding, None }
	public StatusTypes curStatus = StatusTypes.None;

	public enum Alignment { Buff, Debuff, None }
	public Alignment alignment;
	public float DurationLeft;
	public float RemainingDuration
	{
		get { return DurationLeft; }
		set { DurationLeft = value; }
	}

	public float effectAmt;
	public float EffectAmt
	{
		get { return effectAmt; }
		set { effectAmt = value; }
	}

	public void SetupStatus(Player affected, Ability abilitySource, StatusTypes abilType, float duration, float effectAmount, bool cleanupStatus = true)
	{
		Affected = affected;
		Source = abilitySource;
		curStatus = abilType;
		RemainingDuration = duration;
		EffectAmt = effectAmount;
		CleanupStatus = cleanupStatus;
	}

	public void UpdateStatus(int index, float deltaTime)
	{
		//Keep our index up to date.
		statusIndex = index;

		//Adjust the duration
		if (DurationLeft > 0)
		{
			DurationLeft -= deltaTime;
		}

		//Remove the effects
		if (StatusActive)
		{
			if (DurationLeft <= 0)
			{
				DeactivateStatus();
			}
		}
		else
		{
			if (DurationLeft > 0)
			{
				ActivateStatus();
			}
		}
	}

	public void ActivateStatus()
	{
		//Debug.Log("Activating Status: " + curStatus + " of effectiveness " + effectAmt + "\n");
		ControlVisual = true;
		StatusActive = true;
	}

	public void DeactivateStatus()
	{
		//Debug.Log("Deactivating Status: " + curStatus + " of effectiveness " + effectAmt + "\n");
		
		//IMPORTANT NOTE: Removing the status effect must happen BEFORE we set EffectAmt to 0. Otherwise it won't remove the correct amount
		RemoveStatus();

		StatusActive = false;
		ControlVisual = false;
		DurationLeft = 0;
		EffectAmt = 0;
	}

	public void ModifyStatus(float durationAdj = 0, float effectAmountAdj = 0)
	{
		RemoveStatus();

		//Debug.Log("Modifying Status: " + effectAmt + " to " + (effectAmt + effectAmountAdj) + "\n");

		DurationLeft += durationAdj;
		effectAmt += effectAmountAdj;
		ApplyStatus();
	}

	public void ApplyStatus()
	{
		//Debug.Log("Applying Status: " + curStatus + "  " + effectAmt + "\n");
		if (curStatus == StatusTypes.Bleed)
		{
			Affected._dmgPerSec += effectAmt;
		}
		else if (curStatus == StatusTypes.Slowed)
		{
			Affected._speedMult -= effectAmt;
		}
		else if (curStatus == StatusTypes.Shielded)
		{
			Affected._dmgTakenMult -= effectAmt;
		}
		else if (curStatus == StatusTypes.Sturdy)
		{
			Affected._kckBackMult -= effectAmt;
		}
		else if (curStatus == StatusTypes.Empowered)
		{
			Affected._dmgDealtMult += effectAmt;
		}
		else if (curStatus == StatusTypes.Bounding)
		{
			Affected._jumpMult += effectAmt;
		}
	}

	public void RemoveStatus()
	{
		//TODO: Notify our source that we are no longer active.

		if (curStatus == StatusTypes.Bleed)
		{
			Affected._dmgPerSec -= effectAmt;
		}
		else if (curStatus == StatusTypes.Slowed)
		{
			Affected._speedMult += effectAmt;
		}
		else if (curStatus == StatusTypes.Shielded)
		{
			Affected._dmgTakenMult += effectAmt;
		}
		else if (curStatus == StatusTypes.Sturdy)
		{
			Affected._kckBackMult += effectAmt;
		}
		else if (curStatus == StatusTypes.Empowered)
		{
			Affected._dmgDealtMult -= effectAmt;
		}
		else if (curStatus == StatusTypes.Bounding)
		{
			Affected._jumpMult -= effectAmt;
		}
	}
}
