using UnityEngine;
using System.Collections;

public class IconLoader : MonoBehaviour 
{
	public static Sprite[] Icons;

	void Awake()
	{
		Icons = Resources.LoadAll<Sprite>("Icons");
	}
}
