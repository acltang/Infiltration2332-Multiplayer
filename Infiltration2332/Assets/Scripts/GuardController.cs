using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardController : MonoBehaviour {

    enum State
    {
        Patrol,
        Alert,
        Chasing, 
        Return
    }

    State currentState;

	LineOfSight los;
    public float alertTimer = 10.0f;
    public float patrolSpeed = 15.0f;
    public float alertSpeed = 35.0f;
	public float returnSpeed = 15.0f;
    private Vector3 nextPos;
    float rotationSpeed = 90.0f;
    Vector3 startPos;
    Quaternion originalRotation;
	Queue<Vector3> path;
	float ErrorDistance = 3.0f;

    AudioSource alert = null;
    AudioSource lose = null;
    bool alertSoundPlayed = false;

    GameObject hero = null;
    bool LoadingInitiated = false;

	float chasingTimer = 2.0f;
	Vector3 lastPos;

    // Use this for initialization
    void Start()
    {
        hero = GameObject.Find("Hero");
        currentState = State.Patrol;
        startPos = transform.position;
		originalRotation = new Quaternion(transform.rotation.x, transform.rotation.y,
										transform.rotation.z, transform.rotation.w);
		los = GetComponent<LineOfSight> ();
		path = null;
        AudioSource[] audios = GetComponents<AudioSource>();
        alert = audios[0];
        lose = audios[1];
		GetComponent<Rigidbody2D> ().freezeRotation = true;
		lastPos = transform.position;
    }



    // Update is called once per frame
    void FixedUpdate()
    {
		los.SightTest();
        switch (currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;

            case State.Alert:
                HandleAlert();
                break;

            case State.Chasing:
                HandleChasing();
                break;

            case State.Return:
                HandleReturn();
                break;
        }
		if (los.playerInLos) 
		{
            if (!alertSoundPlayed)
            {
                alert.Play();
            }
            alertSoundPlayed = true;

            nextPos = GameObject.Find ("Hero").transform.position;       
			currentState = State.Chasing;
			path = null;
		}
		lastPos = transform.position;
    }		

    public void HandlePatrol()
	{        
        los.detectionRange = 30.0f;
		los.color = Color.yellow;
        Move(patrolSpeed);
    }

    public void HandleAlert()
    {
        los.color = Color.magenta;
        if(los.playerInLos)
        {
            alertTimer = 10.0f;
            currentState = State.Chasing;
        }
        if (alertTimer <= 0)
        {
            currentState = State.Return;
            nextPos = startPos;
            alertTimer = 10.0f;
        }
        alertTimer -= Time.deltaTime;
        float angle = rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, angle);
    }

    public void HandleChasing()
    {
        los.detectionRange = 60.0f;
        los.color = Color.red;

		alertTimer = 10.0f;

        if(NeedPath())
        {
            GetPath();
        }
		Vector3 moveDist = (transform.position - lastPos);
		bool hasMoved = moveDist.magnitude > 0.5f;
		Debug.Log (moveDist);
		if (!hasMoved) 
		{
			chasingTimer -= Time.deltaTime;
			if (chasingTimer <= 0) 
			{
				alertSoundPlayed = false;
				currentState = State.Alert;
				path = null;
				ErrorDistance = 3.0f;
				chasingTimer = 2.0f;
			}
		}
		if (!los.playerInLos && (Vector3.Distance(transform.position, nextPos) < ErrorDistance))
        {
            if(path == null || path.Count == 0)
            {
                alertSoundPlayed = false;
                currentState = State.Alert;
                path = null;
				ErrorDistance = 3.0f;
            }
            else
            {
                nextPos = path.Dequeue();
            }
        }


        RotateToNext();
        Move(alertSpeed);
    }

    public void HandleReturn()
    {
        if (NeedPath())
        {
            GetPath();
        }
		bool done = false;
        los.detectionRange = 40.0f;
        los.color = Color.yellow;
        if (Vector3.Distance(nextPos, transform.position) < 3f)
        {
            if (path == null || path.Count == 0)
            {
				path = null;
                currentState = State.Patrol;
				transform.SetPositionAndRotation (startPos, originalRotation);
				done = true;
            }
            else
            {
                nextPos = path.Dequeue();
            }
        }

		if (!done)
		{
			RotateToNext ();
			Move (returnSpeed);
		}
    }

    private void Move(float speed)
    {
		GetComponent<Rigidbody2D> ().MovePosition (transform.position + transform.right * Time.deltaTime * speed);
    }

    public void SetState(string state)
    {
        if(state.Equals("Chasing"))
        {
            currentState = State.Chasing;
        }
        else if(state.Equals("Alert"))
        {
            currentState = State.Alert;
        }
        else if(state.Equals("Patrol"))
        {
            currentState = State.Patrol;
        }
        else
        {
            currentState = State.Return;
        }
    }

    private void RotateToNext()
    {
        Vector3 dir = nextPos - transform.position;

        dir.Normalize();
        float theta = Mathf.Rad2Deg * (float)Mathf.Acos((Vector3.Dot(transform.right, dir)));
        if (theta > 0.001f)
        {
            Vector3 sign = Vector3.Cross(transform.right, dir);
            transform.Rotate(Vector3.forward, Mathf.Sign(sign.z) * theta * 0.3f);
        }
    }

    private bool NeedPath(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dir.magnitude);
		if(hit.collider != null && hit.collider.tag != "Player" && hit.collider.tag != "Spider"
			&& (path == null || path.Count == 0) )
        {
            return true;
        }
        return false;
    }

    private void GetPath(Vector3 pos)
    {
        GameObject AStar = GameObject.Find("A_");
        Pathfinding p = AStar.GetComponent<Pathfinding>();
        p.FindPath(transform.position, pos);
        path = AStar.GetComponent<Grid>().GetPath();
        nextPos = path.Dequeue();
    }

    private bool NeedPath()
    {
        return NeedPath(nextPos);
    }

    private void GetPath()
    {
        GetPath(nextPos);
    }

    public void SetTarget(Vector3 pos)
    {
		path = null;
		if (NeedPath (pos)) 
		{
			GetPath (pos);
		}
		else 
		{
			nextPos = pos;
		}
    }

	void OnCollisionEnter2D(Collision2D collision)
	{
		GetComponent<Rigidbody2D> ().velocity = new Vector2 (0, 0);
		if (collision.gameObject.tag == "Player") 
		{
            if (!LoadingInitiated)
            {
                StartCoroutine(DelayedLoad());
                LoadingInitiated = true;
            }
        }
		if (collision.gameObject.tag != "Player" && collision.gameObject.tag != "Spider" && currentState == State.Patrol) 
		{
			transform.Rotate (Vector3.forward, 180);
		}
		if (collision.gameObject.tag == "Hazard")
		{
			GameObject.Destroy (this.gameObject);
		}
		if (collision.gameObject.tag == "Guard" && currentState == State.Chasing) 
		{
			ErrorDistance = 15.0f;
		}
			
	}

    private IEnumerator DelayedLoad()
    {
        lose.Play();
        Destroy(hero);
        yield return new WaitForSeconds(lose.clip.length);
        WorldController.RestartLevel();
    }
}


