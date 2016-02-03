using UnityEngine;
using System.Collections;

public class AirCurrent : MonoBehaviour
{
	public AirCurrent targetPos;
	public Vector3 A;
	public Vector3 B;
	public Vector3 C;
	public Vector3 D;
	public Vector3 AB;
	public float maxDist = 5;
	public float pushSpeed = 25;

	void Start()
	{
		A = transform.position;
		if (targetPos != null)
		{
			B = targetPos.transform.position;
		}
	}

	void Update()
	{
		if (targetPos != null)
		{
			D = Vector3.zero;
			foreach (Player player in GameManager.Instance.players)
			{
				C = player.transform.position;

				AB = B - A;

				float t = Vector3.Dot(C - A, AB) / Vector3.Dot(AB, AB);

				//if (t < 0)
				//{
				//	t = -.01f;
				//}
				//if (t > 1)
				//{
				//	t = 1.01f;
				//}

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

					float reverseInputAmt = 1- Mathf.Clamp(player.controller.inputAmt, 0, 1);

					if (reverseInputAmt < .5f)
					{
						overInputThreshhold = true;
					}
					//Debug.Log(reverseInputAmt + "\n");

					if (!overInputThreshhold)
					{
						if (betweenPoints)
						{
							player.controller.mRigidBody.velocity = Vector3.zero;
							player.controller.mRigidBody.useGravity = false;
							player.controller.ApplyExternalForce(reverseInputAmt * AB.normalized * pushSpeed + -playerToD * pushSpeed / 5, true, true);
						}
						else if (nearANode)
						{
							player.controller.mRigidBody.velocity = Vector3.zero;
							player.controller.mRigidBody.useGravity = false;
							player.controller.ApplyExternalForce(reverseInputAmt * AB.normalized * pushSpeed + -playerToD * pushSpeed / 5, true, true);
						}
						else if (nearBNode)
						{
							player.controller.mRigidBody.velocity = Vector3.zero;
							player.controller.mRigidBody.useGravity = false;
							player.controller.ApplyExternalForce(reverseInputAmt * AB.normalized * pushSpeed + -playerToD * pushSpeed / 5, true, true);
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


		//If a player is within me
		//Push them towards the target
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, maxDist / 2);
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(A, B);
	}
}
