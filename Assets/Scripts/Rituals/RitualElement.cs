using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RitualElement : MonoBehaviour
{
	public RitualID connectedRitual = RitualID.Cyclone;

	private Image bg;
	public bool Selected = false; private bool PrevSelected = false;

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
