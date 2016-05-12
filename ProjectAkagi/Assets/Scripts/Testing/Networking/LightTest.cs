using UnityEngine;
using System.Collections;

public class LightTest : MonoBehaviour {
    public Renderer mRenderer;
    public GameObject parent;
    public NetworkPlayerTest player;

    void Awake()
    {
        
    }

    void OnEnable()
    {
        player = parent.GetComponent<NetworkPlayerTest>();
        player.onClicked += ChangeColor;
    }

    void OnDisable()
    {
        player.onClicked -= ChangeColor;
    }

    void ChangeColor(Color color)
    {
        mRenderer.material.color = color;
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
