using UnityEngine;
using System.Collections;

public class NetworkPlayerTest : Photon.MonoBehaviour {
    public Color color = Color.white;

    public GameObject light;

    public delegate void ClickAction(Color _color);
    public event ClickAction onClicked;

	// Use this for initialization
	void Start () {
        Random.seed = System.DateTime.Now.Millisecond;
        if (photonView.isMine)
        {
            //light.GetComponent<LightTest>().enabled = true;
        }
        else
        {

        }
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Vector3 mColor;
        if (stream.isWriting)
        {
            //mColor = new Vector3(color.r, color.g, color.b);
            //Debug.Log("OnPhotonSerializeView:Sending " + mColor);
            // We own this player, send other players our data
            //stream.SendNext(mColor);

        }
        else
        {
            // Network player, recieve data            
            //mColor = (Vector3)stream.ReceiveNext();
            //this.color = new Vector4(mColor.x, mColor.y, mColor.z, 1);
            //Debug.Log("OnPhotonSerializeView:Recieving " + this.color);
            //this.light.GetComponent<Renderer>().material.color = this.color;
        }
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 mColor;           
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            this.color = new Color(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f));
            mColor = new Vector3(this.color.r, this.color.g, this.color.b);
            //this.light.GetComponent<Renderer>().material.color = this.color;
            if (onClicked != null)
            {
                onClicked(this.color);
            }
        }
	
	}

    void OnGUI()
    {
        GUI.Label(new Rect(10, 20, 200, 200), this.color.ToString());
    }

    void LateUpdate()
    {

    }
}

