using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    static bool AudioBegin = false;
    AudioSource music = null;

    void Awake()
    {
        AudioSource audio = GetComponent<AudioSource>();
        music = audio;

        if (!AudioBegin)
        {
            music.Play();
            DontDestroyOnLoad(gameObject);
            AudioBegin = true;
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Level6" ||
            SceneManager.GetActiveScene().name == "MainMenu")
        {
            music.Stop();
            AudioBegin = false;
        }
    }
}

