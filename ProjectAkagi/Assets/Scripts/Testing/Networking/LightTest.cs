using UnityEngine;
using System.Collections;

public class LightTest : MonoBehaviour {
    public Renderer mRenderer;
    public NetworkPlayerTest player;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    void Awake()
    {
        
    }

    void OnEnable()
    {
        player.onClicked += ChangeColor;
    }

    void OnDisable()
    {
        player.onClicked -= ChangeColor;
    }

    void ChangeColor(Color color)
    {
        if (mRenderer != null)
            mRenderer.material.color = color;

        Vector3 _color = new Vector3(color.r, color.g, color.b);
        GetComponent<PhotonView>().RPC("ChangeColorNetwork", PhotonTargets.All, _color);
    }

    [PunRPC]
    void ChangeColorNetwork(Vector3 _color)
    {
        Color col = new Color(_color.x, _color.y, _color.z, 1);
        if (mRenderer != null)
            mRenderer.material.color = col;
    }

    // Use this for initialization
    void Start () {
        player = this.transform.parent.gameObject.GetComponent<NetworkPlayerTest>();
        mRenderer = this.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
