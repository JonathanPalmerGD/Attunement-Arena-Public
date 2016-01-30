using UnityEngine;
using System.Collections;

public class Ability : ScriptableObject
{
	public Player Owner;

	public bool initialized = false;
	public string keyBinding;
	public string keyBindingUserDisplay;

	public AbilityDisplayUI abilDispUI;
	public int IconID;

	public virtual bool UseCharges
	{
		get { return false; }
	}
	public int Charges = 0;
	public virtual int Cost
	{
		get { return 0; }
	}
	public virtual float GeneralDamage
	{
		get { return 0f; }
	}
	public virtual float SpecialDamage
	{
		get { return 0f; }
	}
	public float CurrentCooldown;
	public float MaxCooldown;

	public void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
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
		if (CurrentCooldown >= 0)
		{
			CurrentCooldown -= deltaTime;
		}
		else
		{
			CurrentCooldown = 0;
		}
	}

	public bool CanUseAbility()
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

		return false;
	}

	//Returns false if unsuccessful
	public bool ActivateAbilityOverhead(bool validateActivation = true)
	{
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

		//Activate the Effect
		ExecuteAbility();

		return true;
	}

	public virtual void ExecuteAbility()
	{
		return;
	}
}
