using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGame : MonoBehaviour {
	public GameObject menu; // Assign in inspector

	void Update()
	{
		if (Input.GetKeyDown("escape") && !ButtonFunctions.menuActive) 
		{
			Time.timeScale = 0;
			menu.SetActive(true);
			ButtonFunctions.menuActive = true;
		}
	}
}
