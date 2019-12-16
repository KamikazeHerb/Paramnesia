using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    public class AISight : MonoBehaviour
    {
        public Transform guard;
        public float angleToPlayer;
        public float distanceToPlayer;
        public bool PlayerVisible;
        public Vector3 lastPlayerSighting;
        public Vector3 directionToPlayer;
        public Transform player;
        public LayerMask objectMask;

        private float FOVAngle;
        public void Start()
        {
            FOVAngle = (this.gameObject.GetComponentInChildren<Light>().spotAngle/2) * 0.9f;
            PlayerVisible = false;
            lastPlayerSighting = Vector3.zero;
        }

        public void Update()
        {
            LookForPlayer();
        }

        public void LookForPlayer()
        {
            Vector3 lookTarget = player.position;
            lookTarget.y = transform.position.y;
            Vector3 rayTarget = new Vector3(player.position.x, player.gameObject.GetComponent<Collider>().bounds.max.y, player.position.z);
            Vector3 rayOrigin = guard.position;
            directionToPlayer = (rayTarget - rayOrigin).normalized;
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
            angleToPlayer = Vector3.Angle(directionToPlayer, this.transform.forward);

            //Draw debug rays to show guard's field of view
            //var direction = Quaternion.AngleAxis(FOVAngle, transform.up) * (transform.forward * 5);
            //Debug.DrawRay(this.gameObject.transform.position, direction, Color.blue);
            //var direction2 = Quaternion.AngleAxis(-FOVAngle, transform.up) * (transform.forward * 5);
            //Debug.DrawRay(this.gameObject.transform.position, direction2, Color.blue);

            //If player is within guard's field of view
            if (Vector3.Distance(player.position, this.transform.position) < 40 && angleToPlayer < FOVAngle)
            {
                //Ray cast to player to see if line of sight is obstructed
                if (!Physics.Raycast(rayOrigin, (directionToPlayer * distanceToPlayer), out RaycastHit hit, distanceToPlayer, objectMask))
                {
                    float dis = Vector3.Distance(rayOrigin, rayTarget);
                    PlayerVisible = true;
                    GetComponent<GuardFSM>().alerted = true;
                    lastPlayerSighting = player.position;
                }
                else
                {
                    PlayerVisible = false;
                }
            }
            else
            {
                PlayerVisible = false;
            }
        }
    }
}
