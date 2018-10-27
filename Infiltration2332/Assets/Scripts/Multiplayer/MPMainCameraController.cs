using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPMainCameraController : MonoBehaviour {
    public GameObject player;       //Public variable to store a reference to the player game object

    private Vector3 offset;         //Private variable to store the offset distance between the player and camera

	ConnectionManager gameConnection;

    // Use this for initialization
    void Start()
    {
		gameConnection = GameObject.Find("Game Connection").GetComponent<ConnectionManager>();
		//player = GameObject.FindGameObjectWithTag ("Player");

        if (gameConnection.getPlayerColor() == "grey") // P1
        {
            transform.position = new Vector3(GameObject.Find("GreySpawn").transform.position.x, 
                                             GameObject.Find("GreySpawn").transform.position.y, -10f);
            
            offset = transform.position - (GameObject.Find("GreySpawn").transform.position);
        }
        else // P2
        {
            transform.position = new Vector3(GameObject.Find("RedSpawn").transform.position.x, 
                                             GameObject.Find("RedSpawn").transform.position.y, -10f);
                                             
            offset = transform.position - (GameObject.Find("RedSpawn").transform.position);
        }

        /*
        if (player != null)
        {
            //Calculate and store the offset value by getting the distance between the player's position and camera's position.
            offset = transform.position - player.transform.position;
        }*/
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        if (player != null)
        {
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
            //player = GameObject.FindGameObjectWithTag ("Player");
            transform.position = player.transform.position + offset;
        }
    }
}
