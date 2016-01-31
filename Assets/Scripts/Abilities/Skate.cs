using UnityEngine;
using System.Collections;

public class Skate : Ability
{
	public GameObject icePrefab;
	public override KeyActivateCond activationCond
	{
		get { return KeyActivateCond.KeyHold; }
	}
	public override bool UseCharges
	{
		get
		{
			return false;
		}
	}
	public override int IconID
	{
		get { return 5; }
	}
	public override float GeneralDamage
	{
		get
		{
			return 0f;
		}
	}
	public float Force = 15;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		icePrefab = Resources.Load<GameObject>("icePrefab");
		
		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void UpdateAbility(float deltaTime)
	{
		base.UpdateAbility(deltaTime);
		
	}
	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		//Make an icePrefab below the player.
		//Debug.Log("Execute Skate!\n");

		GameObject newPlatform = GameObject.Instantiate<GameObject>(icePrefab);
		
		float yDiff = inputVector.normalized.y;
		newPlatform.transform.position = Owner.transform.position - (inputVector * .4f) - (Vector3.up * yDiff);


		Vector3 lookAtPos = new Vector3((Owner.transform.position + inputVector).x, newPlatform.transform.position.y, (Owner.transform.position + inputVector).z);
		newPlatform.transform.LookAt(lookAtPos);

		Vector3 oldVel = Owner.controller.mRigidBody.velocity;
		
		if (Owner.Grounded)
		{
			Owner.controller.Jumping = true;
		}

		Vector3 forwardForce;
		if (Owner.transform.position.y < Owner.transform.position.y + inputVector.y)
		{
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, Mathf.Clamp(oldVel.y, 0, 100), oldVel.z);
			//Debug.Log("Gain height\n");
			forwardForce = new Vector3(inputVector.x, inputVector.y * 8, inputVector.z).normalized;
			
			//Owner.SendMessage("ApplyExternalForce", );
		}
		else
		{
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, Mathf.Clamp(oldVel.y, -12, 100), oldVel.z);
			//Debug.Log("Forward\n");
			forwardForce = new Vector3(inputVector.x, inputVector.y * 6, inputVector.z).normalized;
			//Owner.SendMessage("ApplyExternalForce", inputVector * Force);
		}

		Owner.controller.AddExternalForce(forwardForce * Force, ForceMode.Impulse, true);
		//Owner.SendMessage("ApplyExternalForce", Vector3.up * Force);
		
		GameObject.Destroy(newPlatform, Duration);

	}
}
