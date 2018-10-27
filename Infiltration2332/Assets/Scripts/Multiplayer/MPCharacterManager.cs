using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPCharacterManager : MonoBehaviour {

    GameObject hero;
    GameObject dummy;
    static GameObject LosCamera;
    ConnectionManager gameConnection;

	// Use this for initialization
	void Start () {
        hero = GameObject.Find("Hero(Clone)");
        dummy = GameObject.Find("Dummy(Clone)");
        LosCamera = GameObject.Find("LOSCamera");
        gameConnection = GameObject.Find("Game Connection").GetComponent<ConnectionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameConnection.isConnected())
        {
            if (!hero)
            {
                if (gameConnection.getPlayerColor() == "grey") // P1
                {
                    Vector3 heroPosition = (GameObject.Find("GreySpawn").transform.position);
                    hero = (GameObject)Instantiate(Resources.Load("Prefabs/Hero"), heroPosition, Quaternion.identity);
                }
                else // P2
                {
                    Vector3 heroPosition = (GameObject.Find("RedSpawn").transform.position);
                    hero = (GameObject)Instantiate(Resources.Load("Prefabs/Hero"), heroPosition, Quaternion.identity);
                    hero.GetComponent<SpriteRenderer>().sprite = Resources.Load("Textures/Guard(1)", typeof(Sprite)) as Sprite;
                }

                SetCameraPlayer(hero);
            }

            if (!dummy)
            {
                if (gameConnection.getPlayerColor() == "grey") // P1
                {
                    Vector3 dummyPosition = (GameObject.Find("RedSpawn").transform.position);
                    dummy = (GameObject)Instantiate(Resources.Load("Prefabs/Multiplayer/Dummy"), dummyPosition, Quaternion.identity);
                    dummy.GetComponent<SpriteRenderer>().sprite = Resources.Load("Textures/Guard(1)", typeof(Sprite)) as Sprite;
                }
                else // P2
                {
                    Vector3 dummyPosition = (GameObject.Find("GreySpawn").transform.position);
                    dummy = (GameObject)Instantiate(Resources.Load("Prefabs/Multiplayer/Dummy"), dummyPosition, Quaternion.identity);
                }
            }
        }
    }

    public static void SetCameraPlayer(GameObject player)
    {
        Camera.main.GetComponent<MPMainCameraController>().player = player;
        LosCamera.GetComponent<MPLineOfSightCamera>().player = player;
    }
}
