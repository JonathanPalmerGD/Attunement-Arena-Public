using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
	public int playerID = 0;

	public RigidbodyFirstPersonController controller;
	//public List<Ability> rituals;

	public GameObject hitscanTarget = null;
	public Vector3 targetScanDir = Vector3.zero;
	public Vector3 hitscanContact = Vector3.zero;

	public CameraController cameraController;
	public Camera myCamera;

	private float health;
	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = value;
		}
	}
	private float mana;
	public float Mana
	{
		get
		{
			return mana;
		}
		set
		{
			mana = value;
		}
	}

	public List<Ability> abilities;
	public Dictionary<string, Ability> abilityBindings;

	public Ability AddAbility(string abilityName, string keyBinding, string displayKeyBinding)
	{
		Ability newAbility = ScriptableObject.CreateInstance(abilityName) as Ability;

		newAbility.Init(this, keyBinding, displayKeyBinding);

		abilities.Add(newAbility);
		abilityBindings.Add(keyBinding, newAbility);

		return newAbility;
	}

	void Start()
	{
		if (cameraController == null)
		{
			cameraController = GetComponent<CameraController>();
			cameraController.PositionCamera((byte)playerID);
		}
		if (myCamera == null)
		{
			myCamera = GetComponent<Camera>();
		}
		abilities = new List<Ability>();
		abilityBindings = new Dictionary<string, Ability>();

		controller = GetComponent<RigidbodyFirstPersonController>();
	}

	public void UseAbility()
	{

	}

	void Update()
	{
		GetInput();

		hitscanTarget = TargetScan();

		for (int i = 0; i < abilities.Count; i++)
		{
			abilities[i].UpdateAbility(GameManager.Instance.modifiedTimeScale * Time.deltaTime);
		}
	}

	GameObject TargetScan()
	{
		//Where the mouse is currently targeting.
		Ray ray = myCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hit;
		//Debug.DrawLine(transform.position, (transform.position + ray) * 100, Color.green);

		//If we fire, set targetScanDir to someplace arbitrarily far away in the shooting. Even if we hit something, we want to target wherever the cursor pointed.
		targetScanDir = transform.position + (ray.direction * 500);

		//Mask so we don't consider targeting ourself.
		LayerMask layerMask = ~((1 << 2) | (1 << 8));
		//LayerMask layerMask = ~(1 << LayerMask.NameToLayer ("Player"));

		//If we hit something
		if (Physics.Raycast(ray, out hit, 1500, layerMask))
		{
			hitscanContact = hit.point;
			//Handle cases for what we hit.

			//Debug.Log(hit.collider.gameObject.tag + "\n");
			//if (hit.collider.gameObject.tag == "NPC")
			//{
			//	NPC n = hit.collider.gameObject.GetComponent<NPC>();
			//	CheckNewTarget((NPC)n);

			//	return n.gameObject;
			//}
			//else if (hit.collider.gameObject.tag == "Enemy")
			//{
			//	Enemy e = hit.collider.gameObject.GetComponent<Enemy>();
			//	CheckNewTarget((Enemy)e);

			//	return e.gameObject;
			//}
			////This is outdated. You used to be able to target terrain.
			//else if (hit.collider.gameObject.tag == "WorldObject")
			//{
			//	//Island e = hit.collider.gameObject.GetComponent<Island>();
			//	//CheckNewTarget((Island)e);
			//}
			//else if (hit.collider.gameObject.tag == "Projectile")
			//{
			//	Projectile p = hit.collider.gameObject.GetComponent<Projectile>();
			//	if (p != null)
			//	{
			//		return p.gameObject;
			//	}
			//}
			//else
			//{
				//Catch all case.
				return hit.collider.gameObject;
			//}
			//Debug.Log(hit.collider.gameObject.name + "\n");
		}
		else
		{
			hitscanContact = targetScanDir;
			//We didn't hit anything. We're about to return null as the default.
			//Debug.Log("Targetting nothing\n");
		}
		return null;
	}

	public void GetInput()
	{

	}
}
