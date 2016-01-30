﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Flags]
public enum Ritual
{
	GenericRitual01 = 1, // Minor HP / MP boost
	AnIceGuy = 2
}

public class RitualElement : MonoBehaviour
{
	public Ritual connectedRitual = Ritual.AnIceGuy;

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