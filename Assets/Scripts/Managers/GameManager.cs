using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
	public string GetPlayerTitle(int number)
	{
		if (number == 0)
		{
			return "Traveler";
		}
		else if (number == 1)
		{
			return "Diplomat";
		}
		else if (number == 2)
		{
			return "Historian";
		}
		else if (number == 3)
		{
			return "Scientist";
		}

		return "Error";
	}

	public Dictionary<string, Vector3> npcPositions;
	public Dictionary<string, GameObject> NPCs;

	public List<GameObject> NPCsToMove;
	public List<Vector3> targetPlaces;

	public GameObject mainScene;
	public GameObject accuseScene;

	public bool overrideMove = false;

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
			//TODO: NETWORK ME


			currentPhase = value;
		}
	}

	public void ChangePhase(GamePhase targetPhase)
	{

	}

	#region Game Phases
	//This will be called when all players are successfully in the game.
	public void BeginMurder()
	{
		
	}

	public void BeginMainPlay()
	{
		
	}

	public void BeginAccusation()
	{
		
	}
	#endregion

	#region Player and NPC Lookup
	public string GetCharacterRole(string characterName)
	{
		if (characterName == "Yukiko Sanada" || characterName == "Diplomat")
		{
			return "Diplomat";
		}
		else if (characterName == "Shu Kouri" || characterName == "Historian")
		{
			return "Historian";
		}
		else if (characterName == "Lawrence Woodman" || characterName == "Scientist")
		{
			return "Scientist";
		}
		else if (characterName == "Penelope White" || characterName == "Maverick")
		{
			return "Maverick";
		}
		Debug.LogError("Unable to convert " + characterName + " to a character role\n");
		return "ErrorCase";
	}

	public string GetCharacterName(string characterRole)
	{
		if (characterRole == "Yukiko Sanada" || characterRole == "Diplomat")
		{
			return "Yukiko Sanada";
		}
		else if (characterRole == "Shu Kouri" || characterRole == "Historian")
		{
			return "Shu Kouri";
		}
		else if (characterRole == "Lawrence Woodman" || characterRole == "Scientist")
		{
			return "Lawrence Woodman";
		}
		else if (characterRole == "Penelope White" || characterRole == "Maverick")
		{
			return "Penelope White";
		}
		Debug.LogError("Unable to convert " + characterRole + " to a character role\n");
		return "ErrorCase";
	}

	public void LookupNPCs()
	{
		npcPositions = new Dictionary<string, Vector3>();
		NPCs = new Dictionary<string, GameObject>();

		GameObject npcParent = GameObject.Find("NPCs");

		for (int i = 0; i < npcParent.transform.childCount; i++)
		{
			Transform child = npcParent.transform.GetChild(i);
			NPCs.Add(child.name, child.gameObject);
		}

		GameObject positionParent = GameObject.Find("NPC Positions");

		for (int i = 0; i < positionParent.transform.childCount; i++)
		{
			Transform child = positionParent.transform.GetChild(i);
			npcPositions.Add(child.name, child.position);
			//Debug.Log(child.name);
		}
	}

	public GameObject GetNPC(string NPCName)
	{
		if (NPCs.ContainsKey(NPCName))
		{
			return NPCs[NPCName];
		}
		return null;
	}
	#endregion

	#region Entity Movement
	public void MoveNPCToPosition(string NPCName, string positionName)
	{
		//Debug.Log("Hit\n  " + NPCs.ContainsKey(name) + "   " + npcPositions.ContainsKey(positionName));

		//If the NPC and target position exist, move the npc
		if (NPCs.ContainsKey(NPCName) && npcPositions.ContainsKey(positionName))
		{
			//TODO: Kush, network the contents of these two lists.
			//if (GameManager.Instance.Multiplayer)
			//{
			//	// Sending Player Index is of no use now. Maybe, can be used in future
			//	//PlayerController localPlayer = PlayerController.getNetworkPlayer();
			//	localPlayer.CmdMoveNPCToPosition(localPlayer.playerIndex, NPCName, positionName);
			//}
			//else
			//{
			//	//We have validated, move them into the lists for NPCsToMove.
			//	NPCsToMove.Add(NPCs[NPCName]);
			//	targetPlaces.Add(npcPositions[positionName]);
			//}
		}
	}

	public void MovePlayersToPositions(string[] positionName)
	{
		//for (int i = 0; i < positionName.Length; i++)
		//{
		//	//Debug.Log("Hit\n" + i + " " + positionName[i]);
		//	//1 player is i = 0.
		//	//2 player is i = 1.
		//	if (PlayerController.GetNumPlayers() > i)
		//	{
		//		//If the NPC and target position exist, move the npc
		//		if (PlayerController.GetPlayerByID(i) != null && npcPositions.ContainsKey(positionName[i]))
		//		{
		//			//We have validated, move them into the lists for NPCsToMove.
		//			//TODO: Check to make sure we can move players in a networked setting?
		//			NPCsToMove.Add(PlayerController.GetPlayerByID(i).gameObject);
		//			targetPlaces.Add(npcPositions[positionName[i]]);
		//		}
		//	}
		//	else
		//	{
		//		//Debug.Log("Failure: " + name + "\n" + positionName[i] + "\n");
		//	}
		//}
	}
	#endregion

	public bool CheckNPCPosition(string NPCName, string positionName, float allowedDistance = 2)
	{
		if (NPCs.ContainsKey(NPCName) && npcPositions.ContainsKey(positionName))
		{
			Vector3 pos = NPCs[NPCName].transform.position;
			Vector3 pos2 = npcPositions[positionName];
			//Debug.DrawLine(pos, pos2, Color.green, 5.0f);

			float distance = Vector3.Distance(new Vector3(pos.x, pos.y, 0), new Vector3(pos2.x, pos2.y, 0));

			if (distance < allowedDistance)
			{
				return true;
			}
		}
		//Debug.Log(name + " is not at " + positionName + "\n");
		return false;
	}

	void Start()
	{
		//mainScene = SubSceneParent.Instance.LookupSubScene("Main Scene");
		//accuseScene = SubSceneParent.Instance.LookupSubScene("Accuse Scene");
		//mainScene.SetActive(true);
		//accuseScene.SetActive(false);

		NPCsToMove = new List<GameObject>();
		targetPlaces = new List<Vector3>();

		//socks = 30;
		LookupNPCs();
		//Debug.Log(NPCs.Count +"\n");
	}
	void Update()
	{
		ProcessNPCRelocation();

		BeginMurder();
		BeginMainPlay();
		BeginAccusation();

#if UNITY_EDITOR
		if(Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.K))
			{
				//InvManager.Instance.DemandItem(26);

				//if (Input.GetKey(KeyCode.LeftShift))
				//{
				//	MoveNPCToPosition("Chartreuse", "Chartreuse Pos 1");
				//}
				//else
				//{
				//	MoveNPCToPosition("Chartreuse", "Chartreuse Pos 2");
				//}
			}

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				CurrentPhase = GamePhase.MainPlay;
				//AccuseNPC("Castform");
			}
		}
#endif
	}

	public void ProcessNPCRelocation()
	{
		if (!overrideMove)
		{
			//Do nothing
		}
		else
		{
			//If we have NPCs to move
			if (NPCsToMove.Count > 0)
			{
				//If we're in normal display
				if (UIManager.Instance.NormalDisplay())
				{
					//Start fading
					UIManager.Instance.BeginFade(.4f, .4f, .4f);
				}
				//If we finished fading out
				else if (UIManager.Instance.FadedOut())
				{
					//Change their position

					for (int i = 0; i < NPCsToMove.Count; i++)
					{
						Vector3 xyz = new Vector3(targetPlaces[i].x, targetPlaces[i].y, NPCsToMove[i].transform.position.z);
						NPCsToMove[i].transform.position = xyz;
					}

					//Clear the list of NPCs to move.
					overrideMove = false;
					NPCsToMove.Clear();
					targetPlaces.Clear();
				}
			}
		}
	}

	public void DoNothing()
	{

	}
}