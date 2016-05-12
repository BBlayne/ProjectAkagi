using UnityEngine;
using System.Collections;

public class NetworkPlayerTest : Photon.MonoBehaviour {
    public Color color = Color.red;

    public GameObject light;

    public delegate void ClickAction(Color _color);
    public event ClickAction onClicked;

	// Use this for initialization
	void Start () {
        Random.seed = System.DateTime.Now.Millisecond;
        color = new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));
	    if (photonView.isMine)
        {
            light.GetComponent<LightTest>().enabled = true;
        }
        else
        {

        }
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player, send other players our data

        }
        else
        {
            // Network player, recieve data

        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            if (onClicked != null)
                onClicked(color);
        }
	
	}
}

