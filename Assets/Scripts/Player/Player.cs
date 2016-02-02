using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public bool initialized;

	public int playerID = 0;
	public enum PlayerStatus { Burning, Shielded, Chilled, None }
	public PlayerStatus curStatus = PlayerStatus.None;
	public float curStatusDur;

	public bool playerDead = false;

	public enum PlayerControls { Mouse, GamePad }
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

	public GameObject hitscanTarget = null;
	public Vector3 targetScanDir = Vector3.zero;
	public Vector3 hitscanContact = Vector3.zero;

	public PlayerSpawn mySpawn;
	public CameraController cameraController;
	public Camera myCamera;

	public Scrollbar hpBar;
	public Scrollbar mpBar;
	public ParticleSystem chilledParticles;
	public ParticleSystem shieldParticles;
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
			//Debug.Log("" + hpBar.name + "\n" + Health + " " + MaxHealth + "\n");

			hpBar.size = Health / MaxHealth;
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
			if (value >= MaxMana)
			{
				value = MaxMana;
			}
			if (value < 0)
			{
				value = 0;
			}

			mpBar.size = Mana / MaxMana;
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
	public float ManaRegenRate = 100f;

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
			hpBar = GameCanvas.Instance.LookupComponent<Scrollbar>("P" + playerID + " Health");
			mpBar = GameCanvas.Instance.LookupComponent<Scrollbar>("P" + playerID + " Mana");

			MaxHealth = 100;
			MaxMana = 100;
			Health = 100;
			Mana = 100;

			hpBar.size = Health / MaxHealth;
			mpBar.size = Mana / MaxMana;

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

			shieldParticles.enableEmission = false;
		}
	}

	public T CreateAbility<T>(string keyBinding, string displayKeyBinding) where T : Ability
	{
		//Debug.Log("Creating " + abilityName + " " + keyBinding + "\n");
		T newAbility = ScriptableObject.CreateInstance(typeof(T)) as T;

		newAbility.Init(this, keyBinding, displayKeyBinding);

		abilities.Add(newAbility);
		abilityBindings.Add(keyBinding, newAbility);

		return newAbility;
	}
	public T GetAbility<T>(bool createIfNotFound = false) where T : Ability
	{
		//When a player sets an ability to a button bind PlayerInput+AbilityName

		T abil = abilities.FirstOrDefault(a => a is T) as T;
		//	?? CreateAbility<T>(PlayerInput + "Secondary", "X");

		//The section after the ?? is the same as below:
		if (abil == null && createIfNotFound)
		{
		//Create the ability
			abil = CreateAbility<T>(PlayerInput + "Secondary", "X");
		}

		return abil;
	}

	public void AddAbilityBinding(Ability newAbil, string keyBinding)
	{
		abilityBindings.Add(keyBinding, newAbil);
	}
	#endregion

	void Update()
	{
		UpdateStatus();

		UpdateHealth();
		UpdateMana();

		if (playerDead) return;

		GetInput();

		if (damaged)
		{
			if (damageIndicator == null)
			{
				damageIndicator = GameCanvas.Instance.LookupComponent<Image>("P" + playerID + " Damage Indicator");

			}
			StartCoroutine("DamageFlash");
			//Coroutine to flash the damage screen
		}

		hitscanTarget = TargetScan();

		for (int i = 0; i < abilities.Count; i++)
		{
			abilities[i].UpdateAbility(GameManager.Instance.modifiedTimeScale * Time.deltaTime);
		}
	}

	public void UpdateStatus()
	{
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.L))
		{
			SetPlayerStatus(PlayerStatus.Chilled, 5, true);
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			SetPlayerStatus(PlayerStatus.Shielded, 5, true);
		}
