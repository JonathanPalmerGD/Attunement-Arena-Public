using System;
using System.Collections.Generic;
using UnityEngine;

class Bounds : MonoBehaviour
{
	public float Radius = 150f;

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
