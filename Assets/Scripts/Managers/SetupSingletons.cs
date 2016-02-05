using UnityEngine;
using System.Collections;

public class SetupSingletons : MonoBehaviour
{
	public bool randomArena = false;
	void Awake ()
	{
		GameCanvas.Instance.DoNothing();
		AudioManager.Instance.DoNothing();
		UIManager.Instance.DoNothing();
		GameManager.Instance.SetRandomArena(randomArena);
		
	}

	void Start()
	{
		UIManager.Instance.Init();
		Destroy(gameObject);
	}
}
