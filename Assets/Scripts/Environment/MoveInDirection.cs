using UnityEngine;
using System.Collections;

public class MoveInDirection : MonoBehaviour 
{
	public Vector3 direction;
	public float velocity;

	void Start () 
	{
		
	}
	
	void Update () 
	{
		transform.position += direction.normalized * velocity * Time.deltaTime;
	}
}
