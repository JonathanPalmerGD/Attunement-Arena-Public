using UnityEngine;
using System.Collections;

public class Ability : ScriptableObject
{
	public Player Owner;

	public bool initialized = false;
	public string keyBinding;
	public string keyBindingUserDisplay;

	public AbilityDisplayUI abilDispUI;
	public enum KeyActivateCond { KeyDown, KeyHold, GetAxis }
	public virtual KeyActivateCond activationCond
	{
		get { return KeyActivateCond.KeyDown; }
	}

	public virtual int IconID
	{
		get { return 0; }
	}
	public virtual int AlternateIconID
	{
		get { return 0; }
	}
	public int SetDisplayIcon
	{
		set
		{
			abilDispUI.Icon.sprite = UIManager.Icons[value];
		}
	}

	public virtual bool UseCharges
	{
		get { return false; }
	}
	private int charges;
	public int Charges
	{
		get { return charges; }
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (value >= MaxCharges)
			{
				value = MaxCharges;
			}
			if (UseCharges)
			{
				abilDispUI.ChargeDisplay.text = "" + value;
				charges = value;
			}
		}
	}
	public int MaxCharges = 0;
	private float cost = 0;
	public virtual float Cost
	{
		get { return cost; }
		set
		{
			if (value <= 0)
			{
				value = 0;
			}
			if (value >= Owner.MaxMana)
			{
				value = Owner.MaxMana;
			}

			abilDispUI.CostDisplay.text = "" + (int)value;
			cost = value;
		}

	}
	protected float generalDamage = 0;
	public virtual float GeneralDamage
	{
		get { return generalDamage; }
		set { generalDamage = value; }
	}
	protected float specialDamage = 0;
	public virtual float SpecialDamage
	{
		get { return generalDamage; }
		set { generalDamage = value; }
	}
	public float Duration;

	protected float currentCooldown;
	public virtual float CurrentCooldown
	{
		get
		{
			return currentCooldown;
		}
		set
		{
			currentCooldown = value;

			abilDispUI.CooldownDisplay.fillAmount = currentCooldown / MaxCooldown;
		}
	}
	public float MaxCooldown;
	protected float cooldownReduction = 0;
	public virtual float CooldownReduction
	{
		get { return cooldownReduction; }
		set { cooldownReduction = value; }
	}

	public virtual void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		if (!initialized)
		{
			//Add UI to the controlling player
			Owner = newOwner;

			keyBinding = newKeyBinding;
			keyBindingUserDisplay = displayKeyBinding;

			//Note, this property is set by AddAbilityDisplay, therefore assigning it here is for understanding what's going on more than actually doing anything.
			abilDispUI = UIManager.Instance.AddAbilityDisplay(this);

			abilDispUI.SetupDisplay(this);

			initialized = true;
		}
	}

	public virtual void UpdateAbility(float deltaTime)
	{
		if (CurrentCooldown > 0)
		{
			CurrentCooldown -= deltaTime;
		}
		else
		{
			if (UseCharges)
			{
				if (Charges < MaxCharges)
				{
					Charges++;
				}
				CurrentCooldown = MaxCooldown;
			}
			else
			{
				CurrentCooldown = 0;
			}
		}
	}

	public virtual bool CanUseAbility()
	{
		bool canPayCost = false;
		bool chargeValid = false;
		bool onCooldown = false;

		//If our owner can pay the cost
		if (Owner.Mana >= Cost)
		{
			canPayCost = true;
		}

		//If our owner doesn't use charges or has enough charges
		if (!UseCharges || Charges > 0)
		{
			chargeValid = true;
		}

		//If we aren't on cooldown
		if (UseCharges || CurrentCooldown <= 0)
		{
			onCooldown = true;
		}

		//Activate the ability.
		if (canPayCost && chargeValid && onCooldown)
		{
			return true;
		}

		//Debug.Log("Failed to be able to use the ability\n" + canPayCost + " " + chargeValid + " " + onCooldown + "\n" + OutputInfo());
		return false;
	}

	//Returns false if unsuccessful
	public bool ActivateAbilityOverhead(Vector3 inputVector = default(Vector3), bool validateActivation = true)
	{
		//Debug.Log(OutputInfo());
		if (validateActivation)
		{
			if (!CanUseAbility())
			{
				return false;
			}
		}

		//Set it on cooldown
		CurrentCooldown = MaxCooldown;

		//Remove a charge if we use charges
		if (UseCharges)
		{
			Charges--;
		}

		//Pay the cost
		Owner.Mana -= Cost;
		//Debug.Log(Owner.Mana + "  -" + Cost + "\n");

		//Activate the Effect
		ExecuteAbility(inputVector.normalized);

		return true;
	}

	public virtual void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		return;
	}

	/// <summary>
	/// A function for getting a blast intensity which is frequently used by different abilities.
	/// </summary>
	/// <param name="sourcePoint">The origination point of the intensity</param>
	/// <param name="target">The Player object to check</param>
	/// <param name="explosiveRange">The range of the explosion</param>
	/// <param name="minPercentFalloff">The percentage of blast that should be affected on the complete edge.</param>
	/// <param name="maxPercentFalloff">The highest scalable percentage in the inner area</param>
	/// <param name="affectOwner">Should it affect the owner of the ability?</param>
	/// <param name="needClearLoS">If it should raycast to make a clear line of sight.</param>
	/// <returns>The percentage of blast intensity from the min to the max.</returns>
	public virtual float CheckBlastIntensity(Vector3 sourcePoint, Player target,
										float explosiveRange = 0, float minPercentFalloff = .25f, float maxPercentFalloff = 1, 
										bool affectOwner = false, bool needClearLoS = true)
	{
		//TODO: Implement Need Clear LoS correctly.

		if (!affectOwner || target != Owner)
		{
			float dist = Vector3.Distance(Owner.transform.position, target.transform.position);
			//Debug.Log("Checking blast intensity: " + Owner.name + "  " + affectOwner + "  " + target.name + "\nDist: " + dist);

			//Debug.DrawRay(Owner.transform.position, target.transform.position - Owner.transform.position, Color.black, 5.0f);
			RaycastHit hit;
			//Debug.Log(Vector3.Distance(Owner.transform.position, target.transform.position) + "\n");
			if (Physics.Raycast(Owner.transform.position, target.transform.position - Owner.transform.position, out hit, explosiveRange))
			{
				//Debug.Log("Hit\n" + hit.transform.name);
				if (hit.collider.gameObject.tag == "Player" && hit.collider.gameObject == target.gameObject)
				{
					//Debug.Log(target.name + "\tExpDist :" + explosiveRange + " - " + dist + " " + explosiveRange + "\n");

					//Find what percentage away the player is
					float percentOfdist = (explosiveRange - dist) / explosiveRange;

					//The players take a minimum of 25% effect from the blast.
					float blastIntensityBasedOnDist = Mathf.Clamp(percentOfdist, minPercentFalloff, maxPercentFalloff);

					//Find the direction they're knocked away
					Vector3 knockbackDir = target.transform.position - Owner.transform.position;

					//Debug.Log(percentOfdist + "  " + blastIntensityBasedOnDist + " \n" + knockbackDir + "\n");

					//Debug.DrawLine(Owner.transform.position, Owner.transform.position + Vector3.up * 100, Color.blue, 15f);

					return blastIntensityBasedOnDist;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// A function for checking if a target is within a cone of effect.
	/// </summary>
	/// <param name="sourcePoint">The origination point for the cone</param>
	/// <param name="coneDir">The direction the cone is aimed</param>
	/// <param name="target">The Player object to check</param>
	/// <param name="ConeRange">Range of the cone (not perfect range, distance from point to edge on cone)</param>
	/// <param name="MaxConeAngle">Angle of the Cone, in degrees</param>
	/// <param name="affectOwner">Should this cone affect the owner?</param>
	/// <param name="needClearLoS">If the cone needs to respect Line of Sight</param>
	/// <returns>If the target is within the cone (respecting LoS or hitting Owner)</returns>
	public virtual bool CheckConeEffect(Vector3 sourcePoint, Vector3 coneDir, Player target, 
										float ConeRange = 10, float MaxConeAngle = 15, 
										bool affectOwner = false, bool needClearLoS = true)
	{
		foreach (Player p in GameManager.Instance.players)
		{
			//Don't influence self.
			if (affectOwner && p == Owner) return false;

			// Get vector from source to target
			Vector3 tetherVector = target.transform.position - sourcePoint;

			// If out of range, ignore
			if (tetherVector.sqrMagnitude > ConeRange * ConeRange) return false;

			// If target is not within the cone, ignore
			if (Vector3.Angle(coneDir, tetherVector) > MaxConeAngle) return false;

			//If we don't need line of sight, return true.
			if (!needClearLoS)
				return true;

			//Debug.DrawRay(Owner.transform.position, p.transform.position - Owner.transform.position, Color.black, 5.0f);
			RaycastHit hit;
			if (Physics.Raycast(Owner.transform.position, p.transform.position - Owner.transform.position, out hit, ConeRange))
			{
				//Debug.DrawRay(Owner.transform.position, p.transform.position - Owner.transform.position, Color.black, 5.0f);
				//Debug.Log(hit.collider.gameObject.name + "\n" + Owner.name + "   " + hit.collider.gameObject.tag);
				if (hit.collider.gameObject.tag == "Player" && hit.collider.gameObject == target.gameObject)
				{
					return true;
				}
			}
			//Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);
		}

		return false;
	}
	
	public virtual void RefreshAbility()
	{
		CurrentCooldown = 0;
		Charges = MaxCharges;
	}

	public virtual void SetCharges()
	{
		if (UseCharges)
		{
			Charges = MaxCharges;
		}
	}

	public virtual string OutputInfo()
	{
		string output = "[Ability]: " + this.GetType() + "\n";
		output += "Owner: " + Owner.name + "\n";
		output += "keyBinding: " + keyBinding + "\n";
		output += "Charges: " + Charges + "\n";
		output += "MaxCharges: " + MaxCharges + "\n";
		output += "Cost: " + Cost + "\n";
		output += "GeneralDamage: " + GeneralDamage + "\n";
		output += "SpecialDamage: " + SpecialDamage + "\n";
		output += "CurrentCooldown: " + MaxCooldown + "\n";
		output += "UseCharges: " + UseCharges + "\n";

		return output;
	}
}
