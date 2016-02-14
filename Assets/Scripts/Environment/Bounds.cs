using System;
using System.Collections.Generic;
using UnityEngine;

class Bounds : MonoBehaviour
{
	public float Radius = 150f;
	public float BoundFloor = -25;
	public bool ConsiderFloor = true;

	void Start()
	{

	}

	void Update()
	{
		for (int i = 0; i < GameManager.Instance.players.Length; i++)
		{
			Player player = GameManager.Instance.players[i];
			float dist = Vector3.Distance(player.transform.position, transform.position);

			if (ConsiderFloor)
			{
				if (player.transform.position.y < transform.position.y + BoundFloor)
				{
					Vector3 oldVel = player.controller.mRigidBody.velocity;
					//Debug.Log(oldVel.y + "\n");
					player.controller.mRigidBody.velocity = new Vector3(oldVel.x, 80, oldVel.z);
					//Debug.Log(-2.25f * oldVel.y + "\n");

					//Debug.Log("Hit bounds\n" + player.transform.position + "\n" + (transform.position.y - BoundFloor));
					player.AdjustHealth(-15);
					player.GetAbility<Gust>().Charges += 2;
				}
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
		//Gizmos.DrawSphere(transform.position, Radius);
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(transform.position, Radius);
		Gizmos.color = new Color(0f, 0f, 0f, 0.50f);
		if (ConsiderFloor)
			Gizmos.DrawCube(transform.position - Vector3.down * BoundFloor, new Vector3(Radius * 1.5f, 1, Radius * 1.5f));
		Gizmos.color = Color.white;
	}
}
