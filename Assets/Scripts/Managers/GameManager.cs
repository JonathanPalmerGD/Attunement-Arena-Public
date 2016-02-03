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
	public AudioSource music;

	public List<PlayerSpawn> SpawnPoints;

	void Awake()
	{
		LookupPrefabs();

		SpawnPoints = new List<PlayerSpawn>();

		SpawnPoints = GameObject.FindObjectsOfType<PlayerSpawn>().ToList();

		int playerCount = 2;
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
			if (Input.GetJoystickNames().Length < NumPlayers)
			{
				if (players[i].playerID == playerCount - 1)
				{
					//Debug.Log("Joy Count: " + Input.GetJoystickNames().Length + "\n");
					players[i].ControlType = Player.PlayerControls.Mouse;
				}
			}

#if UNITY_EDITOR
			//if (players[i].playerID == playerCount - 1)
			//{
			//	players[i].ControlType = Player.PlayerControls.Mouse;
			//}
#endif
		}

		music = AudioManager.Instance.MakeSource("Meditate_Theme");
		music.volume = .3f;
		music.loop = true;
		music.Play();

		AudioManager.Instance.AddMusicTrack(music, true);
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

			//A/B
			Gust newGust = players[i].CreateAbility<Gust>(players[i].PlayerInput + "Primary", "A B");
			newGust.MaxCooldown = .5f;
			players[i].AddAbilityBinding(newGust, players[i].PlayerInput + "Jump");
			newGust.MaxCharges = 5;
			newGust.Charges = 5;
			
			//X Button
			Extract newExtract = players[i].CreateAbility<Extract>(players[i].PlayerInput + "Secondary", "X");

			//Right Trigger
			Bolt newBolt = players[i].CreateAbility<Bolt>(players[i].PlayerInput + "RTrigger", "RTrig");
			newBolt.MaxCooldown = .07f;
			newBolt.MaxAngle = 8;
			newBolt.GeneralDamage = 1.5f;
			newBolt.Cost = 3;
			newBolt.Duration = .35f;

			//Left Trigger
			Skate newSkate = players[i].CreateAbility<Skate>(players[i].PlayerInput + "LTrigger", "LTrig");
			newSkate.Cost = 2f;
			newSkate.Force = 32;
			newSkate.MaxCooldown = .05f;
			newSkate.Duration = 5f;
			newSkate.GeneralDamage = 0f;

			//Y
			WaterShield shield = players[i].CreateAbility<WaterShield>(players[i].PlayerInput + "Special", "Y");
			shield.MaxCooldown = 8f;
			shield.Cost = 15;
			shield.GeneralDamage = 15f;
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
					//Debug.Log("Applying ritual " + rituals[k].GetType() + " to player " + players[i].playerID + "\n");
					rituals[k].ApplyToPlayer(players[i]);
					playerText.text += (string.IsNullOrEmpty(playerText.text) ? "" : "  ") + (rituals[k]).ToString();
				}
			}
		}
	}

	void Update()
	{

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Delete))
		{
			PlayerPrefs.DeleteAll();
		}
#endif
		if (Input.GetKeyDown(KeyCode.M))
		{
			if (music.volume == 0)
			{
				AudioManager.Instance.maxMusicVol = .3f;
			}
			else
			{
				AudioManager.Instance.maxMusicVol = 0f;
			}
		}
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