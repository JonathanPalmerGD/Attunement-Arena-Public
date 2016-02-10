using UnityEngine;
using System.Collections;

public class ExtractProj : MonoBehaviour
{
	public enum ProjType
	{
		Air, Water, Lava
	}
	private ProjType projType;

	public float Radius
	{
		get { return transform.localScale.x * 0.5f; }
	}

	public Extract origin;

	public void Init(Extract from, ProjType projType)
	{
		origin = from;

		GetComponent<SphereCollider>().enabled = false;
		name = from.Owner.name + "'s " + projType.ToString() + " Extract Projectile";
		transform.SetParent(from.Owner.transform);
		transform.localPosition = new Vector3(0f, 2f, 1.5f);
		transform.localScale = Vector3.one * 0.3f;
		transform.localRotation = Quaternion.identity;

		RenderBallisticPath rbp = gameObject.GetComponent<RenderBallisticPath>();
		rbp.enabled = false;

		Color clr;
		switch (projType)
		{
			case ProjType.Air:
				clr = new Color(1.0f, 1.0f, 1.0f, 0.25f);
				rbp.initialVelocity = 25f * from.ProjSpeedMult;
				break;
			case ProjType.Water:
				clr = new Color(0.25f, 0.25f, 1.0f, 0.25f);
				rbp.initialVelocity = 15f * from.ProjSpeedMult;
				break;
			case ProjType.Lava:
				clr = new Color(1.0f, 0.2f, 0.2f, 1f);
				rbp.initialVelocity = 10f * from.ProjSpeedMult;
				break;
			default:
				clr = Color.magenta;
				rbp.initialVelocity = 15f * from.ProjSpeedMult;
				break;
		}

		Material mat = GetComponent<MeshRenderer>().material;
		mat.color = clr;
		mat.SetFloat("_Mode", 2f);

		clr.a = 1.0f;
		LineRenderer lr = GetComponent<LineRenderer>();
		lr.material.color = clr;

		this.projType = projType;
	}

	public void Throw()
	{
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		Destroy(GetComponent<RenderBallisticPath>());
		Destroy(GetComponent<LineRenderer>());

		switch (projType)
		{
			case ProjType.Air:
				rb.useGravity = false;
				rb.velocity = transform.forward * (25f * origin.ProjSpeedMult);
				break;
			case ProjType.Water:
			default:
				rb.velocity = transform.forward * (15f * origin.ProjSpeedMult);
				break;
			case ProjType.Lava:
				rb.velocity = transform.forward * (10f * origin.ProjSpeedMult);
				break;
		}
		transform.SetParent(origin.Owner.transform.parent);
		GetComponent<SphereCollider>().enabled = true;
		Destroy(gameObject, 10f);
	}

	void OnCollisionEnter(Collision collision)
	{
		Vector3 where = collision.contacts[0].point;
		float blastRadius = Radius * 9f * origin.ProjSpreadMult;

		foreach(Player p in GameManager.Instance.players)
		{
			var tetherVector = p.transform.position - where;
			if (tetherVector.sqrMagnitude > blastRadius*blastRadius) continue;

			float effect = 1.0f;
			if(p == origin.Owner)
			{
				continue;
			}

			switch(projType)
			{
				case ProjType.Air:
					p.controller.ApplyExternalForce(tetherVector.normalized * (effect * 300f * Radius));
                    break;
				case ProjType.Lava:
					p.controller.ApplyExternalForce(tetherVector.normalized * (effect * 20f * Radius ));
					p.AdjustHealth((effect * -25f * Radius));
					break;
				case ProjType.Water:
					p.controller.ApplyExternalForce(tetherVector.normalized * (effect * 50f * Radius));
					p.AdjustHealth((effect * -15f * Radius));
					break;
				default:
					break;
			}
		}

		ParticleSystem particles = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Effects/"+projType.ToString() + "BallPff")).GetComponent<ParticleSystem>();
		particles.transform.position = where;
		particles.Emit(450);
		Destroy(particles.gameObject, 5f);


		Destroy(gameObject);
	}
}
