using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCanvas : MonoBehaviour 
{
	private static GameCanvas _instance;
	public static GameCanvas Instance
	{
		get 
		{
			if (_instance == null)
			{
				_instance = GameObject.Find("In-Game UI - Canvas").GetComponent<GameCanvas>();
			}

			if (!_instance.initialized)
			{
				_instance.Init();
			}

			//Debug.Log("Hit\n" + (_instance == null).ToString());
			return _instance; 
		}
	}
	public Dictionary<string, UIComponent> compDict;
	
	public bool displayInspectorInfo = true;

	[Header("Dictionary Information")]
	public int registeredCount = 0;
	public List<string> componentKeys;
	public List<UIComponent> componentValues;

	public bool initialized = false;

	void Awake()
	{
		if (!initialized)
		{
			Init();
		}
	}

	void Init()
	{
		initialized = true;
		compDict = new Dictionary<string, UIComponent>();
		if (displayInspectorInfo)
		{
			componentKeys = new List<string>();
			componentValues = new List<UIComponent>();
		}
	}

	private Transform FindChildRecursive(Transform current, string targetName, int counter = 0)
	{
		//Transform[] allChildren = current.GetComponentsInChildren<Transform>();
		//Debug.Log("Find Child Recursive " + current.name + "  " + allChildren.Length + " for " + targetName + "\n");

		if (counter > 25)
			return null;
		foreach (Transform child in current)
		{
			//Debug.Log("Comparing " + child.name + " to " + targetName + "\n");
			if (child.name == targetName)
			{
				return child;
			}
			else if (child.childCount > 0)
			{
				counter++;
				Transform t = FindChildRecursive(child, targetName, counter);

				if (t != null || counter > 15)
				{
					return t;
				}
			}
		}

		//Does not exists
		return null;
	}

	public void RegisterComponent(string LookupKey, UIComponent comp)
	{
		if(LookupKey.Length < 1)
		{
			//Debug.Log("Defaulting Lookup Key as name\n");
			LookupKey = comp.name;
		}

		if (compDict.ContainsKey(LookupKey))
		{
			Debug.LogError("Lookup Key [" + LookupKey + "] already exists\n");
		}
		else
		{
			//Debug.Log("Adding Key " + LookupKey + "\n");
			compDict.Add(LookupKey, comp);
			if (displayInspectorInfo)
			{
				componentKeys.Add(LookupKey);
				componentValues.Add(comp);
			}
		}

		registeredCount = compDict.Count;
	}

	public T LookupComponent<T>(string compName) where T : Component
	{
		try
		{
			return compDict[compName].GetComponent<T>();
		}
		catch(System.Exception e)
		{
			Debug.LogError("[GameCanvas].Lookup Component error with " + compName + "\t\t" + typeof(T).ToString() + "\n" + e.Message);
			return compDict[compName].GetComponent<T>();
		}
	}

	public GameObject LookupGameObject(string compName)
	{
		return compDict[compName].gameObject;
	}

	public void DoNothing()
	{

	}
}
