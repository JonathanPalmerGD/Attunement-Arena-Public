using UnityEngine;
using System.Collections;

public class CheckReady : MonoBehaviour
{
	public bool ready = false;
	private UnityEngine.UI.Text t;
	public byte pNum = 0;
	public Player.PlayerControls ctrlType = Player.PlayerControls.Mouse;
	public Player.PlayerControls CtrlType
	{
		set
		{
			if (ctrlType != value)
			{
				ctrlType = value;
				ready = false;
				if (t)
					t.text = "Awaiting Player " + (pNum + 1) + " Ready!\n" + (ctrlType == Player.PlayerControls.GamePad ? "Controller " + (pNum + 1) : "Mouse & Keyboard");
			}
		}
	}
	public string ReadyButton = "Jump";
	public string UnreadyButton = "Primary";

	public GameObject[] DisableOnNext;
	public GameObject[] EnableOnNext;

	public bool IsReady
	{
		get { return ready; }
		set
		{
			if (ready ^ value)
			{
				ready = value;

				t.text = ready ? "All Good!"
							   : "Awaiting Player " + (pNum + 1) + " Ready!\n" + (ctrlType == Player.PlayerControls.GamePad ? "Controller " + (pNum + 1) : "Mouse & Keyboard");
			}
		}
	}

	void Start()
	{
		if (!t) t = GetComponentInChildren<UnityEngine.UI.Text>();
		t.text = "Awaiting Player " + (pNum + 1) + " Ready!\n" + (ctrlType == Player.PlayerControls.GamePad ? "Controller " + (pNum + 1) : "Mouse & Keyboard");
	}

	void Update()
	{
		if (!t)
		{
			t = GetComponentInChildren<UnityEngine.UI.Text>();
			t.text = "Awaiting Player " + (pNum + 1) + " Ready!\n" + (ctrlType == Player.PlayerControls.GamePad ? "Controller " + (pNum + 1) : "Mouse & Keyboard");
		}
		if (!IsReady && Input.GetButtonDown((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + ReadyButton))
		{
			var players = FindObjectsOfType<CheckReady>();
			Debug.Log("Player Ready!\nWe have " + players.Length + " players this go around.");

			IsReady = true;

			bool allReady = true;
			foreach (CheckReady cr in players)
			{
				allReady &= cr.IsReady;
			}

			if (allReady)
			{
				PlayerPrefs.SetInt("PlayerCount", players.Length);

				// Move to Ritual Select
				foreach (GameObject go in DisableOnNext)
					go.SetActive(false);
				foreach (GameObject go in EnableOnNext)
					go.SetActive(true);
			}
		}
		else if (IsReady && Input.GetButtonDown((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + UnreadyButton))
		{
			IsReady = false;
		}
	}
}