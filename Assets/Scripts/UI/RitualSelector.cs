using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RitualSelector : MonoBehaviour
{
	public byte pNum = 0;
	public Player.PlayerControls ctrlType = Player.PlayerControls.GamePad;

	private int TotalRitualCount;
	private int CurrRitual;
	public byte SelectedRitualCount;
	[SerializeField, EnumFlagsField]
	public RitualID SelectedRituals;

	private RectTransform contentRect;

	public string HorizAxis = "Horizontal";
	private bool prevLeft = false, prevRight = false;
	public string ToggleButton = "Jump";
	public string DoneButton = "Start";
	public GameObject[] DisableOnDone;
	public GameObject[] EnableOnDone;

	[System.NonSerialized]
	public bool Done = false;

	private string Hint
	{
		get
		{
			if (ctrlType == Player.PlayerControls.GamePad)
			{
				return "<color=#FFAA00FF>[LStick Left], [LStick Right]</color> Choose || <color=green>[A]</color> Select" + (SelectedRitualCount > 0 ? " || <color=blue>[Start]</color> Finish" : "");
			}
			else
			{
				return "<color=#FFAA00FF>[Left Arrow], [Right Arrow]</color> Choose || <color=green>[Space]</color> Select" + (SelectedRitualCount > 0 ? " || <color=blue>[Enter]</color> Finish" : "");
			}
		}
	}

	public Coroutine PnPCR;
	public Text TooManyText;
	public Text InputHints;

	void Start()
	{
		contentRect = GetComponent<ScrollRect>().content;
		contentRect.offsetMin = new Vector2(-102f, 0f);
		CurrRitual = 0;
		TotalRitualCount = contentRect.childCount;

		InputHints.text = Hint;
	}

	void Update()
	{
		if (Done) return;

		if (GetLeft())
		{
			CurrRitual = Mathf.Max(0, CurrRitual - 1);
		}
		else if (GetRight())
		{
			CurrRitual = Mathf.Min(TotalRitualCount - 1, CurrRitual + 1);
		}

		contentRect.offsetMin = new Vector2(Mathf.Lerp(contentRect.offsetMin.x, (-102f - (200f * CurrRitual)), Time.deltaTime * 10f), 0f);

		if (GetSelect())
		{
			RitualElement re = contentRect.GetChild(CurrRitual).GetComponent<RitualElement>();
			if (SelectedRitualCount < 3 || re.Selected)
			{
				re.Selected = !re.Selected;
				SelectedRituals ^= re.connectedRitual;

				if (re.Selected) SelectedRitualCount++; else SelectedRitualCount--;

				InputHints.text = Hint;

				// User has not done a dumb and picked more than three Rituals
				// Clear error away
				if (PnPCR != null) StopCoroutine(PnPCR);
				Color c = TooManyText.color;
				c.a = 0f;
				TooManyText.color = c;
			}
			else
			{
				// User has done a dumb and picked more than three Rituals
				// Show error
				if (PnPCR != null) StopCoroutine(PnPCR);
				PnPCR = StartCoroutine(PopAndFade(TooManyText));
			}
		}

		if (SelectedRitualCount > 0 && GetDone())
		{
			PlayerPrefs.SetString("P" + pNum + "Rits", ((long)SelectedRituals).ToString());
			foreach (GameObject go in DisableOnDone)
				go.SetActive(false);
			foreach (GameObject go in EnableOnDone)
				go.SetActive(true);

			Done = true;

			bool AllDone = true;
			foreach (RitualSelector rs in FindObjectsOfType<RitualSelector>())
			{
				AllDone &= rs.Done;
			}

			if (AllDone)
			{
				// Do Scene Change
				Application.LoadLevel(Application.loadedLevel + 1);
			}
		}
	}

	public IEnumerator PopAndFade(Text element)
	{
		float totalTime = 0f;
		Color c = element.color; // Get color reference
		c.a = 1f;
		element.color = c; // Max alpha color
		while (totalTime < 2f) // Keep for two seconds
		{
			totalTime += Time.deltaTime;
			yield return null;
		}
		totalTime = 0f; // Reset timer
		while (totalTime < 2f) // Fade over two seconds
		{
			totalTime += Time.deltaTime;
			c.a = 1f - (totalTime / 2f); // Alpha should fade from full to zero over two seconds
			element.color = c; // Set new alpha
			yield return null;
		}
		c.a = 0f;
		element.color = c;
	}

	public bool GetLeft()
	{
		bool left = Input.GetAxisRaw((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + HorizAxis) < -0.05, rtn;
		if (left && !prevLeft)
		{
			rtn = true;
		}
		else
		{
			rtn = false;
		}
		prevLeft = left;
		return rtn;
	}

	public bool GetRight()
	{
		bool right = Input.GetAxisRaw((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + HorizAxis) > 0.05, rtn;
		if (right && !prevRight)
		{
			rtn = true;
		}
		else
		{
			rtn = false;
		}
		prevRight = right;
		return rtn;
	}

	public bool GetSelect()
	{
		return Input.GetButtonDown((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + ToggleButton);
	}

	public bool GetDone()
	{
		return Input.GetButtonDown((ctrlType == Player.PlayerControls.GamePad ? ("P" + (pNum) + " ") : "") + DoneButton);
	}
}
