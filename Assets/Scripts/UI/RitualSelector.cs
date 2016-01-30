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
	public Ritual SelectedRituals;

	private RectTransform contentRect;

	public string HorizAxis = "Horizontal";
	private bool prevLeft = false, prevRight = false;
	public string ToggleButton = "Jump";
	public string DoneButton = "Start";

	void Start()
	{
		contentRect = GetComponent<ScrollRect>().content;
		contentRect.offsetMin = new Vector2(-102f, 0f);
		CurrRitual = 0;
		TotalRitualCount = contentRect.childCount;
	}

	void Update()
	{
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
			re.Selected = !re.Selected;
			SelectedRituals ^= re.connectedRitual;
		}
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
