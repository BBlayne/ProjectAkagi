using UnityEngine;
using System.Collections;

public class Slot : MonoBehaviour {
    public int slotId = -1;

    public Transform peekPoint;
    public Transform foldPoint;
    public Transform chips;
    public Transform uiPos;
    public Transform PlayerHandPosition;
    public Transform PlayerHand;
    public Transform dealerButton;

    // The root card refers to the "PlayingCard" object that
    // has the animator.
    public GameObject mCardRoot0;
    public GameObject mCardRoot1;
    // The child card refers to the card itself, with the
    // model and material/renderer.
    public GameObject mCardChild0;
    public GameObject mCardChild1;

    // The Peeking eye sprite
    public GameObject mEyes;

    public GameObject[] mHighlightChips;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
