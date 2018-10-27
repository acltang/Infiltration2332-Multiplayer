using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour {

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
			Vector3 move = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0);
			Vector3 position = transform.position;
			GetComponent<Rigidbody2D> ().MovePosition (transform.position + speed * move * Time.deltaTime);
			
			RotatePlayer (move);
			
			SendPosition(move, position);
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
	
	
	// TODO: Move into ConnectionManager?
	ulong frames = 0;
	public void SendPosition(Vector3 move, Vector3 position)
	{
		Vector3 noMovement = new Vector3(0, 0, 0);
		frames++;
		if (frames % 1 == 0 && position != noMovement && move != noMovement)
		{
			ConnectionManager gameConnection = GameObject.Find ("Game Connection").GetComponent<ConnectionManager> ();
			if (gameConnection.isConnected())
			{
				PlayerMoveMessage msg = new PlayerMoveMessage();
				msg.move = move;
				msg.position = position;
				msg.time = Time.time;
				
				gameConnection.sendJSON(msg);
			}
		}
	}
}
