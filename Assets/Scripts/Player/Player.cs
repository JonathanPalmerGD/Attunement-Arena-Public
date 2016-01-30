using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public bool initialized;

	public int playerID = 0;
	public enum PlayerControls { Mouse, GamePad}
	public PlayerControls ControlType = PlayerControls.GamePad;
	public string PlayerInput
	{
		get
		{
			if (ControlType == PlayerControls.Mouse)
			{
				return "";
			}
			else if (ControlType == PlayerControls.GamePad)
			{
				return "P" + playerID + " ";
			}

			Debug.LogError("Player Input Selector error\n");
			return "";
		}
	}

	#region Object References
	public RigidbodyFirstPersonController controller;
	//public List<Ability> rituals;

	public GameObject hitscanTarget = null;
	public Vector3 targetScanDir = Vector3.zero;
	public Vector3 hitscanContact = Vector3.zero;

	public PlayerSpawn mySpawn;
	public CameraController cameraController;
	public Camera myCamera;
	#endregion

	#region Properties
	public bool Grounded
	{
		get { return controller.Grounded; }
	}

	public bool damaged = false;

	#region Health & HealthAdj
	private float health;
	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = value;
		}
	}
	private float maxHealth;
	public float MaxHealth
	{
		get
		{
			return maxHealth;
		}
		set
		{
			maxHealth = value;
		}
	}
	private float healthToAdj;
	public float HealthToAdj
	{
		get
		{
			return healthToAdj;
		}
		set
		{
			healthToAdj = value;
		}
	}
	#endregion
	#region Mana & ManaAdj
	private float mana;
	public float Mana
	{
		get
		{
			return mana;
		}
		set
		{
			mana = value;
		}
	}
	private float manaToAdj;
	public float ManaToAdj
	{
		get
		{
			return manaToAdj;
		}
		set
		{
			manaToAdj = value;
		}
	}
	private float maxMana;
	public float MaxMana
	{
		get
		{
			return maxMana;
		}
		set
		{
			maxMana = value;
		}
	}
	#endregion
	#endregion

	#region Ability List and Dict
	public List<Ability> abilities;
	public Dictionary<string, Ability> abilityBindings;
	#endregion

	#region Start, Init and AddAbility
	void Start()
	{
		Init();
	}

	public void Init()
	{
		if (!initialized)
		{
			Mana = 100;
			if (myCamera == null)
			{
				myCamera = GetComponent<Camera>();
			}
			if (cameraController == null)
			{
				cameraController = GetComponent<CameraController>();
			}
			cameraController.Owner = this;
			cameraController.PositionCamera((byte)playerID);

			abilities = new List<Ability>();
			abilityBindings = new Dictionary<string, Ability>();

			controller = GetComponent<RigidbodyFirstPersonController>();
			controller.Owner = this;

			initialized = true;

			UIManager.Instance.ConfigureUISize(this);

			if (ControlType == PlayerControls.Mouse)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}

	public Ability CreateAbility(string abilityName, string keyBinding, string displayKeyBinding)
	{
		Ability newAbility = ScriptableObject.CreateInstance(abilityName) as Ability;

		newAbility.Init(this, keyBinding, displayKeyBinding);

		abilities.Add(newAbility);
		abilityBindings.Add(keyBinding, newAbility);

		return newAbility;
	}

	public void AddAbilityBinding(Ability newAbil, string keyBinding)
	{
		abilityBindings.Add(keyBinding, newAbil);
	}
	#endregion

	public void UseAbility()
	{

	}

	void Update()
	{
		GetInput();

		if (damaged)
		{
			StartCoroutine("DamageFlash");
			//Coroutine to flash the damage screen
		}

		hitscanTarget = TargetScan();

		for (int i = 0; i < abilities.Count; i++)
		{
			abilities[i].UpdateAbility(GameManager.Instance.modifiedTimeScale * Time.deltaTime);
		}
	}

	public void AdjustHealth(float amount)
	{
		if (amount < 0)
		{
			if (!damaged)
			{
				damageFlashTimer = .75f;
			}
			else
			{
				damageFlashTimer += .25f;
			}
			damaged = true;
		}
		if (Health + amount >= MaxHealth)
		{
			HealthToAdj += MaxHealth - Health;
		}
		else
		{
			if (amount < 0)
			{
				HealthToAdj += amount;
			}
			else
			{
				HealthToAdj += amount;
			}
		}
	}

	void UpdateHealth()
	{
		//If our displayed health value isn't correct
		if (HealthToAdj != 0)
		{
			float rate = Time.deltaTime * 8;
			float gainThisFrame = 0;
			if (HealthToAdj < 0)
			{
				if (Health + HealthToAdj < 0)
				{
					rate *= 10;
				}

				//If the health left to adjust is LESS than the rate, just lose the remaining debt.
				gainThisFrame = HealthToAdj > -rate ? HealthToAdj : -rate;
			}
			else
			{
				if (Health + HealthToAdj >= MaxHealth)
				{
					rate *= 4;
				}

				//If the health left to adjust is greater than the rate, use the rate.
				gainThisFrame = HealthToAdj > rate ? rate : HealthToAdj;
			}

			//Note this damage as taken, pay off our debt
			HealthToAdj -= gainThisFrame;

			//Add it to displayed HP
			Health += gainThisFrame;

			//if (Health <= 0 && !isDead)
			//{
			//	KillEntity();
			//}
		}
	}
	void UpdateMana()
	{
		//If our displayed Mana value isn't correct
		if (ManaToAdj != 0)
		{
			float rate = Time.deltaTime * 8;
			float gainThisFrame = 0;
			if (ManaToAdj < 0)
			{
				if (Mana + ManaToAdj < 0)
				{
					rate *= 10;
				}

				//If the Mana left to adjust is LESS than the rate, just lose the remaining debt.
				gainThisFrame = ManaToAdj > -rate ? ManaToAdj : -rate;
			}
			else
			{
				if (Mana + ManaToAdj >= MaxMana)
				{
					rate *= 4;
				}

				//If the Mana left to adjust is greater than the rate, use the rate.
				gainThisFrame = ManaToAdj > rate ? rate : ManaToAdj;
			}

			//Note this Mana spent, pay off our debt
			ManaToAdj -= gainThisFrame;

			//Add it to displayed Mana
			Mana += gainThisFrame;
		}
	}

	GameObject TargetScan()
	{
		//Where the mouse is currently targeting.
		Ray ray = myCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hit;
		//Debug.DrawLine(transform.position, (transform.position + ray) * 100, Color.green);

		//If we fire, set targetScanDir to someplace arbitrarily far away in the shooting. Even if we hit something, we want to target wherever the cursor pointed.
		targetScanDir = transform.position + (ray.direction * 500);

		//Mask so we don't consider targeting ourself.
		LayerMask layerMask = ~((1 << 2) | (1 << 8));
		//LayerMask layerMask = ~(1 << LayerMask.NameToLayer ("Player"));

		//If we hit something
		if (Physics.Raycast(ray, out hit, 1500, layerMask))
		{
			hitscanContact = hit.point;
			//Handle cases for what we hit.

			//Debug.Log(hit.collider.gameObject.tag + "\n");
			//if (hit.collider.gameObject.tag == "NPC")
			//{
			//	NPC n = hit.collider.gameObject.GetComponent<NPC>();
			//	CheckNewTarget((NPC)n);

			//	return n.gameObject;
			//}
			//else if (hit.collider.gameObject.tag == "Enemy")
			//{
			//	Enemy e = hit.collider.gameObject.GetComponent<Enemy>();
			//	CheckNewTarget((Enemy)e);

			//	return e.gameObject;
			//}
			////This is outdated. You used to be able to target terrain.
			//else if (hit.collider.gameObject.tag == "WorldObject")
			//{
			//	//Island e = hit.collider.gameObject.GetComponent<Island>();
			//	//CheckNewTarget((Island)e);
			//}
			//else if (hit.collider.gameObject.tag == "Projectile")
			//{
			//	Projectile p = hit.collider.gameObject.GetComponent<Projectile>();
			//	if (p != null)
			//	{
			//		return p.gameObject;
			//	}
			//}
			//else
			//{
			//Catch all case.
			return hit.collider.gameObject;
			//}
			//Debug.Log(hit.collider.gameObject.name + "\n");
		}
		else
		{
			hitscanContact = targetScanDir;
			//We didn't hit anything. We're about to return null as the default.
			//Debug.Log("Targetting nothing\n");
		}
		return null;
	}

	public void GetInput()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			AdjustHealth(-15);
		}

		#region Jumping
		if (Input.GetButtonDown(PlayerInput + "Jump"))
		{
			//Debug.Log("Hit\n" + abilityBindings.ContainsKey(PlayerInput + "Jump"));
			if (abilityBindings.ContainsKey(PlayerInput + "Jump"))
			{
				//Debug.Log("Hit\n");
				abilityBindings[PlayerInput + "Jump"].ActivateAbilityOverhead(Vector3.down);
			}
		}

		if (Input.GetButtonDown(PlayerInput + "Primary"))
		{
			if (abilityBindings.ContainsKey(PlayerInput + "Primary"))
			{
				abilityBindings[PlayerInput + "Primary"].ActivateAbilityOverhead(targetScanDir);
			}
		}
		#endregion

		#region Cursor Unlocking
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
#endif
		#endregion
	}

	public Image damageIndicator;

	float damageFlashTimer;
	public IEnumerator DamageFlash()
	{
		damageIndicator.gameObject.SetActive(true);
		Color col = damageIndicator.color; // Get color reference
		col.a = 1f;
		damageIndicator.color = col; // Max alpha color

		while (damageFlashTimer > 0)
		{
			damageFlashTimer = Mathf.Clamp(damageFlashTimer, 0, 1.25f);

			damageFlashTimer -= Time.deltaTime;
			col.a = .30f * (damageFlashTimer / 0.75f); // Alpha should fade from full to zero over two seconds
			damageIndicator.color = col; // Set new alpha
			yield return null;
		}
		damaged = false;
		col.a = 0f;
		damageIndicator.color = col;
		damageIndicator.gameObject.SetActive(false);
	}
}
