using UnityEngine;
using System.Collections;

public class SetupSingletons : MonoBehaviour
{
	void Awake ()
	{
		GameCanvas.Instance.DoNothing();
		UIManager.Instance.DoNothing();
		GameManager.Instance.DoNothing();
		UIManager.Instance.Init();
		
		Destroy(gameObject);
	}
}
