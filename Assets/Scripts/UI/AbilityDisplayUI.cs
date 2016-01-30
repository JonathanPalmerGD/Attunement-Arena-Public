﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AbilityDisplayUI : MonoBehaviour 
{
	public Image Icon;
	public Image CooldownDisplay;
	public Text CostDisplay;
	public Text HotkeyDisplay;
	public Text ChargeDisplay;

	public void SetupDisplay(Ability ability)
	{
		Icon.sprite = UIManager.Icons[ability.IconID];
		SetCooldownPercentage(0);
		CostDisplay.text = "" + (int)ability.Cost;
		HotkeyDisplay.text = ability.keyBindingUserDisplay;

		Icon.sprite = UIManager.Icons[ability.IconID];
		Icon.sprite = UIManager.Icons[ability.IconID];
	}

	public void SetCooldownPercentage(float outOfOnePercentage)
	{
		CooldownDisplay.fillAmount = outOfOnePercentage;
	}
}