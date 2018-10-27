using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ButtonFunctions : MonoBehaviour
{
    public GameObject menu;
    public GameObject otherMenu;
    public static bool menuActive = false;

    public void ChangeScene(string sceneName)
    {
        Time.timeScale = 1;
        menuActive = false;
        WorldController.ChangeScene(sceneName);
    }

    public void Quit()
    {
        WorldController.Quit();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        menuActive = false;
        WorldController.RestartLevel();
    }

    public void Pause()
    {
        Time.timeScale = 0;
        menu.SetActive(true);
        menuActive = true;
        otherMenu.SetActive(false);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        menu.SetActive(false);
        menuActive = false;
    }
}


