﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
	#region Prefabs
	public GameObject playerPrefab;
	#endregion
	public float modifiedTimeScale = 1;

	public const int NUM_PLAYERS = 2;

	public int NumPlayers
	{
		get { return players.Length; }
	}
	public Player[] players;
	public AudioSource music;

	public GameObject[] ArenaPrefabs;
	public GameObject arena;

	public List<Material> PlatformAppearance;
	public static bool spawnRandomArena = false;

	public List<PlayerSpawn> SpawnPoints;

	void Awake()
	{
		LookupPrefabs();

		#region Spawn Players and Arena
		if (spawnRandomArena)
		{
			SpawnRandomArena();
		}

		SpawnPoints = new List<PlayerSpawn>();

		SpawnPoints = GameObject.FindObjectsOfType<PlayerSpawn>().ToList();
		#endregion

		int playerCount = NUM_PLAYERS;
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

		#region Set Player Positions
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
		#endregion

		#region Music
		music = AudioManager.Instance.MakeSource("Meditate_Theme");
		music.volume = .3f;
		music.loop = true;
		music.Play();

		AudioManager.Instance.AddMusicTrack(music, true);
		#endregion
	}

	private void LookupPrefabs()
	{
		playerPrefab = Resources.Load<GameObject>("Player Prefab");

		ArenaPrefabs = Resources.LoadAll<GameObject>("Arenas");
		PlatformAppearance = new List<Material>();
		PlatformAppearance.Add(Resources.Load<Material>("Platform Appearances/Platform Falling"));
		PlatformAppearance.Add(Resources.Load<Material>("Platform Appearances/Platform Heavy Damage"));
		PlatformAppearance.Add(Resources.Load<Material>("Platform Appearances/Platform Damaged"));
		PlatformAppearance.Add(Resources.Load<Material>("Platform Appearances/Platform Whole"));

	}
	private void SpawnRandomArena()
	{
		int arenaNum = Random.Range(0, ArenaPrefabs.Length);
		arena = GameObject.Instantiate<GameObject>(ArenaPrefabs[arenaNum]);

		arena.name = "Arena [" + arenaNum + "]";
		arena.transform.position = Vector3.zero;
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
			players[i].AddAbilityBinding(newGust, players[i].PlayerInput + "Jump");
			
			//X Button
			Smash newSmash = players[i].CreateAbility<Smash>(players[i].PlayerInput + "Secondary", "X");

			//Right Trigger
			Bolt newBolt = players[i].CreateAbility<Bolt>(players[i].PlayerInput + "RBump", "RB");

			//Left Trigger
			Skate newSkate = players[i].CreateAbility<Skate>(players[i].PlayerInput + "LBump", "LB");

			//Y
			WaterShield shield = players[i].CreateAbility<WaterShield>(players[i].PlayerInput + "Special", "Y");
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

				string ritString = "Applying Rituals to " + players[i].name + "\n";
				for (int k = 0; k < rituals.Length; k++)
				{
					//Debug.Log("Applying ritual " + rituals[k].GetType() + " to player " + players[i].playerID + "\n");
					ritString += "  " + rituals[k].GetType().ToString();
					rituals[k].ApplyToPlayer(players[i]);
					playerText.text += (string.IsNullOrEmpty(playerText.text) ? "" : "  ") + (rituals[k].DisplayName);
				}

				Debug.Log(ritString + "\n");
			}
		}
	}

	public void SetAbilityCharges()
	{
		for (int i = 0; i < NumPlayers; i++)
		{
			for (int k = 0; k < players[i].abilities.Count; k++)
			{
				players[i].abilities[k].SetCharges();
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
}