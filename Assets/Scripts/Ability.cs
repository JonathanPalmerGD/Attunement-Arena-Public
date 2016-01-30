using UnityEngine;
using System.Collections;

public class Ability : ScriptableObject 
{
	public Player owner;

	public bool initialized = false;
	public void Init(Player newOwner)
	{
		if (!initialized)
		{
			//Add UI to the controlling player
			owner = newOwner;



			initialized = true;
		}
	}
	
	void Update () 
	{
	
	}
}
