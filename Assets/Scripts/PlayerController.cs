using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	public string playerName;
	void Start () 
	{
	
	}
	
	void Update () 
	{
		if (Input.GetButtonDown(playerName + " Jump"))
		{
			Debug.Log(name + "\nJumped\n");
		}
	}
}
