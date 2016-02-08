using UnityEngine;
using System.Collections;

public class Smash : Ability
{
	public GameObject rockFistPrefab;
	public GameObject groundPoundPrefab;

	public EarthFists activeFists;

	public override int IconID
	{ get { return 5; } }
	public override int AlternateIconID
	{ get { return 15; } }
	public override bool UseCharges
	{
		get
		{
			return true;
		}
	}
	public override float Cost
	{
		get
		{
			return 10;
		}

		set
		{
			return;
		}
	}

	public bool SetFistActivity
	{
		get { return activeFists.gameObject.activeSelf; }
		set { activeFists.gameObject.SetActive(value); }
	}
	private float counter = 0;
	public float SwingTimer
	{
		get { return counter; }
		set { counter = value; }
	}
	public float SwingDuration = 3;

	public bool CurrentlySwinging;

	#region Attributes
	//How much it knocks others back
	public float Force = 450;

	//How much it knocks people up when you hit the ground.
	public float RecoilForce = 450;

	public float PoundRange = 13;

	//The size of the fists.
	public float FistSize = 4;

	//For a ritual?
	public bool earthAligned = true;
	#endregion

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		rockFistPrefab = Resources.Load<GameObject>("Projectiles/RockSmashPrefab");
		groundPoundPrefab = Resources.Load<GameObject>("Effects/groundPoundPrefab");
		GeneralDamage = 18;
		SpecialDamage = 12;
		MaxCooldown = 6;
		MaxCharges = 3;

		activeFists = GameObject.Instantiate<GameObject>(rockFistPrefab).GetComponent<EarthFists>();
		activeFists.transform.SetParent(newOwner.transform);
		activeFists.Creator = this;


		base.Init(newOwner, newKeyBinding, displayKeyBinding);

		PlaceFists();
		SetFistActivity = false;
	}

	public void PlaceFists()
	{
		activeFists.transform.position = Owner.transform.position;
		activeFists.transform.position += Vector3.up * ((Owner.transform.localScale.y / 2) + FistSize / 2);
		activeFists.transform.position += Owner.transform.forward * (1.0f);
		activeFists.transform.rotation = Quaternion.Euler(Vector3.zero);
	}

	public override void UpdateAbility(float deltaTime)
	{
		//If we are at max charges, don't cooldown
		if (Charges == MaxCharges)
		{
			CurrentCooldown = 0;
		}
		else
		{
			//If we're swinging, we can't use the ability
			if (CurrentlySwinging)
			{
				//Let ourselves swing
				SwingTimer -= deltaTime;
				SetDisplayIcon = AlternateIconID;
				if (SwingTimer <= 0)
				{
					CurrentlySwinging = false;
					//Reset this here
					SwingTimer = SwingDuration;
				}
			}
			else
			{
				//
				SetDisplayIcon = IconID;

				//Only refreshes cooldown when grounded?
				if (Owner.controller.Grounded)
				{
					base.UpdateAbility(deltaTime);
				}
			}
		}
	}

	public override bool CanUseAbility()
	{
		if (!CurrentlySwinging)
		{
			return base.CanUseAbility();
		}
		return false;
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		PlaceFists();
		SetFistActivity = true;
		
		//Swing the fists down over our head.
		CurrentlySwinging = true;
		SwingTimer = SwingDuration;

		base.ExecuteAbility(inputVector);
	}

	public void Collide()
	{
		//TODO: Play Ground Pound Audio

		foreach (Player player in GameManager.Instance.players)
		{
			if (player != Owner)
			{
				//float blastIntensity = CheckBlastIntensity(activeFists.transform.position, player, FistSize, .25f, 1.0f, false, true);

				//if (blastIntensity > 0)
				//{
				//Find the direction they're knocked away
				Vector3 knockbackDir = player.transform.position - Owner.transform.position;

				//Knock them away based on the direction, force of the ability and how much of the blast they're affected by
				player.controller.ApplyExternalForce((knockbackDir.normalized + Vector3.up).normalized * Force);

				//Adjust their health based on the blasts damage and how much of the blast.
				player.AdjustHealth(-1 * GeneralDamage);

				//Debug.Log("Smashed " + player.name + "\n");
				//}
			}
		}

		CurrentlySwinging = true;
		SwingTimer = 1f;
	}

	public void OnGroundImpact()
	{
		//TODO: Play Ground Pound Audio

		//Create particle effect.
		GameObject poundPrefab = GameObject.Instantiate<GameObject>(groundPoundPrefab);
		poundPrefab.transform.position = Owner.transform.position;
		GameObject.Destroy(poundPrefab, 3.0f);

		//Debug.Log("Groundpound\n");
		foreach (Player player in GameManager.Instance.players)
		{
			float blastIntensity = CheckBlastIntensity(Owner.transform.position, player, PoundRange, .7f, 1.0f, false, false);
			//Debug.Log("blastIntensity " + blastIntensity + "\n");

			if (blastIntensity > 0 && player.Grounded)
			{
				//Debug.Log("Ground Pounded " + player.name + "\n");

				//Find the direction they're knocked away
				Vector3 knockbackDir = player.transform.position - Owner.transform.position;

				//Knock them away based on the direction, force of the ability and how much of the blast they're affected by
				player.controller.ApplyExternalForce((Vector3.up) * RecoilForce * blastIntensity);

				//Adjust their health based on the blasts damage and how much of the blast.
				player.AdjustHealth(-1 * blastIntensity * SpecialDamage);
			}
		}

		CurrentlySwinging = true;
		SwingTimer = 1f;
	}

}
