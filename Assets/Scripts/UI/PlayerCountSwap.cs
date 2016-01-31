using UnityEngine;
using System.Collections;

public class PlayerCountSwap : MonoBehaviour
{
	public bool CanSwap = false;
	public GameObject[] TwoP, ThreeP, FourP;
	public CheckReady TwoLast, ThreeLast, FourLast;
	public RitualSelector TwoRSLast, ThreeRSLast, FourRSLast;
	public PlayerCountSwap otherPCS;
	private bool useKbd = false;
	private byte currPCount = 2;
	private byte CurrCount
	{
		set
		{
			if (CanSwap)
			{
				otherPCS.CurrCount = value;
				foreach (CheckReady cr in FindObjectsOfType<CheckReady>())
				{
					cr.IsReady = false;
				}
			}
			currPCount = value;
			foreach (GameObject go in TwoP)
			{
				go.SetActive(value == 2);
			}
			foreach (GameObject go in ThreeP)
			{
				go.SetActive(value == 3);
			}
			foreach (GameObject go in FourP)
			{
				go.SetActive(value == 4);
			}
		}
	}

	void Update()
	{
		if (!CanSwap) return;
		if (currPCount != 2 && (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)))
		{
			CurrCount = 2;
		}
		if (currPCount != 3 && (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)))
		{
			CurrCount = 3;
		}
		if (currPCount != 4 && (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)))
		{
			CurrCount = 4;
		}

		//if (Input.GetKeyDown(KeyCode.K))
		//{
		//	useKbd = !useKbd;
		//	TwoLast.CtrlType = ThreeLast.CtrlType = FourLast.CtrlType = TwoRSLast.ctrlType = ThreeRSLast.ctrlType = FourRSLast.ctrlType = (useKbd ? Player.PlayerControls.Mouse : Player.PlayerControls.GamePad);
		//}
	}
}
