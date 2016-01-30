using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	//public List<Ability> rituals;

	private float health;
	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = value;
		}
	}
	private float mana;
	public float Mana
	{
		get
		{
			return mana;
		}
		set
		{
			mana = value;
		}
	}

	public List<Ability> abilities;
	public Dictionary<string, Ability> abilityBindings;

	public Ability AddAbility(string abilityName, string keyBinding)
	{
		Ability newAbility = ScriptableObject.CreateInstance(abilityName) as Ability;

		newAbility.Init(this, keyBinding);

		abilities.Add(newAbility);
		abilityBindings.Add(keyBinding, newAbility);

		return newAbility;
	}

	void Start()
	{
		abilities = new List<Ability>();
		abilityBindings = new Dictionary<string, Ability>();
	}

	public void UseAbility()
	{

	}

	void Update()
	{
		GetInput();
	}

	public void GetInput()
	{

	}
}
