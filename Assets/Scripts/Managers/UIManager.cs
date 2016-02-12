using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class UIManager : Singleton<UIManager>
{
	#region Variables & Assets
	#region Assets
	#region Static Prefabs & Assets
	public static GameObject gameUIPrefab;
	public static GameObject playerUIPrefab;
	public static Sprite[] Icons;
	#endregion

	TextAsset itemXML;
	#endregion

	#region Ingame UI references
	public GameObject inGameUI;

	public RectTransform[] playerUIParents;
	public PopulateContainer[] PlayerAbilityDisplay;
	
	public Image transitionImage;

	public CanvasGroup WinState;
	public CanvasGroup LoseState;
	#endregion

	#region Menu States
	public enum MenuState { Closed, Open, Suppressed, Error }
	//public MenuState journalState;
	#endregion

	#region Fade Controls
	public enum DisplayTransition { NormalDisplay, FadingOut, FadingIn, Darkened};
	public DisplayTransition fadingTrans = DisplayTransition.NormalDisplay;
	public float fadeCounter = 0;
	public float fadeOutDuration = .2f;
	public float fadeInDuration = .2f;
	public float fadeHoldCounter = 0;
	public float fadeHoldDuration = .2f;
	#endregion
	#endregion

	private bool initialized = false;

	#region Awake, Start & Update
	void Awake()
	{
		LookupPrefabs();
		GameObject go = GameObject.Find("In-Game UI - Canvas");

		if (go == null)
		{
			inGameUI = GameObject.Instantiate<GameObject>(gameUIPrefab);
			go.name = "In-Game UI - Canvas";
		}
		else
		{
			inGameUI = go;
			go.name = "In-Game UI - Canvas";
		}

		for (int i = 0; i < GameManager.Instance.NumPlayers; i++)
		{
			AddPlayerUI(GameManager.Instance.players[i]);
		}
		
	}

	private void LookupPrefabs()
	{
		gameUIPrefab = Resources.Load<GameObject>("UI/In-Game UI - Canvas");
		playerUIPrefab = Resources.Load<GameObject>("UI/Player UI");

		Icons = Resources.LoadAll<Sprite>("Icons");
	}


	public void Init()
	{
		if (!initialized)
		{
			PlayerAbilityDisplay = new PopulateContainer[GameManager.Instance.NumPlayers];

			for (int i = 0; i < PlayerAbilityDisplay.Length; i++)
			{
				
				PlayerAbilityDisplay[i] = GameCanvas.Instance.LookupComponent<PopulateContainer>("P" + i + " Ability Parent");
			}

			for (int i = 0; i < GameManager.Instance.NumPlayers; i++)
			{
				GameManager.Instance.players[i].damageIndicator = GameCanvas.Instance.LookupComponent<Image>("P" + GameManager.Instance.players[i].playerID + " Damage Indicator");
				GameManager.Instance.players[i].damageIndicator.gameObject.SetActive(false);
				GameCanvas.Instance.LookupComponent<Text>("P" + GameManager.Instance.players[i].playerID + " DeadText").gameObject.SetActive(false);
			}

			GameManager.Instance.AddPlayerAbilities();
			GameManager.Instance.ApplyPlayerRituals();
			GameManager.Instance.SetAbilityCharges();

			initialized = true;
		}
	}

	public Vector2 anchMinp0 = new Vector2(0, .5f);
	public Vector2 anchMaxp0 = new Vector2(1, 1);
	public Vector2 anchMinp1 = new Vector2(0, 0);
	public Vector2 anchMaxp1 = new Vector2(1, .5f);

	public Vector2 anchMinp04 = new Vector2(0, .5f);
	public Vector2 anchMaxp04 = new Vector2(.5f, 1);

	public Vector2 anchMinp14 = new Vector2(.5f, .5f);
	public Vector2 anchMaxp14 = new Vector2(1, 1);

	public Vector2 anchMinp24 = new Vector2(0, 0);
	public Vector2 anchMaxp24 = new Vector2(.5f, .5f);

	public Vector2 anchMinp34 = new Vector2(.5f, 0);
	public Vector2 anchMaxp34 = new Vector2(1, .5f);

	public void ConfigureUISize(Player player)
	{
		int id = player.playerID;

		#region TWO PLAYERS
		//P0
		//Min	0		.5
		//Max	1		1
		
		//P1
		//Min	0		0
		//Max	1		.5
		#endregion

		#region FOUR PLAYERS
		//P0					//P1
		//Min	0		.5		//Min	.5		.5
		//Max	.5		1		//Max	1		1	

		//P2					//P3
		//Min	0		0		//Min	.5		0
		//Max	.5		.5		//Max	1		.5	
		#endregion

		if (GameManager.Instance.NumPlayers == 2)
		{
			if (player.playerID == 0)
			{
				playerUIParents[id].anchorMin = anchMinp0;
				playerUIParents[id].anchorMax = anchMaxp0;
			}
			else
			{
				playerUIParents[id].anchorMin = anchMinp1;
				playerUIParents[id].anchorMax = anchMaxp1;
			}
		}
		else if (GameManager.Instance.NumPlayers == 3)
		{
			if (player.playerID == 0)
			{
				playerUIParents[id].anchorMin = anchMinp04;
				playerUIParents[id].anchorMax = anchMaxp04;
			}
			else if (player.playerID == 1)
			{
				playerUIParents[id].anchorMin = anchMinp14;
				playerUIParents[id].anchorMax = anchMaxp14;
			}
			else if (player.playerID == 2)
			{
				playerUIParents[id].anchorMin = anchMinp24;
				playerUIParents[id].anchorMax = anchMaxp24;
			}
		}
		else if (GameManager.Instance.NumPlayers == 4)
		{
			if (player.playerID == 0)
			{
				playerUIParents[id].anchorMin = anchMinp04;
				playerUIParents[id].anchorMax = anchMaxp04;
			}
			else if (player.playerID == 1)
			{
				playerUIParents[id].anchorMin = anchMinp14;
				playerUIParents[id].anchorMax = anchMaxp14;
			}
			else if (player.playerID == 2)
			{
				playerUIParents[id].anchorMin = anchMinp24;
				playerUIParents[id].anchorMax = anchMaxp24;
			}
			else if (player.playerID == 3)
			{
				playerUIParents[id].anchorMin = anchMinp34;
				playerUIParents[id].anchorMax = anchMaxp34;
			}
		}

		playerUIParents[id].offsetMin = playerUIParents[id].offsetMax = Vector2.zero;
	}

	public void AddPlayerUI(Player newPlayer)
	{
		//Debug.Log("TODO: Create player UI per player\n");
		GameObject UIGameObject = GameObject.Instantiate(playerUIPrefab);

		int id = newPlayer.playerID;
		UIGameObject.transform.SetParent(inGameUI.transform);
		playerUIParents[id] = UIGameObject.GetComponent<RectTransform>();
		playerUIParents[id].localScale = Vector3.one;

		//EXTREMELY IMPORTANT: We use .Name, which is a special thing of UIComponents.
		playerUIParents[newPlayer.playerID].GetComponent<UIComponent>().Name = "Player UI";
		RecursiveChildNaming(newPlayer.playerID, playerUIParents[newPlayer.playerID]);

	}

	private void RecursiveChildNaming(int id, Transform target)
	{
		//This entire method is messy. Be careful if you change.
		
		string oldName = target.name;
		string newName = target.name.Replace("Player", "P" + id);

		//Debug.Log("Comparing " + oldName + "  " + newName + "\n" + (newName != oldName));
		if (newName != oldName)
		{
			UIComponent comp = target.GetComponent<UIComponent>();
			if(comp)
			{
				//EXTREMELY IMPORTANT: We use .Name, which is a special thing of UIComponents.
				comp.Name = newName;
			}
		}

		for (int i = 0; i < target.childCount; i++)
		{
			RecursiveChildNaming(id, target.GetChild(i));
		}
	}

	public AbilityDisplayUI AddAbilityDisplay(Ability ability)
	{
		//Find the parent ID.
		int id = ability.Owner.playerID;
		AbilityDisplayUI aDisUI = null;
		
		GameObject go = PlayerAbilityDisplay[id].AddPrefabToContainerReturn();
		aDisUI = go.GetComponent<AbilityDisplayUI>();

		//We're adding this here as a bit of an unnecessary step to make sure it's set.
		ability.abilDispUI = aDisUI;

		return aDisUI;
	}

	void Update()
	{
		HandleFade();

		#region Escape Key
		if (Input.GetButtonDown("Quit"))
		{
			Application.Quit();
		}
		#endregion

		#region Editor Keys
		#if UNITY_EDITOR
		//if (Input.GetKeyDown(KeyCode.H))
		//{
		//	BeginFade(.35f);
		//}
		#endif
		#endregion
	}
	#endregion

	#region Transitions
	public void BeginFade(float fadeOutDur = .8f, float fadeHoldDur = .8f, float fadeInDur = .8f)
	{
		fadeCounter = 0;
		fadeHoldCounter = 0;
		fadeInDuration = fadeInDur;
		fadeHoldDuration = fadeHoldDur;
		fadeOutDuration = fadeOutDur;

		fadingTrans = DisplayTransition.FadingOut;
	}

	public void HandleFade()
	{
		if (fadingTrans != DisplayTransition.NormalDisplay)
		{
			float fadePercentage = 0;
			if (fadingTrans == DisplayTransition.FadingOut)
			{
				fadeCounter += Time.deltaTime;
				if (fadeCounter >= fadeOutDuration)
				{
					fadingTrans = DisplayTransition.Darkened;
					fadeCounter = fadeInDuration;
				}
				fadePercentage = fadeCounter / fadeOutDuration;
			}
			if (fadingTrans == DisplayTransition.Darkened)
			{
				fadeHoldCounter += Time.deltaTime;
				if (fadeHoldCounter >= fadeHoldDuration)
				{
					fadingTrans = DisplayTransition.FadingIn;
				}

				fadePercentage = 1;
			}
			
			if (fadingTrans == DisplayTransition.FadingIn)
			{
				fadeCounter -= Time.deltaTime;
				if (fadeCounter <= 0)
				{
					fadingTrans = DisplayTransition.NormalDisplay;
					fadeCounter = 0;
				}

				fadePercentage = fadeCounter / fadeInDuration;
			}

			transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, fadeCounter / fadeInDuration);
		}
	}

	public bool FadedOut()
	{
		if (fadingTrans == DisplayTransition.Darkened)
		{
			return true;
		}
		return false;
	}

	public bool NormalDisplay()
	{
		if (fadingTrans == DisplayTransition.NormalDisplay)
		{
			return true;
		}
		return false;
	}
	#endregion
}