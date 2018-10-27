using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCardUIController : MonoBehaviour {

    public string CardColor;
	GameObject hero;
	GameObject spider;
	CanvasRenderer cr;
	// Use this for initialization
	void Start ()
    {
		hero = GameObject.Find("Hero");
        cr = GetComponent<CanvasRenderer>();
        cr.SetAlpha(0);
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (hero != null) 
		{
			HeroController hCtrl = hero.GetComponent<HeroController> ();

			if (hCtrl.EnableMovement) 
			{
			
				if (CardColor.Equals ("red")) 
				{
					if (hCtrl.HasRedKeyCard) 
					{
						cr.SetAlpha (1.0f);
					} 
					else 
					{
						cr.SetAlpha (0);
					}
				}
				if (CardColor.Equals ("blue")) 
				{
					if (hCtrl.HasBlueKeyCard)
					{
						cr.SetAlpha (1.0f);
					} 
					else 
					{
						cr.SetAlpha (0);
					}
				}

			} 
			else 
			{
				spider = GameObject.FindGameObjectWithTag ("Spider");
				if (spider) 
				{
					SpiderController sCtrl = spider.GetComponent<SpiderController> ();

					if (CardColor.Equals ("red")) 
					{
						if (sCtrl.HasRedKeyCard) 
						{
							cr.SetAlpha (1.0f);
						} 
						else 
						{
							cr.SetAlpha (0);
						}
					}
					if (CardColor.Equals ("blue"))
					{
						if (sCtrl.HasBlueKeyCard) 
						{
							cr.SetAlpha (1.0f);
						} 
						else 
						{
							cr.SetAlpha (0);
						}
					}
				}
			}
		}
	}
}
