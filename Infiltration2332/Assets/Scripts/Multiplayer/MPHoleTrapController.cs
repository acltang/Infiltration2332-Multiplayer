using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPHoleTrapController : MonoBehaviour {

    GameObject hero = null;
    GameObject dummy = null;
    AudioSource holeDie = null;
    bool LoadingInitiated = false;
	public float HeroDist = 10.0f;
    ConnectionManager gameConnection;

	enum State
	{
		Open,
		Closed
	}

	State currentState;

	// Use this for initialization
	void Start ()
	{
       // hero = GameObject.Find("Hero");
        holeDie = GetComponent<AudioSource>();
		currentState = State.Closed;
        //GetComponent<Rigidbody2D> ().freezeRotation = true;
        gameConnection = GameObject.Find("Game Connection").GetComponent<ConnectionManager>();
    }
	
	// Update is called once per frame
	void Update () 
	{
        hero = GameObject.Find("Hero(Clone)");
        dummy = GameObject.Find("Dummy(Clone)");
        LoadingInitiated = false;
        if (hero != null && dummy != null) 
		{
			switch (currentState) 
			{
			case State.Open:
					SetTexture ("Textures/LaserHolder");
					GetComponent<BoxCollider2D> ().enabled = true;
					if (Vector3.Distance (transform.position, hero.transform.position) > HeroDist &&
                        Vector3.Distance(transform.position, dummy.transform.position) > HeroDist) 
					{
						currentState = State.Closed;
					}
					break;
			case State.Closed:
					SetTexture ("Textures/TrapDoorClose");
					GetComponent<BoxCollider2D> ().enabled = false;
					if (Vector3.Distance (transform.position, hero.transform.position) <= HeroDist ||
                        Vector3.Distance(transform.position, dummy.transform.position) <= HeroDist) 
					{
						currentState = State.Open;
					}
					break;
			}
		}
	}


    void OnCollisionEnter2D(Collision2D collision)
    {
		if (currentState == State.Open)
		{
			if (collision.gameObject.name == "Hero(Clone)")
			{
				if (!LoadingInitiated) 
				{
                    StartCoroutine (DelayedLoad ());
					LoadingInitiated = true;
				}
			}
            if (collision.gameObject.name == "Dummy(Clone)")
            {
                if (!LoadingInitiated)
                {
                    StartCoroutine(DelayedLoadDummy());
                    LoadingInitiated = true;
                }
            }
            if (collision.gameObject.name == "Spider(Clone)") 
			{
				collision.gameObject.GetComponent<SpiderController> ().DestroySpider ();
				holeDie.Play ();
			}
			if (collision.gameObject.tag == "Guard") 
			{
				holeDie.Play ();
			}
			//GetComponent<Rigidbody2D> ().velocity = new Vector2 (0, 0);
		}
    }

    private IEnumerator DelayedLoad()
    {
        holeDie.Play();
        hero.GetComponent<Renderer>().enabled = false;
        hero.GetComponent<HeroController>().EnableMovement = false;

        yield return new WaitForSeconds(holeDie.clip.length);

        if (gameConnection.getPlayerColor() == "grey") {
            hero.gameObject.transform.position = (GameObject.Find("GreySpawn").transform.position);
        }
        else {
            hero.gameObject.transform.position = (GameObject.Find("RedSpawn").transform.position);
        }

        hero.GetComponent<Renderer>().enabled = true;
        hero.GetComponent<HeroController>().EnableMovement = true;
    }

    private IEnumerator DelayedLoadDummy()
    {
        holeDie.Play();
        dummy.GetComponent<Renderer>().enabled = false;

        yield return new WaitForSeconds(holeDie.clip.length);

        if (gameConnection.getPlayerColor() == "grey") {
            dummy.gameObject.transform.position = (GameObject.Find("RedSpawn").transform.position);
        }
        else {
            dummy.gameObject.transform.position = (GameObject.Find("GreySpawn").transform.position);
        }

        dummy.GetComponent<Renderer>().enabled = true;
    }

	private void SetTexture(string tex)
	{
		SpriteRenderer r = this.GetComponent<SpriteRenderer>();
		r.sprite = Resources.Load(tex, typeof(Sprite)) as Sprite;
	}
}

