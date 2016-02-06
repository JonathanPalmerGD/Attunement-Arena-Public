using UnityEngine;
using System.Collections;

public class VerticalLoop : MonoBehaviour 
{
	//We oscillate back and forth across our starting position at a frequency and an amplitude.
	public float verticalAmplitude = 1.0f;
	public WindMovement parent;
	
	private Vector3 initial;

	void Start()
	{
		initial = transform.localPosition;
	}

	void Update()
	{
		transform.localPosition = initial + Vector3.up * verticalAmplitude * parent.GetLoopPercentage();
	}
}
