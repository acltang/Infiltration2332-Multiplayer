using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonoBehaviour {

    public float speed = 35.0f;

    public bool HasRedKeyCard = false;
    public bool HasBlueKeyCard = false;
    public bool EnableMovement = true;
	bool SpaceUp = false;

    AudioSource getCard = null;
    AudioSource spiderDie = null;


    // Use this for initialization
    void Start ()
    {
        AudioSource[] audios = GetComponents<AudioSource>();
        getCard = audios[0];
        spiderDie = audios[1];
		GetComponent<Rigidbody2D> ().freezeRotation = true;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
		if (EnableMovement) 
		{
			//string json_string_msg = player.gameObject.GetComponent<HeroController>().
			Vector3 move = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0);
			GetComponent<Rigidbody2D> ().MovePosition (transform.position + speed * move * Time.deltaTime);		
			
			RotatePlayer (move);
		} 
		else 
		{
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (0, 0);
		}

    }

	private void RotatePlayer(Vector3 move)
	{
		//Rotation Code
		if (move.y == 0) 
		{
			if (move.x < 0) 
			{
				transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 180));
			}
			if (move.x > 0) 
			{
				transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));		
			}
		} 
		else 
		{
			float angle = Mathf.Rad2Deg * Mathf.Atan (move.y / move.x);
			if (move.x < 0) 
			{
				angle += 180f;
			}
			transform.rotation = Quaternion.Euler (new Vector3 (0, 0, angle));
		}
	}

    public void PlayKeyCardAudio()
    {
        getCard.Play();
    }

    public void PlaySpiderDieAudio()
    {
        spiderDie.Play();
    }
	
	void OnLevelWasLoaded()
	{
		// If a player dies and the scene gets reset, 
		// set prevTimeSet so that it's not used to calculate movement distance in the restarted level.
		// (The delay while reseting the level might mess something up.)
		prevTimeSet = false;
	}
	
	// TODO
	float prevTime; // Can't initialize this, have no idea what the value would be.
	bool prevTimeSet = false; // ugly solution. Only false on first update.
	public void ReceivePosition(Vector3 newMove, Vector3 newPosition, float curTime)
	{
		// Drop the 1st message and use it to initialize prevTime. It's a safer solution.
		if (!prevTimeSet) { prevTime = curTime; prevTimeSet = true; return; } 
		
		float messageDeltaTime = curTime - prevTime;
		prevTime = curTime;
		GetComponent<Rigidbody2D> ().MovePosition (newPosition + speed * newMove * messageDeltaTime);
		//GetComponent<Rigidbody2D> ().MovePosition (newPosition);
		RotatePlayer (newMove);
	}
}
