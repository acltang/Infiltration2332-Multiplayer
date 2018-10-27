using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{

    enum State
    {
        Open,
        Closed,
        Opening,
        Closing
    }

    State currentState;

    float speed = 25f;
    float distMoved = 0;
    float detectionRange = 30f;
    Vector3 originalPos;

    public string DoorType;
	public bool GuardsCanPass;
	GameObject player;
    GameObject mainCamera = null;

    Camera camera = null;

    AudioSource open = null;
    AudioSource close = null;

    // Use this for initialization
    void Start()
    {
        currentState = State.Closed;
        originalPos = transform.position;
		player = GameObject.FindGameObjectWithTag ("Player");
		camera = Camera.main;
        AudioSource[] stateSounds = GetComponents<AudioSource>();
        open = stateSounds[0];
        close = stateSounds[1];
    }

    // Update is called once per frame
    void Update()
    {
		if (player != null) 
		{
			switch (currentState)
			{
				case State.Closed:
					Closed ();
					break;
				case State.Open:
					Opened ();
					break;
				case State.Closing:
					Closing ();
					break;
				case State.Opening:
					Opening ();
					break;
			}
		}
    }

    public void Opening()
    {
        Renderer r = transform.GetComponent<Renderer>();
        float width = Mathf.Max(r.bounds.size.x, r.bounds.size.y);

        if (distMoved >= width)
        {
            currentState = State.Open;
        }

        Move(speed);
    }

    public void Closing()
    {
        if (distMoved <= 0)
        {
            currentState = State.Closed;
        }

        Move(-speed);
    }

    private void Opened()
    {
        float distance = Vector3.Distance(originalPos, player.transform.position);    
		bool stayOpen = false;
		
        if (!DoorType.Equals("spider") && distance < detectionRange)
        {
            stayOpen = true;
        }

        if (GameObject.Find("Spider(Clone)"))
        {
            GameObject spider = GameObject.FindGameObjectWithTag("Spider");
            float spiderDistance = Vector3.Distance(originalPos, spider.transform.position);

            if (DoorType.Equals("spider") && spiderDistance < detectionRange)
            {
                stayOpen = true;
            }
        }

		if (GuardsCanPass) 
		{
			GameObject[] guards = GameObject.FindGameObjectsWithTag ("Guard");
			for (int i = 0; i < guards.Length; i++) {
				float guardDistance = Vector3.Distance (originalPos, guards [i].transform.position);
				if (guardDistance < detectionRange) {
					stayOpen = true;
				}

			}
		}
		if(!stayOpen)
		{
            PlayCloseIfInCamera();
            currentState = State.Closing;
		}
    }

    private void Closed()
    {
        float distance = Vector3.Distance(originalPos, player.transform.position);
        if (DoorType.Equals("normal"))
        {
            if (distance <= detectionRange)
            {
                currentState = State.Opening;
                PlayOpenIfInCamera();
            }
        }
        if (DoorType.Equals("red"))
        {
            GameObject hero = GameObject.Find("Hero");
            HeroController hCtrl = hero.GetComponent<HeroController>();
            if (distance <= detectionRange && hCtrl.HasRedKeyCard)
            {
                currentState = State.Opening;
                PlayOpenIfInCamera();
            }
        }
        if (DoorType.Equals("blue"))
        {
            GameObject hero = GameObject.Find("Hero");
            HeroController hCtrl = hero.GetComponent<HeroController>();
            if (distance <= detectionRange && hCtrl.HasBlueKeyCard)
            {
                currentState = State.Opening;
                PlayOpenIfInCamera();
            }
        }

        if (GameObject.Find("Spider(Clone)"))
        {
            GameObject spider = GameObject.FindGameObjectWithTag("Spider");
            float spiderDistance = Vector3.Distance(originalPos, spider.transform.position);
            if (DoorType.Equals("spider"))
            {
                if (spiderDistance <= detectionRange)
                {
                    currentState = State.Opening;
                    PlayOpenIfInCamera();
                }
            }
        }

		if (GuardsCanPass)
		{
			//Guards can open doors
			GameObject[] guards = GameObject.FindGameObjectsWithTag ("Guard");
			for (int i = 0; i < guards.Length; i++) {
				float guardDistance = Vector3.Distance (originalPos, guards [i].transform.position);
				if (guardDistance < detectionRange) {
					currentState = State.Opening;
                    PlayOpenIfInCamera();
                }

			}
		}
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), collision.gameObject.GetComponent<Collider2D>());
        }
    }

    private void Move(float speed)
    {
        float moveDist = Time.deltaTime * speed;
        transform.position += transform.right * moveDist;
        distMoved += moveDist;
    }

    public void PlayOpenIfInCamera()
    {
        Vector3 screenPoint = camera.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        if (onScreen)
        {
            open.Play();
        }
    }

    public void PlayCloseIfInCamera()
    {
        Vector3 screenPoint = camera.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        if (onScreen)
        {
            close.Play();
        }
    }
}
