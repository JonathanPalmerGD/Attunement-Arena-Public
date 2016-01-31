using UnityEngine;
using System.Collections;

public class Tokens : MonoBehaviour 
{
	public Vector2 timeRange = new Vector2(5, 15);
	public ParticleSystem partSys;
	public float counter = 0;
	public Vector2 manaGainRange = new Vector2(10, 25);
	public bool collectable = false;

	void Start()
	{
		if (Random.Range(0, 10) > 7)
		{
			collectable = true;
		}
	}

	void Update()
	{
		if (!collectable)
		{
			if (counter > 0)
			{
				counter -= Time.deltaTime;
			}
			if (counter < 0)
			{
				partSys.enableEmission = true;
				collectable = true;
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Player plyr = other.GetComponent<Player>();
			if (plyr)
			{
				plyr.AdjustMana(Random.Range(manaGainRange.x, manaGainRange.y));
				collectable = false;
				partSys.enableEmission = false;
				counter = Random.Range(timeRange.x, timeRange.y);
			}
		}
	}
}
