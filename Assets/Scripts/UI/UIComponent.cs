using UnityEngine;
using System.Collections;

public class UIComponent : MonoBehaviour 
{
	public enum RelevantType { Text, Image, Button, Scrollbar, Multiple, GameObject, Other, Unassigned}
	public RelevantType componentType = RelevantType.Unassigned;
	public string alternateLookupKey;
	public bool important = true;

	void Start()
	{
		if (important)
		{
			//Debug.Log("Registering " + name + " as " + alternateLookupKey + "\n");
			GameCanvas.Instance.RegisterComponent(alternateLookupKey, this);
		}
		else
		{
			//Debug.Log("Not Registering " + name + " as " + alternateLookupKey + "\nNot important enough");
			//TODO: Make this a staggered register.
			GameCanvas.Instance.RegisterComponent(alternateLookupKey, this);
		}
	}
}