#endif

		#region Chilled
		if (curStatus == PlayerStatus.Chilled)
		{
			curStatusDur -= Time.deltaTime;
			AdjustHealth(-1 * Time.deltaTime, false);

			if (curStatusDur <= 0)
			{
				curStatusDur = 0;
				curStatus = PlayerStatus.None;
			}

			chilledParticles.enableEmission = true;
		}
		if (curStatus != PlayerStatus.Chilled)
		{
			chilledParticles.enableEmission = false;
		}
		#endregion
		#region Shielded
		if (curStatus == PlayerStatus.Shielded)
		{
			curStatusDur -= Time.deltaTime;

			if (curStatusDur <= 0)
			{
				curStatusDur = 0;
				curStatus = PlayerStatus.None;
			}

			shieldParticles.enableEmission = true;
		}
		else
		{
			shieldParticles.enableEmission = false;
		}
		#endregion
	}

	/// <summary>
	/// Affect the player's health
	/// </summary>
	/// <param name="amount">Positive for healing, negative for damage</param>
	public void AdjustHealth(float amount, bool causeFlicker = true)
	{
		if (playerDead) { return; }

		if (curStatus == PlayerStatus.Shielded)
		{
			WaterShield shield = GetAbility<WaterShield>();

			if (shield)
			{
				//How much damage was prevented
				float damagePrevented = amount * shield.damageReduction;

				//How much gets through
				amount = amount * (1-shield.damageReduction);

				//Reduce shield duration.
				// The percentage lost is the amount of base duration lost.
				curStatusDur -= (damagePrevented / MaxHealth) * shield.Duration;
			}
		}

		if (amount < 0)
		{
			if (causeFlicker)
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

				if (Health + amount <= 0)
				{
					KillPlayer();
				}

			}
			else
			{
				HealthToAdj += amount;
			}
		}
	}

	public void AdjustMana(float amount)
	{
		if (Mana + amount >= MaxMana)
		{
			ManaToAdj += MaxMana - Mana;
		}
		else
		{
			if (amount < 0)
			{
				ManaToAdj += amount;
			}
			else
			{
				ManaToAdj += amount;
			}
		}
	}

	void KillPlayer()
	{
		playerDead = true;

		// Stop the damage flashing
		damaged = false;
		damageFlashTimer = 0f;
		StopAllCoroutines();

		// Give the player a darkened "you're dead" screen
		damageIndicator.gameObject.SetActive(true);
		damageIndicator.color = new Color(0.5f, 0f, 0f, 0.3f);
		StartCoroutine(DeadTextFadein(GameCanvas.Instance.LookupComponent<Text>("P" + playerID + " DeadText")));

		// Test live player count
		byte livePlayers = 0;
		foreach (Player p in GameManager.Instance.players)
		{
			if (!p.playerDead) livePlayers++;
		}

		// We have winrar!
		if (livePlayers < 2)
		{
			foreach (Player p in GameManager.Instance.players)
			{
				if (p.playerDead) continue;

				Text winText = GameCanvas.Instance.LookupComponent<Text>("P" + p.playerID + " DeadText");
				winText.text = "You are an Attunement Master!";
				p.StartCoroutine(p.DeadTextFadein(winText));
				p.damageIndicator.gameObject.SetActive(true);
				p.damageIndicator.color = new Color(1.0f, 0.75f, 0f, 0.3f);
				break;
			}
			StartCoroutine(WaitThenChangeScene());  // Change back to main menu upon finish
		}
	}

	void UpdateHealth()
	{
		//If our displayed health value isn't correct
		if (HealthToAdj != 0)
		{
			float rate = Time.deltaTime * 30;
			float gainThisFrame = 0;
			if (HealthToAdj < 0)
			{
				if (Health + HealthToAdj < 0)
				{
					rate *= 5;
				}

				//If the health left to adjust is LESS than the rate, just lose the remaining debt.
				gainThisFrame = HealthToAdj > -rate ? HealthToAdj : -rate;
			}
			else
			{
				if (Health + HealthToAdj >= MaxHealth)
				{
					rate *= 2;
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
		if (playerDead)
			return;

		if (controller.Grounded)
		{
			ManaToAdj += Time.deltaTime * ManaRegenRate * 1.6f;
		}
		else
		{
			ManaToAdj += Time.deltaTime * ManaRegenRate * 0.4f;
		}


		//If our displayed Mana value isn't correct
		if (ManaToAdj != 0)
		{
			float rate = Time.deltaTime * 10;
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

	public void SetPlayerStatus(PlayerStatus targetStatus, float statusDur = 0, bool maxDuration = false)
	{
		//If we're in the current status. Increase the duration
		if (curStatus == targetStatus)
		{
			//Override the remaining duration if we can't overflow
			if (maxDuration)
			{
				curStatusDur = statusDur;
			}
			else
			{
				//Otherwise straight add it
				curStatusDur += statusDur;
			}
		}
		//If we aren't the right status, decrease the duration
		else if (curStatus != targetStatus)
		{
			//Decrease the old status duration
			if (curStatusDur >= statusDur)
			{
				curStatusDur -= statusDur;
			}
			else
			{
				//If the new status lasts longer, reduce the new status's duration and switch the status
				curStatusDur = statusDur - curStatusDur;
				curStatus = targetStatus;
			}
		}

		if (curStatusDur <= 0 || targetStatus == PlayerStatus.None)
		{
			curStatusDur = 0;
			curStatus = PlayerStatus.None;
		}
	}

	public void SetGustCharges(int newChargeCount, bool canReduceCharges = false)
	{
		for (int i = 0; i < abilities.Count; i++)
		{
			if (abilities[i].GetType() == typeof(Gust))
			{
				if (newChargeCount < 0)
					newChargeCount = 0;
				if (newChargeCount > abilities[i].MaxCharges)
					newChargeCount = abilities[i].MaxCharges;

				if (canReduceCharges)
				{
					abilities[i].Charges = newChargeCount;
				}
				else if (newChargeCount > abilities[i].Charges)
				{
					abilities[i].Charges = newChargeCount;
				}
			}
		}
	}

	GameObject TargetScan()
	{
		//Where the mouse is currently targeting.
		Ray ray = myCamera.ViewportPointToRay(Vector3.one * 0.5f);
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
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.G))
		{
			AdjustHealth(-15);
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			AdjustHealth(100);
			Mana += 100;
		}
#endif

		for (int i = 0; i < abilities.Count; i++)
		{
			//Debug.Log(abilities[i].GetType() + "   " + abilities[i].activationCond + "\n");
			if (abilities[i].activationCond == Ability.KeyActivateCond.KeyDown)
			{
				//Debug.Log("["+ PlayerInput +"][" + abilities[i].keyBinding + "]\n");
				if (Input.GetButtonDown(abilities[i].keyBinding))
				{
					abilityBindings[abilities[i].keyBinding].ActivateAbilityOverhead(targetScanDir);
				}
			}
			else if (abilities[i].activationCond == Ability.KeyActivateCond.KeyHold)
			{
				//Debug.Log("Button Down ["+ PlayerInput +"][" + abilities[i].keyBinding + "]\n");
				if (Input.GetButton(abilities[i].keyBinding))
				{
					abilityBindings[abilities[i].keyBinding].ActivateAbilityOverhead(targetScanDir);
				}
			}
			else if (abilities[i].activationCond == Ability.KeyActivateCond.GetAxis)
			{
				float axisValue = Input.GetAxis(abilities[i].keyBinding);
				//Debug.Log("Hit\n" + axisValue);
				if (axisValue > .2f)
				{
					abilityBindings[abilities[i].keyBinding].ActivateAbilityOverhead(targetScanDir);
				}
			}
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

		//if (Input.GetButtonDown(PlayerInput + "Primary"))
		//{
		//	if (abilityBindings.ContainsKey(PlayerInput + "Primary"))
		//	{
		//		abilityBindings[PlayerInput + "Primary"].ActivateAbilityOverhead(targetScanDir);
		//	}
		//}
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

	public IEnumerator DeadTextFadein(Text deadText)
	{
		deadText.gameObject.SetActive(true);
		deadText.color = new Color(1f, 0f, 0f, 0f);

		for (float alpha = 0f; alpha < 1f; alpha += Time.deltaTime * 0.25f)
		{
			deadText.color = new Color(1f, 0f, 0f, alpha);
			yield return null;
		}
	}

	public IEnumerator WaitThenChangeScene()
	{
		yield return new WaitForSeconds(8.0f);
		UIManager.Instance.SafeCleanup();
		GameManager.Instance.SafeCleanup();
		AudioManager.Instance.SafeCleanup();

		Application.LoadLevel(Application.loadedLevel - 1);
	}
}
