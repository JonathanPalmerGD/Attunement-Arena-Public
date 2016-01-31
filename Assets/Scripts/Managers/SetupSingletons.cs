using UnityEngine;
using System.Collections;

public class SetupSingletons : MonoBehaviour
{
	void Awake ()
	{
		GameCanvas.Instance.DoNothing();
		AudioManager.Instance.DoNothing();
		UIManager.Instance.DoNothing();
		GameManager.Instance.DoNothing();
		
	}

	void Start()
	{
		UIManager.Instance.Init();
		Destroy(gameObject);
	}
}
