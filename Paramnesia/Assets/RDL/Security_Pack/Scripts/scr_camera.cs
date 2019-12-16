using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;
using static UnityStandardAssets.Characters.ThirdPerson.GuardFSM;

public class scr_camera : MonoBehaviour
{
    public GameObject cameraGuard;
    public Transform cameraLight;
    public Transform player;
    public Vector3 lastPlayerSighting;
    public Vector3 directionToPlayer;
    public LayerMask objectMask;
    public float rotationSpeed;
    public float angleToPlayer;
    public float distanceToPlayer;
    public bool PlayerVisible;

    private Transform swivel_camera;
    private Light spotLight;
    private Vector3 startRotation;
    private float FOVAngle;

    // Use this for initialization
    public void Start()
    {
        swivel_camera = this.gameObject.transform.Find("Swivel").Find("Angle").transform;
        startRotation = swivel_camera.eulerAngles;
        lastPlayerSighting = Vector3.zero;
        spotLight = GetComponentInChildren<Light>();
        FOVAngle = spotLight.spotAngle * 0.9f;
    }

    // Update is called once per frame
    public void Update()
    {
        if (PlayerVisible)
        {
            swivel_camera.LookAt(player);
            swivel_camera.eulerAngles = new Vector3(swivel_camera.eulerAngles.x, swivel_camera.eulerAngles.y, startRotation.z);
            cameraLight.LookAt(player);
            spotLight.color = Color.red;
        }
        else
        {
            spotLight.color = Color.green;
            swivel_camera.rotation = Quaternion.Euler(new Vector3(startRotation.x, startRotation.y + (80 * Mathf.Sin(Time.time * rotationSpeed)), startRotation.z));
            spotLight.transform.rotation = Quaternion.Euler(new Vector3(startRotation.x, startRotation.y + (80 * Mathf.Sin(Time.time * rotationSpeed)), startRotation.z));

        }


        //Player detection works in a smilar way to AISight script
        Vector3 lookTarget = player.position;
        lookTarget.y = swivel_camera.position.y;
        Vector3 rayTarget = new Vector3(player.position.x, player.gameObject.GetComponent<Collider>().bounds.max.y, player.position.z);
        Vector3 rayOrigin = swivel_camera.position;
        directionToPlayer = (rayTarget - rayOrigin).normalized;
        angleToPlayer = Vector3.Angle(directionToPlayer, swivel_camera.forward);
        distanceToPlayer = Vector3.Distance(swivel_camera.position,
            new Vector3(player.position.x, player.gameObject.GetComponent<Collider>().bounds.max.y, player.position.z));
        if (Vector3.Distance(player.position, swivel_camera.position) < 10 && angleToPlayer < FOVAngle)
        {
            if (!Physics.Raycast(rayOrigin, (directionToPlayer * distanceToPlayer), out RaycastHit hit, distanceToPlayer, objectMask))
            {
                float dis = Vector3.Distance(rayOrigin, rayTarget);
                //Player spotted, alert camera guard if he is not already alerted
                PlayerVisible = true;
                lastPlayerSighting = player.position;
                if (!cameraGuard.GetComponent<GuardFSM>().alerted)
                {
                    //cameraGuard.GetComponent<GuardFSM>().alerted = true;
                    //cameraGuard.GetComponent<GuardFSM>().currentState = State.SEARCHING;
                    //cameraGuard.GetComponent<GuardFSM>().reachedTarget = false;
                    //cameraGuard.GetComponent<AISight>().lastPlayerSighting = GameObject.FindGameObjectWithTag("Player").transform.position;

                    cameraGuard.GetComponent<GuardContext>().alerted = true;
                    cameraGuard.GetComponent<GuardContext>().reachedTarget = false;
                    cameraGuard.GetComponent<GuardContext>().lastPlayerSighting = GameObject.FindGameObjectWithTag("Player").transform.position;
                    cameraGuard.GetComponent<GuardContext>().SetSearchWaypoint(GameObject.FindGameObjectWithTag("Player").transform.position);
                }
                


            }
            else
            {
                if (PlayerVisible)
                {
                    swivel_camera.eulerAngles = startRotation;
                    cameraLight.transform.eulerAngles = startRotation;
                }
                PlayerVisible = false;
            }
        }
        else
        {
            if (PlayerVisible)
            {
                swivel_camera.eulerAngles = startRotation;
                cameraLight.transform.eulerAngles = startRotation;
            }
            PlayerVisible = false;
        }

    }
}
