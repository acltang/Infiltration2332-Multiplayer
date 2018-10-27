using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveController : MonoBehaviour {

	GameObject hero;
	public string nextScene;

    AudioSource win = null;

    bool LoadingInitiated = false;

	// Use this for initialization
	void Start () 
	{
		hero = GameObject.Find ("Hero");
        win = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (hero != null) 
		{
			if (Vector3.Distance (hero.transform.position, transform.position) <
			   hero.GetComponent<CircleCollider2D> ().radius * 2) {
				if (!LoadingInitiated) {
					StartCoroutine (DelayedLoad ());
					LoadingInitiated = true;
				}
			}
		}
	}

    private IEnumerator DelayedLoad()
    {
        win.Play();
        Destroy(hero);
        yield return new WaitForSeconds(win.clip.length);     
        WorldController.ChangeScene(nextScene);
    }
}
