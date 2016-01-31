using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
	#region Prefabs
	public GameObject playerPrefab;
	#endregion
	public float modifiedTimeScale = 1;

	public int NumPlayers
	{
		get { return players.Length; }
	}
	public Player[] players;

	public List<PlayerSpawn> SpawnPoints;

	public enum GamePhase { Intro, Tutorial, EndTutorial, MainPlay, BeginAccuse, Accusation, EndGame };
	public GamePhase currentPhase = GamePhase.Intro;
	public GamePhase CurrentPhase
	{
		get 
		{
			return currentPhase;
		}
		set 
		{
			currentPhase = value;
		}
	}

    void Awake()
	{
		LookupPrefabs();

		SpawnPoints = new List<PlayerSpawn>();

		SpawnPoints = GameObject.FindObjectsOfType<PlayerSpawn>().ToList();

		int playerCount = 1;
		if (PlayerPrefs.HasKey("PlayerCount"))
		{
			playerCount = PlayerPrefs.GetInt("PlayerCount");
		}

		UIManager.Instance.playerUIParents = new RectTransform[playerCount];

		for (int i = 0; i < playerCount; i++)
		{
			AddPlayer(i);
		}

        players = GameObject.FindObjectsOfType<Player>();

		for (int i = 0; i < playerCount; i++)
		{
			if (SpawnPoints.Count > 0)
			{
				PlayerSpawn spawn = SpawnPoints[Random.Range(0, SpawnPoints.Count)];
				players[i].mySpawn = spawn;
				SpawnPoints.Remove(spawn);
			}

			players[i].transform.position = players[i].mySpawn.transform.position;
			players[i].transform.rotation = players[i].mySpawn.transform.rotation;
#if UNITY_EDITOR
			if (players[i].playerID == playerCount - 1)
			{
				players[i].ControlType = Player.PlayerControls.Mouse;
			}
#endif
		}
    }

	private void LookupPrefabs()
	{
		playerPrefab = Resources.Load<GameObject>("Player Prefab");
	}

	public void AddPlayer(int newID)
	{
		//Create the Player Object.
		GameObject playerGO = GameObject.Instantiate<GameObject>(playerPrefab);
		Player newPlayer = playerGO.GetComponent<Player>();
		//newPlayer.transform.position = GetRandomSpawnPosition();

		newPlayer.playerID = newID;
		playerGO.name = "Player " + newID;
	}

	public void AddPlayerAbilities()
	{
		for (int i = 0; i < NumPlayers; i++)
		{
			players[i].Init();
			Gust newGust = (Gust)players[i].CreateAbility("Gust", players[i].PlayerInput + "Primary", "A");
			newGust.MaxCooldown = .5f;
			players[i].AddAbilityBinding(newGust, players[i].PlayerInput + "Jump");
			newGust.MaxCharges = 5;
			newGust.Charges = 5;

			Skate newSkate = (Skate)players[i].CreateAbility("Skate", players[i].PlayerInput + "Secondary", "X");
			newSkate.Cost = 2f;
			newSkate.Force = 32;
			newSkate.MaxCooldown = .05f;
			newSkate.Duration = 5f;
			newSkate.GeneralDamage = 0f;

			Extract newExtract = (Extract)players[i].CreateAbility("Extract", players[i].PlayerInput + "Special", "Y");

			Bolt newBolt = (Bolt)players[i].CreateAbility("Bolt", players[i].PlayerInput + "Right Bumper", "RB");
			newBolt.MaxCooldown = .07f;
			newBolt.MaxAngle = 8;
			newBolt.GeneralDamage = 1.5f;
			newBolt.Cost = 3;
			newBolt.Duration = .35f;


			WaterShield shield = (WaterShield)players[i].CreateAbility("WaterShield", players[i].PlayerInput + "Left Bumper", "LB");
			shield.MaxCooldown = 8f;
			shield.Cost = 15;
			shield.Duration = 4f;
		}


	}

	public void ApplyPlayerRituals()
	{
		for (int i = 0; i < NumPlayers; i++)
		{
			string ritualKey = "P" + players[i].playerID + "Rits";
			if (PlayerPrefs.HasKey(ritualKey))
			{
				long ritLong = long.Parse(PlayerPrefs.GetString(ritualKey));
				
				Ritual[] rituals = Ritual.GetRitualsForIDs((RitualID)ritLong);
				var playerText = GameCanvas.Instance.LookupComponent<UnityEngine.UI.Text>("P" + players[i].playerID + " Name Text");
				playerText.text = "";

				for (int k = 0; k < rituals.Length; k++)
				{
					Debug.Log("Applying ritual " + rituals[k].GetType() + " to player " + players[i].playerID + "\n");
					rituals[k].ApplyToPlayer(players[i]);
					playerText.text += (string.IsNullOrEmpty(playerText.text) ? "" : "  ") + (rituals[k]).ToString();
				}
			}
		}
	}

	void Update()
	{
#if UNITY_EDITOR
		if(Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.K))
			{
				
			}

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				CurrentPhase = GamePhase.MainPlay;
				
			}
		}
#endif
	}

	public void OnDestroy()
	{
		if (PlayerPrefs.HasKey("PlayerCount"))
		{
			PlayerPrefs.DeleteKey("PlayerCount");
		}
		if (PlayerPrefs.HasKey("PlayerCount"))
		{
			PlayerPrefs.DeleteKey("PlayerCount");
		}
	}

	public void DoNothing()
	{

	}
}