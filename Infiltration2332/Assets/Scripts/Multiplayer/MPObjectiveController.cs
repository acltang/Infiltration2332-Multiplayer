using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPObjectiveController : MonoBehaviour {

	GameObject hero;
	GameObject dummy;
	public string nextScene;

    AudioSource win = null;

    bool LoadingInitiated = false;

	// Use this for initialization
	void Start () 
	{
		//hero = GameObject.Find ("Hero");
        win = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        hero = GameObject.Find("Hero(Clone)");
		dummy = GameObject.Find("Dummy(Clone)");
        if (hero != null) 
		{
			if (Vector3.Distance (hero.transform.position, transform.position) <
			   hero.GetComponent<CircleCollider2D> ().radius * 2 &&
			   Vector3.Distance (dummy.transform.position, transform.position) <
			   dummy.GetComponent<CircleCollider2D> ().radius * 2) {
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

		//imitate hero and dummy destroy
        hero.GetComponent<Renderer>().enabled = false;
        hero.GetComponent<HeroController>().EnableMovement = false;
		dummy.GetComponent<Renderer>().enabled = false;

		//clear keycards obtained in this level
		hero.GetComponent<HeroController>().HasRedKeyCard = false;
		hero.GetComponent<HeroController>().HasBlueKeyCard = false;

		dummy.GetComponent<DummyController>().HasRedKeyCard = false;
		dummy.GetComponent<DummyController>().HasBlueKeyCard = false;

		//go to next level
        yield return new WaitForSeconds(win.clip.length);     
        //WorldController.ChangeScene(nextScene);
		ConnectionManager gameConnection = GameObject.Find("Game Connection").GetComponent<ConnectionManager>();
		gameConnection.sendLoadLevelMessage(nextScene);

		//reenable hero and dummy
		hero.GetComponent<Renderer>().enabled = true;
        hero.GetComponent<HeroController>().EnableMovement = true;
		dummy.GetComponent<Renderer>().enabled = true;
    }
}
