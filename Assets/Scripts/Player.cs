using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
	public int playerID = 0;

	public RigidbodyFirstPersonController controller;
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

	public Ability AddAbility(string abilityName, string keyBinding, string displayKeyBinding)
	{
		Ability newAbility = ScriptableObject.CreateInstance(abilityName) as Ability;

		newAbility.Init(this, keyBinding, displayKeyBinding);

		abilities.Add(newAbility);
		abilityBindings.Add(keyBinding, newAbility);

		return newAbility;
	}

	void Start()
	{
		abilities = new List<Ability>();
		abilityBindings = new Dictionary<string, Ability>();

		controller = GetComponent<RigidbodyFirstPersonController>();
	}

	public void UseAbility()
	{

	}

	void Update()
	{
		GetInput();

		for (int i = 0; i < abilities.Count; i++)
		{
			
		}
	}

	public void GetInput()
	{

	}
}
