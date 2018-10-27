using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroUIScript : MonoBehaviour {

    GameObject hero;
    GameObject spider;
    CanvasRenderer cr;
    void Start()
    {
        hero = GameObject.Find("Hero");
		spider = GameObject.Find ("Spider");
        cr = GetComponent<CanvasRenderer>();
        cr.SetAlpha(0.0f);
    }

    // Update is called once per frame
    void Update()
    {
		if (hero != null) 
		{
			HeroController hCtrl = hero.GetComponent<HeroController> ();
			if (hCtrl.EnableMovement)
				cr.SetAlpha (0.0f);
			else
				cr.SetAlpha (1.0f);
		}
    }

}
