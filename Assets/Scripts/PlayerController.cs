using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	public string playerName = "Player 1";
	public PopulateContainer popCon;

	void Start () 
	{
	
	}
	
	void Update () 
	{
		if (Input.GetButtonDown(playerName + " Jump"))
		{
			Debug.Log(name + "\nJumped\n");
		}

		if (Input.GetKeyDown(KeyCode.N))
		{
			popCon.AddPrefabToContainer();
		}
	}
}
