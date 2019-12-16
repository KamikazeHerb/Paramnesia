using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableDoor : MonoBehaviour
{
    public GameObject doubleDoor;
    public LayerMask mask;
    private GameObject leftDoor;
    private GameObject rightDoor;
    private Vector3 startRotationLeft;
    private Vector3 startRotationRight;

    public void Start()
    {
        leftDoor = doubleDoor.transform.GetChild(0).gameObject;
        startRotationLeft = leftDoor.transform.eulerAngles;
        if (doubleDoor.transform.childCount > 1)
        {
            rightDoor = doubleDoor.transform.GetChild(1).gameObject;
            startRotationRight = rightDoor.transform.eulerAngles;
        }
    }

    // Update is called once per frame
    public void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(doubleDoor.transform.position, 2f, mask);
        for(int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].tag == "Player" || hitColliders[i].tag == "Guard")
            {
                leftDoor.transform.eulerAngles = new Vector3(startRotationLeft.x, startRotationLeft.y + 90, startRotationLeft.z);
                if (doubleDoor.transform.childCount > 1)
                {
                    rightDoor.transform.eulerAngles = new Vector3(startRotationRight.x, startRotationRight.y - 90, startRotationRight.z);
                }
                
            }
            else
            {
                leftDoor.transform.eulerAngles = new Vector3(startRotationLeft.x, startRotationLeft.y, startRotationLeft.z);
                if (doubleDoor.transform.childCount > 1)
                {
                    rightDoor.transform.eulerAngles = new Vector3(startRotationRight.x, startRotationRight.y, startRotationRight.z);
                }
                
            }
        }
    }
}
