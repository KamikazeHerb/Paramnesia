using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;


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

    void Start()
    {
        guards = GameObject.FindGameObjectsWithTag("Guard");
        footstep = GetComponent<AudioSource>();
        if (this.gameObject.tag == "Player")
        {
            loudStepVolume = 0.005f;
            quietStepVolume = 0.0025f;
            crouchStepVolume = 0.0015f;
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
        if (this.gameObject.tag == "Player" && Input.GetMouseButton(0))
        {
            foreach (GameObject guard in guards)
            {
                //Used to switch beteen Finite State Machine and Behaviour tree modes
                if (false)
                {
                    if (Vector3.Distance(guard.transform.position, this.gameObject.transform.position) < 5 && !guard.GetComponent<GuardFSM>().alerted)
                    {
                        guard.GetComponent<GuardFSM>().alerted = true;
                        guard.GetComponent<GuardFSM>().currentState = GuardFSM.State.SEARCHING;
                        guard.GetComponent<GuardFSM>().reachedTarget = false;
                        guard.GetComponent<AISight>().lastPlayerSighting = GameObject.FindGameObjectWithTag("Player").transform.position;
                    }
                }
                else
                {
                    if (Vector3.Distance(guard.transform.position, this.gameObject.transform.position) < 5 && !guard.GetComponent<GuardContext>().alerted)
                    {
                        guard.GetComponent<GuardContext>().alerted = true;
                        guard.GetComponent<GuardContext>().lastPlayerSighting = GameObject.FindGameObjectWithTag("Player").transform.position;
                        guard.transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform.position);
                        guard.GetComponent<GuardContext>().SetSearchWaypoint(GameObject.FindGameObjectWithTag("Player").transform.position);
                    }
                    
                }
                
            }
        }
        footstep.volume = loudStepVolume;
        footstep.Play();
        //footstep.PlayOneShot(footstep.clip);
    }

    public void QuietStep()
    {
        footstep.volume = quietStepVolume;
        footstep.Play();
        //footstep.PlayOneShot(footstep.clip);
    }

    public void CrouchStep()
    {
        footstep.volume = crouchStepVolume;
        footstep.Play();
        //footstep.PlayOneShot(footstep.clip);
    }

}
