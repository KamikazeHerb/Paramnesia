using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMusic : MonoBehaviour
{
    //Sets initial last sighting as a position the player couldn't possibly be
    public bool chase = false;
    public float musicFadeSpeed = 1f;
    public bool musicEnabled;

    private AudioSource chaseTheme;
    private AudioSource sneakTheme;

    public void Start()
    {
        musicEnabled = true;
    }

    public void Update()
    {
        if (musicEnabled)
        {
            chaseTheme.UnPause();
            sneakTheme.UnPause();
            MusicFade();
        }
        else
        {
            chaseTheme.Pause();
            sneakTheme.Pause();
        }
        
    }

    public void Awake()
    {
        chaseTheme = transform.Find("AlertTheme").GetComponent<AudioSource>();
        sneakTheme = GetComponent<AudioSource>();
    }

    void MusicFade()
    {
        if (chase)
        {
            sneakTheme.volume = Mathf.Lerp(GetComponent<AudioSource>().volume, 0f, musicFadeSpeed * Time.deltaTime);
            chaseTheme.volume = Mathf.Lerp(chaseTheme.volume, 0.05f, musicFadeSpeed * Time.deltaTime);
        }
        else
        {
            sneakTheme.volume = Mathf.Lerp(GetComponent<AudioSource>().volume, 0.05f, musicFadeSpeed * Time.deltaTime);
            chaseTheme.volume = Mathf.Lerp(chaseTheme.volume, 0f, musicFadeSpeed * Time.deltaTime);
        }
    }
}
