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
	#endregion

	TextAsset itemXML;
	#endregion

	#region Ingame UI references
	public GameObject inGameUI;

	public GameObject[] playerUIParents;
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
	}

	private void LookupPrefabs()
	{
		//gameUIPrefab = Resources.Load<GameObject>("In-Game UI - Canvas");
		//buttonPrefab = Resources.Load<GameObject>("GivePlayerButton");
		//itemPrefab = Resources.Load<GameObject>("Item Entry");
		//infoPrefab = Resources.Load<GameObject>("Info Entry");
		//solutionUIPrefab = Resources.Load<GameObject>("Display Solution");
		//reqUIPrefab = Resources.Load<GameObject>("Display Requirement");

		//Icons = Resources.LoadAll<Sprite>("Icons");
	}

	void Start()
	{
		//For each player

		//Create a Player UI

		PlayerAbilityDisplay = new PopulateContainer[1];
		PlayerAbilityDisplay[0] = GameCanvas.Instance.LookupComponent<PopulateContainer>("P1 Ability Parent");

		//LoseState = GameCanvas.Instance.LookupComponent<CanvasGroup>("Lose State");
		//WinState = GameCanvas.Instance.LookupComponent<CanvasGroup>("Win State");
		//LoseState.alpha = 0;
		//WinState.alpha = 0;

		//InitButtonListeners();
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