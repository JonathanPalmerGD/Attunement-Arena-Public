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
					other.SetPlayerStatus(Player.PlayerStatus.Chilled, Creator.Duration);
					//other.AdjustHealth(-Creator.GeneralDamage * Time.deltaTime);
				}
			}
		}
	}
}
