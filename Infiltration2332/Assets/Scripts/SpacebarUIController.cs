using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacebarUIController : MonoBehaviour {

    GameObject hero;
    CanvasRenderer cr;
    // Use this for initialization
    void Start () {
        hero = GameObject.Find("Hero");
        cr = GetComponent<CanvasRenderer>();
        cr.SetAlpha(1);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
