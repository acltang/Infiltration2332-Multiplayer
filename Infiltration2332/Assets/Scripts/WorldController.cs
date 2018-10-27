using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {
	public static bool enableRestart = true;
	public static float lastRestart = 0;
	// Use this for initialization
	void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
		
		if(Input.GetKeyDown(KeyCode.R) && Time.time - lastRestart > 10)
        {
			lastRestart = Time.time;
			ConnectionManager gameConnection = GameObject.Find("Game Connection").GetComponent<ConnectionManager>();
			gameConnection.sendLoadLevelMessage(SceneManager.GetActiveScene().name);
        }
		/*
        else if(Input.GetKeyDown(KeyCode.Q))
        {
            Quit();
        }
        */
	}

    public static void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void RestartLevel()
    {
		if (enableRestart) 
		{
			string currentLevel = SceneManager.GetActiveScene ().name;
			ChangeScene (currentLevel);
		}
    }

	public static void GoToNextLevel()
	{
		int currentScene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene (SceneManager.GetSceneByBuildIndex (currentScene + 1).name);
	}

    public static void Quit()
    {
    #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
		Application.Quit();
    }
}
