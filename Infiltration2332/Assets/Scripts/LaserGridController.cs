using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGridController : MonoBehaviour
{
    public IEnumerator off;
    public IEnumerator on;
    public bool active = true;

    public float offset;
    public float period;
    GameObject hero;
    GameObject[] guards;
    GameObject spider;
    GameObject mainCamera = null;

    Camera camera = null;

    AudioSource laser = null;
    AudioSource lose = null;
    public int laserSound = 0;

    bool LoadingInitiated = false;

    // Use this for initialization
    void Start()
    {
        off = Blink(1.5f);
        StartCoroutine(off);
        hero = GameObject.Find("Hero");
		camera = Camera.main;
        AudioSource[] lasers = GetComponents<AudioSource>();
        laser = lasers[laserSound];
        lose = lasers[4];
    }

    // Update is called once per frame
    void Update()
    {     
		if (hero != null)
		{
			guards = GameObject.FindGameObjectsWithTag ("Guard");
			spider = GameObject.Find ("Spider(Clone)");
			if (active)
			{
				IgnoreCollisionGuards (false);
				Physics2D.IgnoreCollision (GetComponent<Collider2D> (), hero.GetComponent<Collider2D> (), false);
				if (spider != null)
					Physics2D.IgnoreCollision (GetComponent<Collider2D> (), spider.GetComponent<Collider2D> (), false);
			} 
			else 
			{
				IgnoreCollisionGuards (true);
				Physics2D.IgnoreCollision (GetComponent<Collider2D> (), hero.GetComponent<Collider2D> ());
				if (spider != null)
					Physics2D.IgnoreCollision (GetComponent<Collider2D> (), spider.GetComponent<Collider2D> (), true);
			}
		}
    }

    public IEnumerator Blink(float waitTime)
    {
        yield return new WaitForSeconds(offset);
        while (true)
        {
            yield return new WaitForSeconds(waitTime + period);
            transform.GetComponent<Renderer>().enabled = false;
            active = false;
            yield return new WaitForSeconds(waitTime + period);
            transform.GetComponent<Renderer>().enabled = true;
            active = true;
            PlayAudioIfInCamera();
        }
    }

    public void PlayAudioIfInCamera()
    {
        Vector3 screenPoint = camera.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        if (onScreen)
        {
            laser.Play();
        }
    }

    private void IgnoreCollisionGuards(bool collision)
    {
        for (int i = 0; i < guards.Length; i++)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), guards[i].GetComponent<Collider2D>(), collision);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (active)
        {
            if (collision.gameObject.name == "Hero")
            {
                if (!LoadingInitiated)
                {
                    StartCoroutine(DelayedLoad());
                    LoadingInitiated = true;
                }
            }
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
