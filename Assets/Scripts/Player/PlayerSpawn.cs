using UnityEngine;
using System.Collections;

public class PlayerSpawn : MonoBehaviour
{
	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(transform.position, Vector3.one * 2f);
		Gizmos.color = Color.white;
	}
}
