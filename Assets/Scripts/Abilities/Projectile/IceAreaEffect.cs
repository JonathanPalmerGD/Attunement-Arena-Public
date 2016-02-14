using UnityEngine;
using System.Collections;

public class IceAreaEffect : MonoBehaviour
{
	public Player Owner;
	public Skate Creator;

	public void OnTriggerEnter(Collider c)
	{
		if (c.name != Owner.name)
		{
			if (c.gameObject.tag == "Player")
			{
				Player other = c.GetComponent<Player>();
				if (other != null)
				{
					Status stat = null;
					if (Creator.slowMultAdj != 0)
					{
						stat = other.AddStatus(Creator, Status.StatusTypes.Slowed, Creator.Duration, Creator.slowMultAdj, true, other.chilledParticles, true, false);
					}

					if (Creator.GeneralDamage != 0)
					{
						stat = other.AddStatus(Creator, Status.StatusTypes.Bleed, Creator.Duration, Creator.GeneralDamage, true, other.chilledParticles, true, false);
					}

				}
			}
		}
	}
}
