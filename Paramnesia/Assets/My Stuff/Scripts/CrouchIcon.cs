using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class CrouchIcon : MonoBehaviour
{
    public GameObject player;
    public GameObject CrouchIconObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<ThirdPersonCharacter>().m_Crouching)
        {
            CrouchIconObject.SetActive(true);
        }
        else
        {
            CrouchIconObject.SetActive(false);
        }
    }
}
