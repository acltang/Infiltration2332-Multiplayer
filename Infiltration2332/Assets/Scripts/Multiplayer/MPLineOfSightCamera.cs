using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This camera handles drawing the Los of the guards and security cameras
public class MPLineOfSightCamera : MonoBehaviour 
{
	public GameObject player;
	public Material mat;
	private Vector3 offset;
	ConnectionManager gameConnection;

	// Use this for initialization
	void Start () 
	{
		gameConnection = GameObject.Find("Game Connection").GetComponent<ConnectionManager>();
        //player = GameObject.FindGameObjectWithTag ("Player");
        //offset = transform.position - player.transform.position;

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

        Camera cam = GetComponent<Camera> ();
		cam.orthographicSize = Camera.main.orthographicSize;
		cam.depth = Camera.main.depth - 1;
	}

	void LateUpdate()
	{
        if (player != null)
        {
            // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
			//player = GameObject.FindGameObjectWithTag ("Player");
            transform.position = player.transform.position + offset;
        }
	}
    /*
	//GL Draw calls made here
	void OnPostRender()
	{
		GameObject[] guards = GameObject.FindGameObjectsWithTag ("Guard");
		GameObject[] cameras = GameObject.FindGameObjectsWithTag ("Camera");

		for (int i = 0; i < guards.Length; i++)
		{
			LineOfSight los = guards [i].GetComponent<LineOfSight> ();
			DrawLos (guards[i].transform.position, los.endPoints, los.color);
		}

		for (int i = 0; i < cameras.Length; i++)
		{
			LineOfSight los = cameras [i].GetComponent<LineOfSight> ();
			DrawLos (cameras[i].transform.position, los.endPoints, los.color);
		}
	}

	private void DrawLos(Vector3 pos, Vector3[] endPoints, Color color)
	{
		for (int i = 0; i < (endPoints.Length - 1); i++) 
		{
			DrawTriangle (pos, endPoints [i], endPoints [i + 1], color);
		}
	}

	private void DrawTriangle(Vector3 pt1, Vector3 pt2, Vector3 pt3, Color color)
	{
		color.a = 0.5f;
		if (!mat) 
		{
			Debug.LogError("Please Assign a material on the inspector");
			return;
		}
		GL.PushMatrix();
		mat.SetPass(0);
		GL.Begin(GL.TRIANGLES);
		GL.Color (color);
		GL.Vertex3(pt1.x, pt1.y, 0);
		GL.Vertex3(pt2.x, pt2.y, 0);
		GL.Vertex3(pt3.x, pt3.y, 0);
		GL.End();
		GL.PopMatrix();
	}
		*/
}
