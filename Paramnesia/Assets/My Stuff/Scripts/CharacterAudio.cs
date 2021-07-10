﻿using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Class to handle playing sound effects using animations
 * 
 */
public class CharacterAudio : MonoBehaviour
{
    public AudioSource footstep;
    private GameObject[] guards;
    private float loudStepVolume;
    private float quietStepVolume;
    private float crouchStepVolume;
    private InputAction sprint;

    void Start()
    {
        sprint = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton", interactions: "");
        sprint.Enable();
        guards = GameObject.FindGameObjectsWithTag("Guard");
        footstep = GetComponent<AudioSource>();
        if (gameObject.CompareTag("Player"))
        {
            loudStepVolume = 0.025f;
            quietStepVolume = 0.0125f;
            crouchStepVolume = 0.0075f;
        }
        else
        {
            loudStepVolume = 0.03f;
            quietStepVolume = 0.015f;
            crouchStepVolume = 0.01f;
        }
    }

    public void LoudStep()
    {
        //If player is running, alert guards within 5 meters of player
        if (gameObject.CompareTag("Player") && sprint.ReadValue<float>() == 1)
        {
            foreach (GameObject guard in guards.Where(g => Vector3.Distance(g.transform.position, gameObject.transform.position) < 5 && !g.GetComponent<GuardContext>().alerted))
            {
                guard.GetComponent<GuardContext>().alerted = true;
                guard.GetComponent<GuardContext>().lastPlayerSighting = GameObject.FindGameObjectWithTag("Player").transform.position;
                guard.transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform.position);
                guard.GetComponent<GuardContext>().SetSearchWaypoint(GameObject.FindGameObjectWithTag("Player").transform.position);
                
            }
        }
        footstep.volume = loudStepVolume;
        footstep.Play();
    }

    public void QuietStep()
    {
        footstep.volume = quietStepVolume;
        footstep.Play();
    }

    public void CrouchStep()
    {
        footstep.volume = crouchStepVolume;
        footstep.Play();
    }

}
