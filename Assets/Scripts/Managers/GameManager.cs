using UnityEngine;
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
			players[i].AddAbility("Gust", "Player " + i + " Q", "Q");
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