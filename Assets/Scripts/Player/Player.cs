using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public bool initialized;

	public int playerID = 0;
	public enum PlayerBuff { Burning, Shielded, Chilled, None }
	public PlayerBuff buffState = PlayerBuff.None;
	public float remainingStatDur;

	public bool playerDead = false;

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
	/// <summary>
	/// A Flagged Enum that has all of the player's selected Rituals
	/// </summary>
	public RitualID rituals;

	public GameObject hitscanTarget = null;
	public Vector3 targetScanDir = Vector3.zero;
	public Vector3 hitscanContact = Vector3.zero;

	public PlayerSpawn mySpawn;
	public CameraController cameraController;
	public Camera myCamera;

	public Scrollbar hpBar;
	public Scrollbar mpBar;
	public ParticleSystem chilledParticles;
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
		}
	}

	public Ability CreateAbility(string abilityName, string keyBinding, string displayKeyBinding)
	{
		//Debug.Log("Creating " + abilityName + " " + keyBinding + "\n");
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
		UpdateBuffState();

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

	public void UpdateBuffState()
	{
		if (Input.GetKeyDown(KeyCode.L))
		{
			SetChilledState(5);
		}

		if (buffState != PlayerBuff.None)
		{
			if (buffState == PlayerBuff.Chilled)
			{
				remainingStatDur -= Time.deltaTime;
				AdjustHealth(-2 * Time.deltaTime, false);
			}

			if (remainingStatDur <= 0)
			{
				remainingStatDur = 0;
				buffState = PlayerBuff.None;
			}
		}
		else
		{
			chilledParticles.enableEmission = false;
		}
	}

	/// <summary>
	/// Affect the player's health
	/// </summary>
	/// <param name="amount">Positive for healing, negative for damage</param>
	public void AdjustHealth(float amount, bool causeFlicker = true)
	{
		if (playerDead) return;

		if (amount < 0 && buffState != PlayerBuff.Shielded)
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
				if (buffState == PlayerBuff.Shielded)
				{
					buffState = PlayerBuff.None;
				}
				else
				{
					HealthToAdj += amount;

					if(Health + amount <= 0)
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
						foreach(Player p in GameManager.Instance.players)
						{
							if (!p.playerDead) livePlayers++;
						}

						// We have winrar!
						if(livePlayers == 1)
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
		if (!playerDead)
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

	public void SetChilledState(float chillDur)
	{
		if(buffState == PlayerBuff.None || buffState == PlayerBuff.Chilled)
		{
			Debug.Log("hit\n");
			remainingStatDur = chillDur;
			chilledParticles.enableEmission = true;
			buffState = PlayerBuff.Chilled;
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
				else if(newChargeCount > abilities[i].Charges)
				{
					abilities[i].Charges = newChargeCount;
				}
			}
		}
	}

	GameObject TargetScan()
	{
		//Where the mouse is currently targeting.
		Ray ray = myCamera.ScreenPointToRay(new Vector3(myCamera.rect.width * Screen.width / 2, myCamera.rect.height * Screen.height / 2));
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
		if (Input.GetKeyDown(KeyCode.Z))
		{
			AdjustHealth(100);
			Mana += 100;
		}

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
			else if(abilities[i].activationCond == Ability.KeyActivateCond.GetAxis)
			{

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

		for(float alpha = 0f; alpha < 1f; alpha += Time.deltaTime * 0.25f)
		{
			deadText.color = new Color(1f, 0f, 0f, alpha);
			yield return null;
		}
	}

	public IEnumerator WaitThenChangeScene()
	{
		yield return new WaitForSeconds(8.0f);
		Application.LoadLevel(Application.loadedLevel - 1);
	}
}
