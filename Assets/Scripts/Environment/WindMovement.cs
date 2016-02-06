using UnityEngine;
using System.Collections;

public class WindMovement : MonoBehaviour 
{
	public Vector3 direction;
	public Vector3 rotatingDir;
	public float wanderAmt = .25f;
	private float inaccuracy = .02f;
	public float velocity;
	public float rotationSpeed;
	public VerticalLoop vLoop;
	float angle = 0;

	public bool looping;
	public float loopCounter;
	public float loopDuration = 2;

	public float loopChance = 0f;
	public float loopThreshold = 20f;
	Quaternion qTo;

	private Vector3 Randomize(Vector3 newVector, float devation)
	{
		newVector += new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)) * devation;
		newVector.Normalize();
		return newVector;
	}

	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			BeginLoop();
		}

		if (wanderAmt > 0)
		{
			direction = Randomize(direction, inaccuracy);
		}

		if (looping)
		{
			loopCounter += Time.deltaTime;

			//Debug.Log(GetLoopPercentage() + "\n");

			transform.position += direction.normalized * -GetLoopPercentage() * velocity * Time.deltaTime;

			if (loopCounter >= loopDuration)
			{
				loopCounter = 0;
				looping = false;
			}
		}
		else
		{

			transform.position += direction.normalized * velocity * Time.deltaTime;
		}
	}

	public float GetLoopPercentage()
	{
		return -Mathf.Cos((loopCounter/loopDuration) * 2 * Mathf.PI);
	}

	void BeginLoop(float rotationSpeedMin = 15, float rotationSpeedMax = 20)
	{
		rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
		looping = true;
		loopCounter = 0;
	}
}
