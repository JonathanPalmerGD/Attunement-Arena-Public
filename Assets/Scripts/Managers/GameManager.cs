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
			players[i].AddAbilityBinding(newGust, players[i].PlayerInput + "Jump");

			//RITUAL:
				//If the player doesn't have Gust
					//Give them Gust
				//Modify Gust

			//Gust newGust = (Gust)players[i].AddAbility("Gust", players[i].PlayerInput + "Jump", "Jump");
			newGust.MaxCharges = 5;
			newGust.Charges = 5;

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
	}

	public void DoNothing()
	{

	}
}