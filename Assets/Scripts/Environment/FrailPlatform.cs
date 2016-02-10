using UnityEngine;
using System.Collections;

public class FrailPlatform : MonoBehaviour
{
	public float fallAccel = 2;
	public bool isBroken = false;
	public int MaxStrikes;
	private int strikes;
	public int Strikes
	{
		get { return strikes; }
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (value > MaxStrikes)
			{
				value = MaxStrikes;
			}

			if (value <= 0 && !isBroken)
			{
				StartCoroutine("PlatformFall");
				strikes = value;
				GetComponent<Renderer>().material = GameManager.Instance.PlatformAppearance[strikes];
			}
			else
			{
				strikes = value;
				GetComponent<Renderer>().material = GameManager.Instance.PlatformAppearance[strikes];
			}
		}
	}

	void Start()
	{
		if (MaxStrikes > GameManager.Instance.PlatformAppearance.Count)
		{
			MaxStrikes = GameManager.Instance.PlatformAppearance.Count;
		}
		Strikes = MaxStrikes;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			if (Strikes > 0)
			{
				Strikes -= 1;
			}
		}
	}

	float platformDeathTimer = 0;
	public IEnumerator PlatformFall()
	{
		platformDeathTimer = 15;
		isBroken = true;

		Rigidbody rbody = GetComponent<Rigidbody>();
		rbody.isKinematic = false;
		while (platformDeathTimer > 0)
		{
			rbody.velocity += Vector3.down * fallAccel * Time.deltaTime;
			platformDeathTimer -= Time.deltaTime;
			yield return null;
		}
		GameObject.Destroy(gameObject);
	}

}
