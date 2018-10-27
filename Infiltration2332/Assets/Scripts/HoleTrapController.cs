using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleTrapController : MonoBehaviour {

    GameObject hero = null;

    AudioSource holeDie = null;

    bool LoadingInitiated = false;

	public float HeroDist = 15.0f;

	enum State
	{
		Open,
		Closed
	}

	State currentState;

	// Use this for initialization
	void Start ()
	{
        hero = GameObject.Find("Hero");
        holeDie = GetComponent<AudioSource>();
		currentState = State.Closed;
		//GetComponent<Rigidbody2D> ().freezeRotation = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (hero != null) 
		{
			switch (currentState) 
			{
			case State.Open:
					SetTexture ("Textures/LaserHolder");
					GetComponent<BoxCollider2D> ().enabled = true;
					if (Vector3.Distance (transform.position, hero.transform.position) > HeroDist) 
					{
						currentState = State.Closed;
					}
					break;
			case State.Closed:
					SetTexture ("Textures/TrapDoorClose");
					GetComponent<BoxCollider2D> ().enabled = false;
					if (Vector3.Distance (transform.position, hero.transform.position) <= HeroDist) 
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
			if (collision.gameObject.name == "Hero")
			{
				if (!LoadingInitiated) 
				{
					StartCoroutine (DelayedLoad ());
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
        Destroy(hero);
        yield return new WaitForSeconds(holeDie.clip.length);
        WorldController.RestartLevel();
    }

	private void SetTexture(string tex)
	{
		SpriteRenderer r = this.GetComponent<SpriteRenderer>();
		r.sprite = Resources.Load(tex, typeof(Sprite)) as Sprite;
	}
}

