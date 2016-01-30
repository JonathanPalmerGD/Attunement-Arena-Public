using UnityEngine;
using System.Collections;

public class CheckReady : MonoBehaviour
{
	public bool ready = false;
	private UnityEngine.UI.Text t;
	public byte pNum = 0;
	public Player.PlayerControls ctrlType = Player.PlayerControls.Mouse;
	public string ReadyButton = "A";
	public string FriendlyReadyButton = "Jump";
	public string UnreadyButton = "B";
	public string FriendlyUnreadyButton = "Primary";
	public bool IsReady
	{
		get { return ready; }
		set
		{
			if (ready ^ value)
			{
				ready = value;

				t.text = ready ? "All Good!\nPress <color=red>[" + FriendlyUnreadyButton + "]</color> to back out"
							   : "Awaiting Player " + (pNum + 1) + " Ready!\nPress <color=green>[" + FriendlyReadyButton + "]</color> to be Ready";
			}
		}
	}

	void Start()
	{
		if (!t) t = GetComponentInChildren<UnityEngine.UI.Text>();
		t.text = "Awaiting Player " + (pNum + 1) + " Ready!\nPress <color=green>[" + FriendlyReadyButton + "]</color> to be Ready";
	}

	void Update()
	{
		if (!t) t = GetComponentInChildren<UnityEngine.UI.Text>();
		if (!IsReady && Input.GetButtonDown((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + ReadyButton))
		{
			IsReady = true;

			bool allReady = true;
			foreach (CheckReady cr in FindObjectsOfType<CheckReady>())
			{
				allReady &= cr.IsReady;
			}

			if (allReady)
			{
				// Move to Ritual Select
			}
		}
		else if (IsReady && Input.GetButtonDown((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + UnreadyButton))
		{
			IsReady = false;
		}
	}
}
