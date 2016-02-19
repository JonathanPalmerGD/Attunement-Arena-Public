using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RitualElement : MonoBehaviour
{
	public RitualID connectedRitual = RitualID.Cyclone;

	private Image bg;
	public bool Selected = false; private bool PrevSelected = false;

	void Start()
	{
		Text label = transform.FindChild("Label").GetComponent<Text>();
		Image icon = transform.FindChild("Icon").GetComponent<Image>();
		Text effect = transform.FindChild("Effect").GetComponent<Text>();

		Ritual myRit = Ritual.GetRitualForID(connectedRitual);
		Debug.Log(connectedRitual.ToString());
		label.text = myRit.DisplayName;
		icon.sprite = IconLoader.Icons[myRit.DisplayIconID];
		effect.text = myRit.Description;
	}

	void Update()
	{
		if (!bg) bg = GetComponent<Image>();
		if(Selected ^ PrevSelected)
		{
			PrevSelected = Selected;

			bg.color = Selected ? new Color(1f, 0.5f, 0f) : Color.white;
		}
	}
}
