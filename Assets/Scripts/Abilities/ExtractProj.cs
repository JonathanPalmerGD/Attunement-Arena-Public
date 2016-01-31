using UnityEngine;
using System.Collections;

public class ExtractProj : MonoBehaviour
{
	private Extract.ProjType projType;

	private float Radius
	{
		get { return transform.localScale.x; }
	}

	public Extract origin;

	public void Init(Extract from, Extract.ProjType projType)
	{
		origin = from;

		GetComponent<SphereCollider>().enabled = false;
		name = from.Owner.name + "'s " + projType.ToString() + " Extract Projectile";
		transform.SetParent(from.Owner.transform);
		transform.localPosition = new Vector3(0f, 2f, 1.5f);
		transform.localScale = Vector3.one * 0.1f;

		Color clr;
		switch(projType)
		{
			case Extract.ProjType.Air:
				clr = new Color(1.0f, 1.0f, 1.0f, 0.25f);
				break;
			case Extract.ProjType.Lava:
				clr = new Color(1.0f, 0.2f, 0.2f, 1f);
				break;
			case Extract.ProjType.Water:
				clr = new Color(0.25f, 0.25f, 1.0f, 0.25f);
				break;
			default:
				clr = Color.magenta;
				break;
		}

		Material mat = GetComponent<MeshRenderer>().material;
		mat.color = clr;
		mat.SetFloat("_Mode", 2f);


		this.projType = projType;
	}

	public void Throw(Vector3 where)
	{
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		switch(projType)
		{
			case Extract.ProjType.Air:
				rb.SetDensity(0.1f);
				
				break;
			case Extract.ProjType.Water:
			default:
				break;
			case Extract.ProjType.Lava:
				rb.SetDensity(2.0f);
				break;
		}
	}
}
