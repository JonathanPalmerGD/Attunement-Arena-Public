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
	public static GameObject infoPrefab;
	public static GameObject itemPrefab;
	public static GameObject gameUIPrefab;
	public static GameObject solutionUIPrefab;
	public static GameObject reqUIPrefab;
	public static GameObject buttonPrefab;
	public static Sprite[] Icons;
	public static Sprite complete;
	public static Sprite incomplete;
	#endregion

	TextAsset itemXML;
	#endregion

	#region Ingame UI references
	public GameObject inGameUI;
	public GameObject inventory;
	public GameObject journal;
	public GameObject sockDrawer;
	public GameObject chatBox;
	public GameObject helpBox;
	public GameObject menuParent;
	//public ItemUI inspection;

	//public PopulateContainer journalContainer;
	//public PopulateContainer inventoryContainer;
	public GameObject journalEntryContainer;
	public GameObject inventoryItemContainer;

	public Button journalCloseButton;
	public Button inventoryCloseButton;
	public Button helpCloseButton;

	public GameObject interactPrompt;

	public Scrollbar inventoryScrolling;
	public Scrollbar journalScrolling;

	//public PlayerController playerTalking;

	public Image transitionImage;

	public GameObject playerButtons;
	public Button inventoryButton;
	public Button journalButton;
	public Button journalShareButton;
	public Button networkButton;
	public Button helpButton;
	public Button chatButton;

	public CanvasGroup WinState;
	public CanvasGroup LoseState;

	private int noOfPlayers = -1;
	#endregion

	#region Menu States
	public enum MenuState { Closed, Open, Suppressed, Error }
	public MenuState journalState;
	public MenuState inventoryState;
	public MenuState sockState;
	public MenuState interactState;
	public MenuState uiButtonState;
	public MenuState helpState;

	//TODO: Turn the inventory states into a state machine
	public bool allowInventoryControl = true;
	public bool allowJournalControl = true;
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

		//inspection = inGameUI.transform.FindChild("Inspect Item").gameObject.GetComponent<ItemUI>();
		menuParent = inGameUI.transform.FindChild("Menu").gameObject;

		inventory = menuParent.transform.FindChild("Inventory").gameObject;
		//inventoryContainer = inventory.GetComponent<PopulateContainer>();
		inventoryItemContainer = FindChildRecursive(inventory.transform, "Entry Scrollable").gameObject;
		inventoryCloseButton = inventory.transform.FindChild("Title").FindChild("Close Button").GetComponent<Button>();

		journal = menuParent.transform.FindChild("Journal").gameObject;
		//journalContainer = journal.GetComponent<PopulateContainer>();
		journalEntryContainer = FindChildRecursive(journal.transform, "Entry Scrollable").gameObject;
		journalCloseButton = journal.transform.FindChild("Title").FindChild("Close Button").GetComponent<Button>();

		inventoryScrolling = FindChildRecursive(inventory.transform, "Inventory Scrollbar").GetComponent<Scrollbar>();
		journalScrolling = FindChildRecursive(journal.transform, "Journal Scrollbar").GetComponent<Scrollbar>();

		interactPrompt = inGameUI.transform.FindChild("Interaction Panel").gameObject;

	} // end Awake

	private void LookupPrefabs()
	{
		gameUIPrefab = Resources.Load<GameObject>("In-Game UI - Canvas");
		buttonPrefab = Resources.Load<GameObject>("GivePlayerButton");
		itemPrefab = Resources.Load<GameObject>("Item Entry");
		infoPrefab = Resources.Load<GameObject>("Info Entry");
		solutionUIPrefab = Resources.Load<GameObject>("Display Solution");
		reqUIPrefab = Resources.Load<GameObject>("Display Requirement");

		Icons = Resources.LoadAll<Sprite>("Icons");
		complete = Icons[2];
		incomplete = Icons[10];
	}

	private Transform FindChildRecursive(Transform current, string targetName, int counter = 0)
	{
		//Transform[] allChildren = current.GetComponentsInChildren<Transform>();
		//Debug.Log("Find Child Recursive " + current.name + "  " + allChildren.Length + " for " + targetName + "\n");

		if (counter > 15)
			return null;
		foreach (Transform child in current)
		{
			//Debug.Log("Comparing " + child.name + " to " + targetName + "\n");
			if (child.name == targetName)
			{
				return child;
			}
			else if (child.childCount > 0)
			{
				counter++;
				Transform t = FindChildRecursive(child, targetName, counter);

				if (t != null || counter > 15)
				{
					return t;
				}
			}
		}

		//Does not exists
		return null;
	}

	void Start()
	{
		//if (inspection.gameObject.activeSelf)
		//{
		//	inspection.Init();
		//	EndInspect();
		//}

		//sockDrawer = GameCanvas.Instance.LookupGameObject("Sock Drawer");
		transitionImage = GameCanvas.Instance.LookupComponent<Image>("Transition");

		playerButtons = GameCanvas.Instance.LookupGameObject("Player Buttons");
		chatBox = GameCanvas.Instance.LookupGameObject("Chat");		
		helpBox = GameCanvas.Instance.LookupGameObject("Help Screen");

		helpCloseButton = GameCanvas.Instance.LookupComponent<Button>("Help Close Button");


		LoseState = GameCanvas.Instance.LookupComponent<CanvasGroup>("Lose State");
		WinState = GameCanvas.Instance.LookupComponent<CanvasGroup>("Win State");
		LoseState.alpha = 0;
		WinState.alpha = 0;

		InitButtonListeners();

		//Close any left open UI stuff.
		inventoryState = MenuState.Closed;
		journalState = MenuState.Closed;
		sockState = MenuState.Closed;
		interactState = MenuState.Closed;
		uiButtonState = MenuState.Open;
		helpState = MenuState.Open;
		chatBox.SetActive(false);

		//CloseSocks();
		//CloseInteract();

#if UNITY_EDITOR
		CloseHelp();
#endif
		//CloseInventory();
		//CloseJournal();


	}// end Start

	private void InitButtonListeners()
	{
		inventoryButton = GameCanvas.Instance.LookupComponent<Button>("Inventory Button");
		journalButton = GameCanvas.Instance.LookupComponent<Button>("Journal Button");
		networkButton = GameCanvas.Instance.LookupComponent<Button>("Network Button");
		helpButton = GameCanvas.Instance.LookupComponent<Button>("Help Button");
		chatButton = GameCanvas.Instance.LookupComponent<Button>("Chat Button");
		
		if (PlayerPrefs.GetString("GameType") == "Multiplayer")
		{
			journalShareButton = GameCanvas.Instance.LookupComponent<Button>("Journal Share Button");

			journalShareButton.onClick.AddListener(() =>
			{
				//StoryRecordManager.Instance.ShareJournal();
			}
				 );
		}
		else
		{
			GameObject.Destroy(GameCanvas.Instance.LookupGameObject("Bottom Region"));
		}

		#region UI Buttons
		inventoryButton.onClick.AddListener(() =>
		{
			ToggleInventory();
		}
			 );

		journalButton.onClick.AddListener(() =>
		{
			ToggleJournal();
		}
			 );

		
		networkButton.onClick.AddListener(() =>
		{
			ToggleNetworkMenu();
		}
			 );

		helpButton.onClick.AddListener(() =>
		{
			ToggleHelp();
		}
			 );

		chatButton.onClick.AddListener(() =>
		{
			ToggleChat();
		}
			 );
		#endregion

		#region Close Buttons
		helpCloseButton.onClick.AddListener(() =>
		{
			CloseHelp();
		}
			 );

		inventoryCloseButton.onClick.AddListener(() =>
		{
			ToggleInventory();
		}
			 );

		journalCloseButton.onClick.AddListener(() =>
		{
			ToggleJournal();
		}
			 );
		#endregion
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
			if (Input.GetKeyDown(KeyCode.Tab) && uiButtonState == MenuState.Open)
			{
				ToggleChat();
			}
			#endregion
		}
		#endregion
	}
	#endregion

	#region Create Information
	public void CreateInformationItem()
	{

	}
	#endregion

	#region Player Button UI
	public bool AllowHotkeys()
	{
		bool uiButtonsOpen = uiButtonState == MenuState.Open;
		return uiButtonsOpen && !chatBox.activeSelf;
	}
	public void HideUIButtons()
	{
		if (uiButtonState == MenuState.Open)
		{
			uiButtonState = MenuState.Closed;
		}
	}
	public void ShowUIButtons()
	{
		if (uiButtonState == MenuState.Closed)
		{
			uiButtonState = MenuState.Open;
		}
	}
	#endregion

	#region Inventory UI Management
	//public GameObject AddInventoryGameObject()
	//{
	//	return inventoryContainer.AddPrefabToContainerReturn();
	//}
	//public ItemUI AddInventoryUI()
	//{
	//	ItemUI newItemUI = inventoryContainer.AddPrefabToContainerReturn().GetComponent<ItemUI>();

	//	newItemUI.button.onClick.AddListener(() =>
	//	{
			
	//		InspectItem(newItemUI.myItem);
	//	}
	//	);

	//	return newItemUI;
	//}

	//This probably wants to return a bool depending on success
	//public void RemoveItemUI(Item toRemove)
	//{
	//	inventoryContainer.RemoveFromContainer(toRemove.appearance.transform);
	//}

	////This probably wants to return a bool depending on success
	//public void RemoveAllInventoryEntries()
	//{
	//	inventoryContainer.Clear();
	//}
	#endregion

	#region Menu Management
	public void UpdateSubMenuDisplay()
	{
		if (uiButtonState == MenuState.Open)
		{
			playerButtons.SetActive(true);
		}
		else if (uiButtonState == MenuState.Closed)
		{
			playerButtons.SetActive(false);
		}
		else
		{
			//this.OutputInfo("Error Menu State for Player Buttons State", ObjectExtensions.OutputType.Error);
		}


		if (journalState == MenuState.Suppressed)
		{
			CloseJournal();
		}
		else if (journalState == MenuState.Open)
		{
			OpenJournal();
		}
		else if (journalState == MenuState.Closed)
		{
			CloseJournal();
		}
		else
		{
			//this.OutputInfo("Error Menu State for Journal State", ObjectExtensions.OutputType.Error);
		}

		if (inventoryState == MenuState.Suppressed)
		{
			CloseInventory();
		}
		else if (inventoryState == MenuState.Open)
		{
			OpenInventory();
		}
		else if (inventoryState == MenuState.Closed)
		{
			CloseInventory();
		}
		else
		{
			//this.OutputInfo("Error Menu State for Journal State", ObjectExtensions.OutputType.Error);
		}
	}

	/// <summary>
	/// Handles the inventory ui switching (and state machine)
	/// </summary>
	/// <returns>Returns true if successful</returns>
	//public bool InspectItem(Item itemToInspect = null)
	//{
	//	if (inventoryState == MenuState.Open)
	//	{
	//		inventoryState = MenuState.Suppressed;
	//	}
	//	if (journalState == MenuState.Open)
	//	{
	//		journalState = MenuState.Suppressed;
	//	}

	//	if (OpenInspection())
	//	{
	//		SetupInspectDisplay(itemToInspect);

	//		return true;
	//	}
	//	else
	//	{
	//		inspection.Init();
	//	}
	//	return false;
	//}

	//private bool SetupInspectDisplay(Item itemToInspect)
	//{
	//	inspection.myItem = itemToInspect;

	//	inspection.visualName.text = itemToInspect.itemName;
	//	inspection.description.text = itemToInspect.description;

	//	inspection.img.sprite = UIManager.Icons[inspection.myItem.iconID];
	//	inspection.img.sprite = Resources.Load<Sprite>(inspection.myItem.itemName) as Sprite;

	//	//for (int i = 0; i < itemToInspect.ClueSolutions.Count; i++)
	//	//{
	//		//Setup Clue Solutions

	//	//	inspection.SetupSolutions(itemToInspect);
	//	//}

	//	for (int i = 0; i < InvManager.NumPlayers; i++)
	//	{
	//		if (InvManager.Instance.MyPlayerIndex != i && inspection.giftButtons != null)
	//		{
	//			Debug.Log(inspection.giftButtons.Length + "\n");
	//			if (inspection.giftButtons[i] != null)
	//			{
	//				Debug.Log("Setting Gift Item Button: " + i + " " + inspection.myItem.uniqueID + "\n");
	//				int targetPlayerID = i;
	//				inspection.giftButtons[i].onClick.AddListener(() =>
	//				{
	//					if (PlayerPrefs.GetString("GameType") == "Multiplayer")
	//					{
	//						PlayerController myCurrentPlayer = PlayerController.getNetworkPlayer();
	//						//Sets Item to inactive & transfers item
	//						myCurrentPlayer.giftItemsAcrossnetwork(inspection.myItem.ownerIndex, targetPlayerID, inspection.myItem.uniqueID);
	//					}
	//					else
	//					{
	//						InvManager.Instance.ResetItemToInactive(inspection.myItem);
	//						InvManager.Instance.TransferItem(targetPlayerID, inspection.myItem.uniqueID);
	//					}
	//					UIManager.Instance.EndInspect();

	//				});
	//			}
	//		}
	//	}

	//	//TODO: Setup solution information display

	//	return true;
	//}

	#region Close Menus
	public void CloseInventory()
	{
		inventoryState = MenuState.Closed;
		inventory.SetActive(false);
	}
	public void CloseJournal()
	{
		journalState = MenuState.Closed;
		journal.SetActive(false);
	}
	public void CloseChat()
	{
		chatBox.SetActive(false);
	}
	public void CloseHelp()
	{
		helpState = MenuState.Closed; 
		helpBox.SetActive(false);
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
		inventory.SetActive(true);
	}
	public void OpenJournal()
	{
		journal.SetActive(true);
	}
	public void OpenChat()
	{
		chatBox.SetActive(true);
	}
	public void OpenHelp()
	{
		helpState = MenuState.Open;
		helpBox.SetActive(true);
	}
	private bool OpenInspection()
	{
		//if (inspection.Initialized)
		//{

		allowInventoryControl = false;
		allowJournalControl = false;

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
		if (allowInventoryControl)
		{
			if (UIManager.Instance.inventory.activeSelf)
			{
				inventoryState = MenuState.Closed;
				//UIManager.Instance.inventory.SetActive(false);
			}
			else
			{
				CloseHelp();
				inventoryState = MenuState.Open;
				//UIManager.Instance.inventory.SetActive(true);
			}
		}
	}
	public void ToggleJournal()
	{
		if (allowJournalControl)
		{
			if (journal.activeSelf)
			{
				journalState = MenuState.Closed;
				//journal.SetActive(false);
			}
			else
			{
				CloseHelp();
				journalState = MenuState.Open;
				//journal.SetActive(true);
			}
		}
	}
	public void ToggleNetworkMenu()
	{

	}
	public void ToggleHelp()
	{
		if (helpBox.activeSelf)
		{
			CloseHelp();
		}
		else
		{
			OpenHelp();
			CloseJournal();
			CloseInventory();
			CloseChat();
		}
	}
	public void ToggleChat()
	{
		if (chatBox.activeSelf)
		{
			chatBox.SetActive(false);
			//PlayerController.getNetworkPlayer().CanMove = true;
			//TODO: Kush setup toast notifications for chat
			//PlayerController.getNetworkPlayer().isChatOn = false;
		}
		else
		{
			chatBox.SetActive(true);
			CloseHelp();
			GameCanvas.Instance.LookupComponent<InputField>("Chat Input Field").Select();
			//PlayerController.getNetworkPlayer().CanMove = false;
			//TODO: Kush setup toast notifications for chat
			//PlayerController.getNetworkPlayer().isChatOn = true;
		}
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