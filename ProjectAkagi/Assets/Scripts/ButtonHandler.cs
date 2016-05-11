using UnityEngine;
using System.Collections;

public class ButtonHandler : MonoBehaviour {
    public GameController controller;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void myClick()
    {
        controller.currentPlayer.SendBetting();
    }
}
