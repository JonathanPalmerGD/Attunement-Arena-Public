using UnityEngine;
using System.Collections;

public class EarthFists : MonoBehaviour
{
	public Smash Creator;
	public GameObject deathPrefab;

	void Start()
	{
		deathPrefab = Resources.Load<GameObject>("Effects/RockCrumblePrefab");
	}
	void SetupFist()
	{
		transform.rotation = Quaternion.identity;
		transform.RotateAround(Creator.Owner.gameObject.transform.position, Creator.Owner.gameObject.transform.right, -55);
	}

	void Update()
	{
		transform.RotateAround(Creator.Owner.gameObject.transform.position, Creator.Owner.gameObject.transform.right, 55 * Time.deltaTime);
	}

	public void PlayDeath()
	{
		GameObject go = GameObject.Instantiate(deathPrefab, transform.position, Quaternion.identity) as GameObject;
		GameObject.Destroy(go, 5.5f);
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject != Creator.Owner.gameObject)
		{
			if (!other.isTrigger)
			{
				//Debug.Log(other.name + "\n");
				//if(other.gameObject.layer
				//If it is a ground object
				if (other.tag == "Ground")
				{
					//Ground Pound
					Creator.OnGroundImpact();
				}
				else
				{
					//If we hit a player or object
					Creator.Collide();
				}

				PlayDeath();
				Creator.SetFistActivity = false;
			}
		}
	}
}
