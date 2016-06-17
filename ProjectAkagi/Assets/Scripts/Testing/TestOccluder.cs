using UnityEngine;
using System.Collections;

public class TestOccluder : MonoBehaviour {

    void Awake()
    {
        GameObject[] occluderGOs = GameObject.FindGameObjectsWithTag("Occluder");

        Debug.Log("(Awake) Occluding objects: " + occluderGOs.Length);
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
