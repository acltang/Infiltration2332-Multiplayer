/*
 * CharacterManager.cs
 * 
 * Handles switching between hero and spider which are both playable characters
 * 
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
	GameObject Hero;
	GameObject spider;
	static GameObject LosCamera;

	bool SpaceUp = false;
	public bool switchToSpider = false;
	public bool switchToPlayer = false;

	float distance = 0;
	float transitionStep = 0;
	float originalDistance = 0;

	float thresholdDistance = 5.0f;

    AudioSource charSwitch = null;

	void Start()
	{
		Hero = GameObject.Find ("Hero");
		spider = GameObject.Find("Spider");
		LosCamera = GameObject.Find ("LOSCamera");
        AudioSource audio = GetComponent<AudioSource>();
        charSwitch = audio;
    }

	void FixedUpdate()
	{
		if (switchToPlayer) 
		{
			SwitchToPlayer ();
		} 
		else if (switchToSpider) 
		{
			SwitchToSpider ();
		} 
		else 
		{
			if (SpaceUp && Input.GetKeyDown (KeyCode.Space))
			{
                charSwitch.Play();
				//Switch 
				if (Hero.GetComponent<HeroController> ().EnableMovement) 
				{
					switchToSpider = true;
					if (!spider)
					{
						Vector3 spiderPosition = Hero.transform.position + (Hero.transform.right) * 4.0f;
						spider = (GameObject)Instantiate (Resources.Load ("Prefabs/Spider"),
							spiderPosition, Quaternion.identity);
						Hero.GetComponent<HeroController> ().EnableMovement = false;
						SetCameraPlayer (spider);
						spider.GetComponent<SpiderController> ().EnableMovement = true;
						switchToSpider = false;
					} 
				}
				else if (spider && spider.GetComponent<SpiderController> ().EnableMovement) 
				{
					switchToPlayer = true;
				}
			}
		}
		if (spider && Hero) 
		{
			if (switchToPlayer) 
			{
				distance = Vector3.Distance (Camera.main.transform.position, Hero.transform.position);
			}
			if (switchToSpider)
			{
				distance = Vector3.Distance (Camera.main.transform.position, spider.transform.position);
			} 
			originalDistance = Vector3.Distance (spider.transform.position, Hero.transform.position);;
		}
		SpaceUp = !Input.GetKey(KeyCode.Space);
	}

	private void SwitchToSpider()
	{
		if (spider) 
		{
			Hero.GetComponent<HeroController> ().EnableMovement = false;
			SetCameraPlayer (null); //So cameras no longer follow player character

			//Calculate how much to move Cameras this update
			MoveStep(spider);

			//If Camera is close enough to spider's location switch control
			if(Vector3.Distance(spider.transform.position, Camera.main.transform.position) < thresholdDistance)
			{
				SetCameraPlayer (spider);
				spider.GetComponent<SpiderController> ().EnableMovement = true;
				switchToSpider = false;
			}
		}
	}

	private void SwitchToPlayer()
	{
		if (Hero) 
		{
			spider.GetComponent<SpiderController> ().EnableMovement = false;
			SetCameraPlayer (null); //So cameras no longer follow player character

			//Calculate how much to move Cameras this update
			MoveStep(Hero);

			//If Camera is close enough to spider's location switch control
			if(Vector3.Distance(Hero.transform.position, Camera.main.transform.position) < thresholdDistance)
			{
				SetCameraPlayer (Hero);
				Hero.GetComponent<HeroController> ().EnableMovement = true;
				switchToPlayer = false;
				if (spider.GetComponent<SpiderController> ().destroyFlag) 
				{
					Destroy (spider);
					spider = null;
				}
			}
		}
	}

	private void MoveStep(GameObject player)
	{
		float speedScale = distance / originalDistance;
		if (speedScale < 0.9f)
		{
			speedScale = 0.9f;
		}
		transitionStep = 3 * distance * speedScale * Time.fixedDeltaTime;
		MoveCamera (player);
	}

	public void MoveCamera(GameObject player)
	{
		Camera.main.transform.position = Vector3.MoveTowards (Camera.main.transform.position, 
			player.transform.position, transitionStep);
		LosCamera.transform.position = Vector3.MoveTowards (LosCamera.transform.position, 
			player.transform.position, transitionStep);
		
	}

	public static void SetCameraPlayer(GameObject player)
	{
		Camera.main.GetComponent<MainCameraController> ().player = player;
		LosCamera.GetComponent<LineOfSightCamera> ().player = player;
	}
}

