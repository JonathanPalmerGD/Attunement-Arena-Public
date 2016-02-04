using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AirCurrent : MonoBehaviour
{
	public ParticleSystem airRenderer;
	public List<ParticleSystem> airCurrentComponents;
	public float maxDist = 5;
	public float pushSpeed = 150;

	#region Vector Math Values
	Vector3 A;
	Vector3 B;
	Vector3 C;
	Vector3 D;
	Vector3 AB;
	#endregion

	public ParticleSystem startZone, endZone;

	void Start()
	{
		airCurrentComponents = new List<ParticleSystem>();
		for (int i = 0; i < transform.childCount; i++)
		{
			airCurrentComponents.Add(transform.GetChild(i).GetComponent<ParticleSystem>());
		}
		
		for (int i = 0; i < airCurrentComponents.Count; i++)
		{
			startZone = airCurrentComponents[i];
			if (i == 0)
			{
				endZone = airCurrentComponents[airCurrentComponents.Count - 1];
			}
			else
			{
				endZone = airCurrentComponents[i - 1];
			}

			if (startZone && endZone)
			{
				ConfigureVisualEffect(startZone, endZone);
			}
		}
	}

	void Update()
	{
		for (int i = 0; i < airCurrentComponents.Count; i++)
		{
			startZone = airCurrentComponents[i];
			if (i == 0)
			{
				endZone = airCurrentComponents[airCurrentComponents.Count - 1];
			}
			else
			{
				endZone = airCurrentComponents[i - 1];
			}

			if (startZone && endZone)
			{
				CheckCurrentSegment(startZone, endZone);
			}
		}
	}

	public void ConfigureVisualEffect(ParticleSystem startZone, ParticleSystem endZone)
	{
		Vector3 startPos = startZone.transform.position;
		Vector3 endPos = endZone.transform.position;

		float particleVel = pushSpeed / 3;
		float dist = Vector3.Distance(startPos, endPos);
		float lifeTime = dist / particleVel;

		startZone.startLifetime = lifeTime * 1.05f;
		startZone.startSpeed = particleVel;
		startZone.emissionRate = 100/ lifeTime;
		startZone.transform.LookAt(endPos);
	}

	public void CheckCurrentSegment(ParticleSystem startZone, ParticleSystem endZone)
	{
		A = startZone.transform.position;
		B = endZone.transform.position;

		D = Vector3.zero;
		foreach (Player player in GameManager.Instance.players)
		{
			C = player.transform.position;

			AB = B - A;

			float t = Vector3.Dot(C - A, AB) / Vector3.Dot(AB, AB);

			D = A + t * AB;

			Vector3 playerToD = player.transform.position - D;
			Vector3 playerToA = player.transform.position - A;
			Vector3 playerToB = player.transform.position - B;

			if (playerToD.sqrMagnitude < maxDist * maxDist)
			{
				bool betweenPoints = false;
				bool nearANode = false;
				bool nearBNode = false;
				bool overInputThreshhold = false;

				if (t > 0 && t < 1f)
				{
					betweenPoints = true;
				}

				if (playerToA.sqrMagnitude < maxDist * maxDist)
				{
					nearANode = true;
				}
				if (playerToA.sqrMagnitude < maxDist * maxDist)
				{
					nearBNode = true;
				}

				//This is how the player escapes the air current.
				//One part listening to the player input
				float reverseInputAmt = 1 - Mathf.Clamp(player.controller.inputAmt, 0, 1);

				//Another part recognizing if the player has a strong force attached to them.
				if (reverseInputAmt < .5f || player.controller.forceAmt > 2)
				{
					overInputThreshhold = true;
				}

				if (!overInputThreshhold)
				{
					if (betweenPoints)
					{
						player.controller.mRigidBody.velocity = Vector3.zero;
						player.controller.mRigidBody.useGravity = false;
						player.controller.ApplyConstantForce(reverseInputAmt * AB.normalized * pushSpeed + -playerToD * pushSpeed / 5, true, true);
					}
					else if (nearANode)
					{
						player.controller.mRigidBody.velocity = Vector3.zero;
						player.controller.mRigidBody.useGravity = false;
						player.controller.ApplyConstantForce(reverseInputAmt * AB.normalized * pushSpeed + -playerToD * pushSpeed / 5, true, true);
					}
					else if (nearBNode)
					{
						player.controller.mRigidBody.velocity = Vector3.zero;
						player.controller.mRigidBody.useGravity = false;
						player.controller.ApplyConstantForce(reverseInputAmt * AB.normalized * pushSpeed + -playerToD * pushSpeed / 5, true, true);
					}
				}
			}
			else
			{
				player.controller.mRigidBody.useGravity = true;
			}

			//Debug.Log("Distance between player and line: " + (player.transform.position - D).magnitude + "\n");

			//Debug.DrawLine(C, D, Color.yellow);
			//Debug.DrawLine(A, D, Color.red);
			//Debug.DrawLine(B, D, Color.green);
			//Debug.DrawLine(C, D, Color.yellow);

		}

	}

	void OnDrawGizmos()
	{
		if (airCurrentComponents != null && airCurrentComponents.Count > 0)
		{
			for (int i = 0; i < airCurrentComponents.Count; i++)
			{
				if (i == 0)
				{
					DrawGizmoCurrent(airCurrentComponents[i].gameObject, airCurrentComponents[airCurrentComponents.Count - 1].gameObject);
				}
				else
				{
					DrawGizmoCurrent(airCurrentComponents[i].gameObject, airCurrentComponents[i - 1].gameObject);
				}

			}
		}
	}

	void DrawGizmoCurrent(GameObject startPos, GameObject endPos)
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(startPos.transform.transform.position, maxDist / 2);

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(startPos.transform.position, endPos.transform.position);
	}
}
