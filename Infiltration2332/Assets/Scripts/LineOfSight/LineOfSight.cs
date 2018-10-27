using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
	public Color color; //Color for GL drawing

	public float detectionRange = 40.0f;
	public bool playerInLos = false;
	float sightAngle = 90f;
	float rayResolution = 1f; //number of rays cast per degree
	public Vector3[] endPoints; //endpoints of all rays cast

	void Start()
	{
		endPoints = new Vector3[Mathf.RoundToInt(sightAngle * rayResolution)];
	}

	//Checks if player is in los and also draws the los visualization
	public void SightTest()
	{
		int numRays = Mathf.RoundToInt(sightAngle * rayResolution);
		Vector3 dir = RotateVector(transform.right, sightAngle / 2);

		bool playerSeen = false;
		for (int i = 0; i < numRays; i++)
		{
			RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, detectionRange);
			float distance = detectionRange;

			if (hit.collider != null && hit.collider.tag != "Hazard" && hit.collider.tag != "Spider")
			{
				distance = Vector2.Distance(hit.point, transform.position);
				if (hit.transform.tag == "Player")
				{ 
					playerSeen = true;
				}
			}

			if (hit.collider != null && hit.collider.tag == "Hazard") 
			{
				RaycastHit2D[] hits = Physics2D.RaycastAll (transform.position, dir, detectionRange);
				for(int j = 0; j < hits.Length; j++)
				{
					if (hits [j].collider.tag == "Player")
					{
						distance = Vector2.Distance (hits [j].point, transform.position);
						playerSeen = true;
					}
				}
			}

			endPoints [i] = transform.position + dir * distance;
			dir = RotateVector(dir, -1f * (sightAngle / numRays));
		}	


		playerInLos = playerSeen;
	}

	private Vector3 RotateVector(Vector3 v, float angle)
	{
		return Quaternion.AngleAxis(angle, Vector3.forward) * v;
	}
}



