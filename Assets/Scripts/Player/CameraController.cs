using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	private Player Owner;

	public void PositionCamera(byte myPlayerID)
	{
		switch (GameManager.Instance.players.Length)
		{
			default:
			case 0:
				return;
			case 1:
				Owner.myCamera.rect = new Rect(0f, 0f, 1f, 1f);
				return;
			case 2:
				Owner.myCamera.rect = new Rect(0f, myPlayerID == 1 ? 0f : 0.5f, 1f, 0.5f);
				return;
			case 3:
			case 4:
				Owner.myCamera.rect = new Rect(((myPlayerID % 2 == 1) ? 0.5f : 0f), ((myPlayerID > 1) ? 0f : 0.5f), 0.5f, 0.5f);
				return;
		}
	}

}
