using UnityEngine;
using System.Collections;

public class UIComponent : MonoBehaviour
{
	public enum RelevantType { Text, Image, Button, Scrollbar, Multiple, GameObject, Other, Unassigned }
	public RelevantType componentType = RelevantType.Unassigned;
	public string alternateLookupKey;
	public bool registered = false;
	public bool important = true;

	public string Name
	{
		get { return name; }
		set
		{
			if (registered)
			{
				GameCanvas.Instance.AlterRegistration(name, value, this);
			}
			name = value;
			
		}
	}

	void Awake()
	{
		if (important)
		{
			//Debug.Log("Registering " + name + " as " + alternateLookupKey + "\n");
			GameCanvas.Instance.RegisterComponent(alternateLookupKey, this);
			registered = true;
		}
	}

	void Start()
	{
		if (!important)
		{
			GameCanvas.Instance.RegisterComponent(alternateLookupKey, this);
			registered = true;
		}
	}
}
