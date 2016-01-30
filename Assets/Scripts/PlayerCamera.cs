using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {
    private Camera myCam;

    void Start() {
        myCam = GetComponent<Camera>();
        PositionCamera(1, 0);
    }

    public void PositionCamera(byte playerCount, byte myPlayerID) {
        switch(playerCount) {
            default:
            case 0:
                return;
            case 1:
                myCam.rect = new Rect(0f, 0f, 1f, 1f);
                return;
            case 2:
                myCam.rect = new Rect(0f, myPlayerID == 1 ? 0f : 0.5f, 1f, 0.5f);
                return;
            case 3:
            case 4:
                myCam.rect = new Rect(((myPlayerID % 2 == 1) ? 0.5f : 0f), ((myPlayerID > 1) ? 0f : 0.5f), 0.5f, 0.5f);
                return;
        }
    }

}
