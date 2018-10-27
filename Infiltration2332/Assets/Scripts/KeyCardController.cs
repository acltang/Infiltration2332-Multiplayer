using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCardController : MonoBehaviour {

    public string CardColor;
	GameObject hero;
	GameObject spider;

    AudioSource getCard = null;
    bool audioPlayed = false;

	// Use this for initialization
	void Start ()
    {
		hero = GameObject.Find ("Hero");
        getCard = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (hero != null)
		{
			spider = GameObject.Find ("Spider(Clone)");
			if (!spider)
			{
				spider = GameObject.Find ("Spider");
			}
			Renderer r = transform.GetComponent<Renderer> ();
			float dist = Mathf.Min (r.bounds.size.x, r.bounds.size.y);

			CheckDistance (hero, dist, true);
			if (spider != null) 
			{
				CheckDistance (spider, dist, false);
			}
		}
	}

	private void CheckDistance(GameObject obj, float dist, bool controllerType)
	{
		if(Vector3.Distance(transform.position, obj.transform.position) <= dist && controllerType)
		{          
            HeroController hCtrl = obj.GetComponent<HeroController>();
			if(CardColor.Equals("red"))
			{
				hCtrl.HasRedKeyCard = true;
			}
			if (CardColor.Equals("blue"))
			{
                hCtrl.HasBlueKeyCard = true;
			}
            hCtrl.PlayKeyCardAudio();
            Destroy(this.gameObject);
        }

		if(Vector3.Distance(transform.position, obj.transform.position) <= dist && !controllerType && 
			!spider.GetComponent<SpiderController>().destroyFlag)
		{
            SpiderController sCtrl = obj.GetComponent<SpiderController>();
			if(CardColor.Equals("red"))
			{            
                sCtrl.HasRedKeyCard = true;
			}
			if (CardColor.Equals("blue"))
			{
                sCtrl.HasBlueKeyCard = true;
			}
            sCtrl.PlayKeyCardAudio();
            Destroy(this.gameObject);
        }
	}
}
