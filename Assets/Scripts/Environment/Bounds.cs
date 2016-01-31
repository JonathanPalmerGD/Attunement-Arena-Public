using System;
using System.Collections.Generic;
using UnityEngine;

class Bounds : MonoBehaviour
{
	public float Radius = 150f;

	void Start()
	{

	}

	void Update()
	{
		for (int i = 0; i < GameManager.Instance.players.Length; i++)
		{
			Player player = GameManager.Instance.players[i];
			float dist = Vector3.Distance(player.transform.position, transform.position);
			
			if(player.transform.position.y < transform.position.y - 25)
			{
				Vector3 oldVel = player.controller.mRigidBody.velocity;
				//Debug.Log(oldVel.y + "\n");
				player.controller.mRigidBody.velocity = new Vector3(oldVel.x, 80, oldVel.z);
				//Debug.Log(-2.25f * oldVel.y + "\n");

				player.AdjustHealth(-15);
				player.SetGustCharges(2);
			}

			if (dist > Radius)
			{
				player.controller.ApplyExternalForce(50 * (transform.position - player.transform.position).normalized);
				player.AdjustHealth(-5);
			}

		}
	}

	bool InBounds(Vector3 position)
	{
		return (transform.position - position).sqrMagnitude <= (Radius * Radius);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0f, 0f, 0f, 0.25f);
		Gizmos.DrawSphere(transform.position, Radius);
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(transform.position, Radius);
		Gizmos.color = Color.white;
	}
}
