using UnityEngine;
using System.Collections;

public class SetupSingletons : MonoBehaviour
{
	public bool randomArena = false;
	void Awake ()
	{
		GameCanvas.Instance.DoNothing();
		AudioManager.Instance.DoNothing();
		GameManager.spawnRandomArena = randomArena;
		UIManager.Instance.DoNothing();
		GameManager.Instance.DoNothing();
		
	}

	void Start()
	{
		UIManager.Instance.Init();
		Destroy(gameObject);
	}
}
