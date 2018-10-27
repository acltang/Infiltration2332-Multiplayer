using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    enum State
    {
        Normal,
        Alert,
        Disabled
    }
    State currentState;

	public float rotationPeriod = 2.0f;
    float currentRotation = 0;
    bool rotateRight = true;
    Quaternion originalRotation;
	LineOfSight los;
    float disabledTimer = 5.0f;
	public float RotationMax;
	GameObject hero;
	float sendDelay = 0;
	public float callDistance;

    AudioSource disabledSound = null;

    // Use this for initialization
    void Start()
    {
        currentState = State.Normal;
        originalRotation = transform.rotation;
		los = GetComponent<LineOfSight> ();
		los.detectionRange = 45.0f;
		hero = GameObject.Find ("Hero");
        disabledSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Normal:
                HandleNormal();
                break;

            case State.Alert:
                HandleAlert();
                break;
            case State.Disabled:
                HandleDisabled();
                break;
        }
		los.SightTest ();
    }

    public void HandleNormal()
    {
		los.color = Color.yellow;
		los.detectionRange = 45.0f;
		if (los.playerInLos) 
		{
			currentState = State.Alert;
		}
        float rotation = 0;
		if (currentRotation >= RotationMax)
        {
            rotateRight = false;
        }
		if (currentRotation <= -1 * RotationMax)
        {
            rotateRight = true;
        }

        //Rotate back and forth rotationAngle degrees
        if (rotateRight)
        {
			rotation = (1 / rotationPeriod) * RotationMax * Time.deltaTime;
        }
        else
        {
			rotation = (-1 / rotationPeriod) * RotationMax * Time.deltaTime;
        }
        transform.Rotate(Vector3.forward, rotation);
        currentRotation += rotation;

		if (los.playerInLos)
        {
            currentState = State.Alert;
        }

    }

    public void HandleAlert()
    {
        los.color = Color.red;

        //Turn toward player
		if (!hero) 
		{
			return;
		}
        Vector3 dir = hero.transform.position - transform.position;
        dir.Normalize();
        float theta = Mathf.Rad2Deg * (float)Mathf.Acos((Vector3.Dot(transform.right, dir)));
        if (theta > 0.001f)
        {
            Vector3 sign = Vector3.Cross(transform.right, dir);
            transform.Rotate(Vector3.forward, Mathf.Sign(sign.z) * theta * 0.07f);
        }

        //Alert all guards
		if (sendDelay <= 0) 
		{
			GameObject[] guards = GameObject.FindGameObjectsWithTag ("Guard");
			for (int i = 0; i < guards.Length; i++) 
			{
				GuardController guard = guards [i].GetComponent<GuardController> ();
				if (Vector3.Distance (transform.position, guard.transform.position) <= callDistance) 
				{
					guard.SetTarget (hero.transform.position);
					guard.SetState ("Chasing");
				}
			}
			sendDelay = 0.5f;
		}
		sendDelay -= Time.deltaTime;

		if (!los.playerInLos)
        {
            currentState = State.Normal;
            transform.rotation = originalRotation;
			sendDelay = 0.5f;
        }
    }

    public void HandleDisabled()
    {      
        disabledTimer -= Time.deltaTime;
		los.detectionRange = 0;
        if (disabledTimer <= 0)
        {
            disabledSound.Pause();
            currentState = State.Normal;
            disabledTimer = 10.0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Spider")
        {
            disabledSound.Play();
            currentState = State.Disabled;
			collision.gameObject.GetComponent<SpiderController> ().DestroySpider ();
        }
    }
}