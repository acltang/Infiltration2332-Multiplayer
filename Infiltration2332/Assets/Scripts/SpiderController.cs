using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
	public bool HasRedKeyCard = false;
	public bool HasBlueKeyCard = false;
    float speed = 35.0f;
    float detectionRange = 100f;
    public bool EnableMovement = true;
	public float spawnTimer;

	public GameObject player;
    AudioSource getCard = null;
    AudioSource ping = null;

	public bool destroyFlag = false;
	public bool pingEnabled = true;

    // Use this for initialization
    void Start()
    {
		spawnTimer = 0.5f;
		player = GameObject.FindGameObjectWithTag ("Player");
        AudioSource[] audios = GetComponents<AudioSource>();
        getCard = audios[0];
        ping = audios[1];
		GetComponent<Rigidbody2D> ().freezeRotation = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		spawnTimer -= Time.deltaTime;
		if (EnableMovement) 
		{
			Vector3 move = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0);
			GetComponent<Rigidbody2D> ().MovePosition (transform.position + speed * move * Time.deltaTime);

			RotatePlayer (move);

			if (Input.GetKeyDown (KeyCode.F) && pingEnabled)
			{
				ping.Play ();
				Vector3 location = transform.position;

				GameObject[] guards = GameObject.FindGameObjectsWithTag ("Guard");
				for (int i = 0; i < guards.Length; i++) 
				{     
					GuardController guard = guards [i].GetComponent<GuardController> ();
					float distance = Vector3.Distance (guard.transform.position, location);
					if (distance <= detectionRange)
					{
						guard.SetTarget (location);
						guard.SetState ("Chasing");
					}
				}
				DestroySpider ();
			}
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
		
    void OnCollisionEnter2D(Collision2D collision)
    {
		if(collision.gameObject.tag == "Guard" || collision.gameObject.tag == "Hazard")
        {
            player.gameObject.GetComponent<HeroController>().PlaySpiderDieAudio();
            DestroySpider ();
        }
		if (collision.gameObject.tag == "Player" && spawnTimer <= 0 && player)
		{
			if (HasRedKeyCard) 
			{
				collision.gameObject.GetComponent<HeroController> ().HasRedKeyCard = true;
			}

			if (HasBlueKeyCard) 
			{
				collision.gameObject.GetComponent<HeroController> ().HasBlueKeyCard = true;
			}
			//Change camera to hero	
			player.gameObject.GetComponent<HeroController> ().EnableMovement = true;
			CharacterManager.SetCameraPlayer (player);

		
			Destroy (this.gameObject);
		}
    }

	public void DestroySpider()
	{ 
		//Drop any keycards
		if (HasRedKeyCard)
		{
			GameObject.Instantiate (Resources.Load ("Prefabs/RedKeyCard"), transform.position, Quaternion.identity);
		}
		if (HasBlueKeyCard)
		{
			GameObject.Instantiate (Resources.Load ("Prefabs/BlueKeyCard"), transform.position, Quaternion.identity);
		}
		transform.GetComponent<Renderer>().enabled = false;
		//Switch control to hero now
		destroyFlag = true;
		GameObject.Find ("HUD").GetComponent<CharacterManager> ().switchToPlayer = true;
	}

    public void PlayKeyCardAudio()
    {
        getCard.Play();
    }
}
