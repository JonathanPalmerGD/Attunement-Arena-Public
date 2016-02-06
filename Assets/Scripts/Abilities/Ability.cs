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
