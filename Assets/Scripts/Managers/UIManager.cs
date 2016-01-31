﻿using UnityEngine;
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
		gameUIPrefab = Resources.Load<GameObject>("In-Game UI - Canvas");
		playerUIPrefab = Resources.Load<GameObject>("Player UI");
			//buttonPrefab = Resources.Load<GameObject>("GivePlayerButton");
		//itemPrefab = Resources.Load<GameObject>("Item Entry");
		//infoPrefab = Resources.Load<GameObject>("Info Entry");
		//solutionUIPrefab = Resources.Load<GameObject>("Display Solution");
		//reqUIPrefab = Resources.Load<GameObject>("Display Requirement");

		Icons = Resources.LoadAll<Sprite>("Icons");
	}

	void Start()
	{
		//LoseState = GameCanvas.Instance.LookupComponent<CanvasGroup>("Lose State");
		//WinState = GameCanvas.Instance.LookupComponent<CanvasGroup>("Win State");
		//LoseState.alpha = 0;
		//WinState.alpha = 0;

		//InitButtonListeners();
	}

	public void Init()
	{
		if (!initialized)
		{
			PlayerAbilityDisplay = new PopulateContainer[GameManager.Instance.NumPlayers];

			for (int i = 0; i < PlayerAbilityDisplay.Length; i++)
			{
				//Debug.Log("Looking up: " + "P" + i + " Ability Parent" + "\n" + GameCanvas.Instance.compDict.Count);

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

		else if (GameManager.Instance.NumPlayers == 4)
		{
			if (player.playerID == 0)
			{
				playerUIParents[id].anchorMin = anchMinp04;
				playerUIParents[id].anchorMax = anchMaxp04;
			}
			else if(player.playerID == 1)
			{
				playerUIParents[id].anchorMin = anchMinp14;
				playerUIParents[id].anchorMax = anchMaxp14;
			}
			else if(player.playerID == 2)
			{
				playerUIParents[id].anchorMin = anchMinp24;
				playerUIParents[id].anchorMax = anchMaxp24;
			}
			else if(player.playerID == 3)
			{
				playerUIParents[id].anchorMin = anchMinp34;
				playerUIParents[id].anchorMax = anchMaxp34;
			}
		}

		playerUIParents[id].offsetMin = playerUIParents[id].offsetMax = Vector2.zero;

		//playerUIParents[id].anchorMin = new Vector2(player.myCamera.rect.x, player.myCamera.rect.y);
		//playerUIParents[id].anchorMax = new Vector2(player.myCamera.rect.width, player.myCamera.rect.y - player.myCamera.rect.width);
		//playerUIParents[id].offsetMin = new Vector2(player.myCamera.rect.x * Screen.width, player.myCamera.rect.width);
		//playerUIParents[id].offsetMax = new Vector2(player.myCamera.rect.y * Screen.width, player.myCamera.rect.height);

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


		//playerUIParents[newPlayer.playerID].


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

	private void InitButtonListeners()
	{
		//inventoryButton = GameCanvas.Instance.LookupComponent<Button>("Inventory Button");
		//journalButton = GameCanvas.Instance.LookupComponent<Button>("Journal Button");
		//networkButton = GameCanvas.Instance.LookupComponent<Button>("Network Button");
		//helpButton = GameCanvas.Instance.LookupComponent<Button>("Help Button");
		//chatButton = GameCanvas.Instance.LookupComponent<Button>("Chat Button");
		
		
			//journalShareButton = GameCanvas.Instance.LookupComponent<Button>("Journal Share Button");

			//journalShareButton.onClick.AddListener(() =>
			//{
				//StoryRecordManager.Instance.ShareJournal();
			//}
				// );

		#region UI Buttons
		//inventoryButton.onClick.AddListener(() =>
		//{
		//	ToggleInventory();
		//}
		//	 );

		//journalButton.onClick.AddListener(() =>
		//{
		//	ToggleJournal();
		//}
		//	 );

		
		//networkButton.onClick.AddListener(() =>
		//{
		//	ToggleNetworkMenu();
		//}
		//	 );

		//helpButton.onClick.AddListener(() =>
		//{
		//	ToggleHelp();
		//}
		//	 );

		//chatButton.onClick.AddListener(() =>
		//{
		//	ToggleChat();
		//}
		//	 );
		#endregion

		#region Close Buttons
		//helpCloseButton.onClick.AddListener(() =>
		//{
		//	CloseHelp();
		//}
		//	 );

		//inventoryCloseButton.onClick.AddListener(() =>
		//{
		//	ToggleInventory();
		//}
		//	 );

		//journalCloseButton.onClick.AddListener(() =>
		//{
		//	ToggleJournal();
		//}
		//	 );
		#endregion
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

		//if (InvManager.NumPlayers > 0)
		//{
		//	if (InvManager.NumPlayers != noOfPlayers)
		//	{
		//		noOfPlayers = InvManager.NumPlayers;
		//		inspection.Init();
		//		EndInspect();
		//	}
		//}

		UpdateSubMenuDisplay();

		#region Escape Key
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//if (inspection.gameObject.activeInHierarchy)
			//{
			//	EndInspect();
			//}
			//else
			//{
			//	if (journalState == MenuState.Open)
			//	{
			//		journalState = MenuState.Closed;
			//	}
			//	if (inventoryState == MenuState.Open)
			//	{
			//		inventoryState = MenuState.Closed;
			//	}
			//}
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

		#region If a Conversation is in progress
		if (true)
		{
			HideUIButtons();
			CloseInventory();
			CloseJournal();
		}
		else
		{
			ShowUIButtons();
			#region Inventory Control
			if (Input.GetKeyDown(KeyCode.I) && AllowHotkeys())
			{
				//if (inspection.gameObject.activeSelf)
				//{
				//	EndInspect();
				//}
				//else
				//{
				//	ToggleInventory();
				//}
			}
			#endregion

			#region Journal Control
			if (Input.GetKeyDown(KeyCode.J) && AllowHotkeys())
			{
				ToggleJournal();
			}
			#endregion

			#region Journal Control
			if (Input.GetKeyDown(KeyCode.H) && AllowHotkeys())
			{
				ToggleHelp();
			}
			#endregion


			#region Chat Control
			//if (Input.GetKeyDown(KeyCode.Tab) && uiButtonState == MenuState.Open)
			//{
			//	ToggleChat();
			//}
			#endregion
		}
		#endregion
	}
	#endregion

	#region Player Button UI
	public bool AllowHotkeys()
	{
		return true;
		//bool uiButtonsOpen = uiButtonState == MenuState.Open;
		//return uiButtonsOpen && !chatBox.activeSelf;
	}
	public void HideUIButtons()
	{
		//if (uiButtonState == MenuState.Open)
		//{
		//	uiButtonState = MenuState.Closed;
		//}
	}
	public void ShowUIButtons()
	{
		//if (uiButtonState == MenuState.Closed)
		//{
		//	uiButtonState = MenuState.Open;
		//}
	}
	#endregion

	#region Menu Management
	public void UpdateSubMenuDisplay()
	{
		//if (uiButtonState == MenuState.Open)
		//{
		//	playerButtons.SetActive(true);
		//}
		//else if (uiButtonState == MenuState.Closed)
		//{
		//	playerButtons.SetActive(false);
		//}
		//else
		//{
		//	//this.OutputInfo("Error Menu State for Player Buttons State", ObjectExtensions.OutputType.Error);
		//}


		//if (journalState == MenuState.Suppressed)
		//{
		//	CloseJournal();
		//}
		//else if (journalState == MenuState.Open)
		//{
		//	OpenJournal();
		//}
		//else if (journalState == MenuState.Closed)
		//{
		//	CloseJournal();
		//}
		//else
		//{
		//	//this.OutputInfo("Error Menu State for Journal State", ObjectExtensions.OutputType.Error);
		//}

		//if (inventoryState == MenuState.Suppressed)
		//{
		//	CloseInventory();
		//}
		//else if (inventoryState == MenuState.Open)
		//{
		//	OpenInventory();
		//}
		//else if (inventoryState == MenuState.Closed)
		//{
		//	CloseInventory();
		//}
		//else
		//{
		//	//this.OutputInfo("Error Menu State for Journal State", ObjectExtensions.OutputType.Error);
		//}
	}

	#region Close Menus
	public void CloseInventory()
	{
		//inventoryState = MenuState.Closed;
		//inventory.SetActive(false);
	}
	public void CloseJournal()
	{
		//journalState = MenuState.Closed;
		//journal.SetActive(false);
	}
	public void CloseChat()
	{
		//chatBox.SetActive(false);
	}
	public void CloseHelp()
	{
		//helpState = MenuState.Closed; 
		//helpBox.SetActive(false);
	}
	public void EndInspect()
	{
		//if (inspection.gameObject.activeSelf)
		//{
		//	inspection.gameObject.SetActive(false);
		//	inventory.SetActive(true);
		//	allowInventoryControl = true;
		//	allowJournalControl = true;

		//	if (inventoryState == MenuState.Suppressed)
		//	{
		//		inventoryState = MenuState.Open;
		//	}
		//	if (journalState == MenuState.Open)
		//	{
		//		journalState = MenuState.Suppressed;
		//	}
		//}
	}
	#endregion

	#region Opening Menus
	public void OpenInventory()
	{
		//inventory.SetActive(true);
	}
	public void OpenJournal()
	{
		//journal.SetActive(true);
	}
	public void OpenChat()
	{
		//chatBox.SetActive(true);
	}
	public void OpenHelp()
	{
		//helpState = MenuState.Open;
		//helpBox.SetActive(true);
	}
	private bool OpenInspection()
	{
		//if (inspection.Initialized)
		//{

		//allowInventoryControl = false;
		//allowJournalControl = false;

		return true;
		//}
		//else
		//{
		//	inspection.Init();
		//	return false;
		//}
	}
	#endregion

	#region Toggling Menus
	public void ToggleInventory()
	{
		//if (allowInventoryControl)
		//{
		//	if (UIManager.Instance.inventory.activeSelf)
		//	{
		//		inventoryState = MenuState.Closed;
		//		//UIManager.Instance.inventory.SetActive(false);
		//	}
		//	else
		//	{
		//		CloseHelp();
		//		inventoryState = MenuState.Open;
		//		//UIManager.Instance.inventory.SetActive(true);
		//	}
		//}
	}
	public void ToggleJournal()
	{
		//if (allowJournalControl)
		//{
		//	if (journal.activeSelf)
		//	{
		//		journalState = MenuState.Closed;
		//		//journal.SetActive(false);
		//	}
		//	else
		//	{
		//		CloseHelp();
		//		journalState = MenuState.Open;
		//		//journal.SetActive(true);
		//	}
		//}
	}
	public void ToggleNetworkMenu()
	{

	}
	public void ToggleHelp()
	{
		//if (helpBox.activeSelf)
		//{
		//	CloseHelp();
		//}
		//else
		//{
		//	OpenHelp();
		//	CloseJournal();
		//	CloseInventory();
		//	CloseChat();
		//}
	}
	public void ToggleChat()
	{
		//if (chatBox.activeSelf)
		//{
		//	chatBox.SetActive(false);
		//	//PlayerController.getNetworkPlayer().CanMove = true;
		//	//TODO: Kush setup toast notifications for chat
		//	//PlayerController.getNetworkPlayer().isChatOn = false;
		//}
		//else
		//{
		//	chatBox.SetActive(true);
		//	CloseHelp();
		//	GameCanvas.Instance.LookupComponent<InputField>("Chat Input Field").Select();
		//	//PlayerController.getNetworkPlayer().CanMove = false;
		//	//TODO: Kush setup toast notifications for chat
		//	//PlayerController.getNetworkPlayer().isChatOn = true;
		//}
	}
	#endregion

	public void ResetUI()
	{
		//Debug.Log("Resetting Inventory UI\n");
		//for (int i = 0; i < playerInventories[myPlayerIndex].items.Count; i++)
		//{
		//int xPos = 105 * i;
		//int yPos = 0;

		//If xPos > Value
		//xPos -= width
		//yPos += 105;
		//Debug.Log(playerInventories[myPlayerIndex].items[i].itemName + "\n");
		//playerInventories[myPlayerIndex].items[i].appearance.rTrans.anchoredPosition = new Vector2(xPos, yPos);

		//items[importantItem].appearance.rTrans.rect = new Rect(xPos, yPos, 100, 100);
		//items[importantItem].appearance.rTrans.anchorMin = new Vector2(.00, .50f);
		//items[importantItem].appearance.rTrans.anchorMax = new Vector2(.66f, .8f);

		//items[importantItem].appearance.rTrans.offsetMin = new Vector2(.00f, .50f);
		//items[importantItem].appearance.rTrans.offsetMax = new Vector2(.66f, .8f);
		//}
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