using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderUIController : MonoBehaviour 
{

    // Use this for initialization
    GameObject hero;
    GameObject spider;
    CanvasRenderer cr;
    void Start ()
    {
        hero = GameObject.Find("Hero");
        cr = GetComponent<CanvasRenderer>();
        cr.SetAlpha(1);
    }
	
	// Update is called once per frame
	void Update () {

		if (hero != null) 
		{
			HeroController hCtrl = hero.GetComponent<HeroController> ();
			if (hCtrl.EnableMovement) 
			{
				cr.SetAlpha (1.0f);
			} 
			else 
			{
				cr.SetAlpha (0.0f);
			}
		}

    }
}
