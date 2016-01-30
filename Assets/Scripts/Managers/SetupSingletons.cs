using UnityEngine;
using System.Collections;

public class SetupSingletons : MonoBehaviour
{
	void Awake ()
	{
		GameCanvas.Instance.DoNothing();
		//InvManager.Instance.DoNothing();
		UIManager.Instance.DoNothing();
		//EventManager.Instance.DoNothing();
		//StoryRecordManager.Instance.DoNothing();	
		//TimeManager.Instance.DoNothing();
		GameManager.Instance.DoNothing();

		Destroy(gameObject);
	}
}
