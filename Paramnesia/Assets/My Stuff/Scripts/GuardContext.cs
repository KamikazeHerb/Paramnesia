using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AivoTree;
using UnityStandardAssets.Characters.ThirdPerson;
using System;
using System.Linq;

public class GuardContext : MonoBehaviour
{
    //AI fields
    public NavMeshAgent agent;
    public ThirdPersonCharacter character;
    public GameObject guardObject;
    public Transform pathHolder;
    public GameObject player;
    public GameObject objective;
    public GameObject[] guards;
    public LayerMask objectMask;
    public GuardClass guardClass;
    public Vector3 lastPlayerSighting;
    public bool alerted;
    public bool reachedTarget;
    public float turnSpeed;
    public float waitTime;
    public float FOVAngle;

    private Vector3 directionToPlayer;
    private Vector3[] waypoints;
    private Vector3 searchDirection;
    private Vector3 searchWaypoint;
    private Vector3 targetWaypoint;
    private GameObject targetLeader;
    private NavMeshPath currentPath;
    private Light spotLight;
    private bool reachedPatrolWaypoint;
    private bool playerVisible;
    private bool targetLeaderAlerted;
    private float searchRadius;
    private float walkingSpeed;
    private float searchSpeed;
    private float chaseSpeed;
    private float angleToPlayer;
    private float distanceToPlayer;
    private int targetWaypointIndex;

    public void Start()
    {
        //Field initialisation
        InitialiseFields();
        InitialisePatrolPaths();
    }

    //Getter methods
    public Vector3 GetSearchWaypoint()
    {
        return searchWaypoint;
    }

    public float GetSearchRadius()
    {
        return searchRadius;
    }

    public GameObject GetTargetLeader()
    {
        return targetLeader;
    }

    public int GetTargetWaypointIndex()
    {
        return targetWaypointIndex;
    }

    public Vector3[] GetWaypoints()
    {
        return waypoints;
    }

    public bool GetTargetLeaderStatus()
    {
        return targetLeaderAlerted;
    }

    public void SetSearchWaypoint(Vector3 waypoint)
    {
        searchWaypoint = waypoint;
    }
    public AivoTreeStatus IsPlayerVisible()
    {
        var lookTarget = player.transform.position;
        lookTarget.y = transform.position.y;
        var rayTarget = new Vector3(player.transform.position.x, player.gameObject.GetComponent<Collider>().bounds.max.y, player.transform.position.z);
        var rayOrigin = guardObject.transform.position;
        directionToPlayer = (rayTarget - rayOrigin).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        angleToPlayer = Vector3.Angle(new Vector3(directionToPlayer.x, 0, directionToPlayer.z), transform.forward);

        //Draw debug rays to show guard's field of view
        var direction = Quaternion.AngleAxis(FOVAngle, transform.up) * (transform.forward * 5);
        Debug.DrawRay(this.gameObject.transform.position, direction, Color.blue);
        var direction2 = Quaternion.AngleAxis(-FOVAngle, transform.up) * (transform.forward * 5);
        Debug.DrawRay(this.gameObject.transform.position, direction2, Color.blue);

        //If player is within guard's field of view
        if (Vector3.Distance(player.transform.position, transform.position) < 40 && angleToPlayer < FOVAngle)
        {
            //Ray cast to player to see if line of sight is obstructed
            if (!Physics.Linecast(rayOrigin, rayTarget, objectMask))
            {
                //Player can be seen, set guard status to alerted and persue player
                float dis = Vector3.Distance(rayOrigin, rayTarget);
                playerVisible = true;
                alerted = true;
                lastPlayerSighting = player.transform.position;
                searchWaypoint = lastPlayerSighting;
                reachedTarget = false;
                Debug.DrawLine(rayOrigin, rayTarget);
                return AivoTreeStatus.Success;
            }
            else
            {
                playerVisible = false;
                return AivoTreeStatus.Failure;
            }
        }
        else
        {
            playerVisible = false;
            return AivoTreeStatus.Failure;
        }
    }

    public AivoTreeStatus MoveToTarget(Vector3 Target)
    {

        if (alerted && playerVisible)
        {
            agent.speed = chaseSpeed;
        }
        else if(alerted)
        {
            agent.speed = searchSpeed;
        }
        else
        {
            agent.speed = walkingSpeed;
        }
        if (Vector3.Distance(transform.position, Target) > 1)
        {
            agent.CalculatePath(Target, currentPath);
            if (currentPath.status == NavMeshPathStatus.PathComplete)
            {
                agent.isStopped = false;
                agent.SetPath(currentPath);
                character.Move(agent.desiredVelocity, false, false);
                return AivoTreeStatus.Success;
            }
            else
            {
                return AivoTreeStatus.Failure;
            }
        }
        else
        {
            return AivoTreeStatus.Success;
        }
    }

    public AivoTreeStatus DetermineSearchWaypoint(Vector3 location, float radius)
    {
        //If waypoint not yeat reached, leave waypoint as it is and return success
        if (Vector3.Distance(transform.position, searchWaypoint) > 1)
        {
            Debug.DrawLine(transform.position, searchWaypoint, Color.white, 0.01f, false);
            return AivoTreeStatus.Success;
        }
        else
        {
            //Stop character moving
            reachedTarget = true;
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            character.Move(Vector3.zero, false, false);

            //Find a random location within a given radius of a given position, and if it is a 
            //valid walkable location on the navmesh then set it as the search waypoint
            var searchLocation = UnityEngine.Random.insideUnitSphere * radius;
            searchLocation += location;
            if (NavMesh.SamplePosition(searchLocation, out NavMeshHit hit, radius, 1))
            {
                agent.CalculatePath(hit.position, currentPath);
                if (currentPath.status == NavMeshPathStatus.PathComplete)
                {
                    
                    searchWaypoint = hit.position;
                    return AivoTreeStatus.Success;
                }
                else
                {
                    return AivoTreeStatus.Running;
                }       
            }
            else
            {
                return AivoTreeStatus.Running;
            }
        }
    }

    public AivoTreeStatus FindNearestLeader()
    {
        int leadersFound = 0;
        var leaders = new List<GameObject>();

        //Find leader guards and add them to a list
        foreach (GameObject guard in guards)
        {
            if (guard.GetComponent<GuardContext>().guardClass == GuardClass.LEADER)
            {
                leadersFound++;
                leaders.Add(guard);
            }
        }

        //If there is more than 1 leader, find closest leader
        if (leadersFound > 1)
        {
            var distances = new float[leadersFound];
            for (int i = 0; i < leadersFound; i++)
            {
                distances[i] = Vector3.Distance(transform.position, leaders[i].transform.position);
            }
            targetLeader = leaders[Array.IndexOf(distances, distances.Min())];
            return AivoTreeStatus.Success;
        }
        else if (leadersFound == 1)
        {
            targetLeader = leaders[0];
            return AivoTreeStatus.Success;
        }
        else
        {
            //No leaders found
            return AivoTreeStatus.Failure;
        }
    }

    public AivoTreeStatus IsTargetLeaderAlerted()
    {
        if (targetLeader.GetComponent<GuardContext>().alerted)
        {
            targetLeaderAlerted = true;
            return AivoTreeStatus.Failure;
        }
        else
        {
            targetLeaderAlerted = false;
            return AivoTreeStatus.Success;
        }
    }

    public AivoTreeStatus IsCorrectGuardClass(GuardClass guardClass)
    {
        if (guardClass == this.guardClass)
        {
            return AivoTreeStatus.Success;
        }
        else
        {
            return AivoTreeStatus.Failure;
        }
    }

    //Alerts all guards within a specified radius of a specified position to the player's location
    public AivoTreeStatus AlertAllGuards(float radius, Vector3 center, Vector3 playerPosition)
    {
        guards = GameObject.FindGameObjectsWithTag("Guard");
        foreach (GameObject guard in guards)
        {
            if (Vector3.Distance(center, guard.transform.position) < radius && !guard.GetComponent<GuardContext>().alerted)
            {
                guard.GetComponent<GuardContext>().alerted = true;
                guard.GetComponent<GuardContext>().reachedTarget = false;
                guard.GetComponent<GuardContext>().SetSearchWaypoint(playerPosition);
                guard.GetComponent<GuardContext>().lastPlayerSighting = playerPosition;
            }

        }
        return AivoTreeStatus.Success;
    }

    public AivoTreeStatus DetermineNextPatrolWaypoint()
    {
        agent.speed = walkingSpeed;
        if (Vector3.Distance(transform.position, waypoints[targetWaypointIndex]) >= 0.2f)
        {
            return AivoTreeStatus.Success; ;
        }
        else
        {
            targetWaypointIndex += 1;
            if (targetWaypointIndex >= waypoints.Length)
            {
                targetWaypointIndex = 0;
            }
            return AivoTreeStatus.Success;
        }
    }

    public void InitialiseFields()
    {
        searchWaypoint = Vector3.zero;
        walkingSpeed = 0.5f;
        turnSpeed = 1f;
        agent = guardObject.GetComponent<NavMeshAgent>();
        character = guardObject.GetComponent<ThirdPersonCharacter>();
        spotLight = guardObject.GetComponentInChildren<Light>();
        guards = GameObject.FindGameObjectsWithTag("Guard");
        player = GameObject.FindGameObjectWithTag("Player");
        agent.updatePosition = true;
        agent.updateRotation = false;
        targetWaypointIndex = 0;
        alerted = false;
        reachedTarget = false;
        currentPath = new NavMeshPath();
        agent.CalculatePath(transform.position, currentPath);
        searchDirection = Vector3.zero;

        switch (guardClass)
        {
            case GuardClass.CHASER:
                walkingSpeed = 0.5f;
                searchSpeed = 1f;
                chaseSpeed = 1f;
                searchRadius = 10f;
                spotLight.color = Color.red;
                break;
            case GuardClass.LEADER:
                walkingSpeed = 0.5f;
                searchSpeed = 0.5f;
                chaseSpeed = 0.8f;
                searchRadius = 7f;
                spotLight.color = Color.blue;
                break;
            case GuardClass.INFORMER:
                walkingSpeed = 0.5f;
                searchSpeed = 0.8f;
                chaseSpeed = 0.8f;
                searchRadius = 7f;
                spotLight.color = Color.yellow;
                break;
            case GuardClass.SNEAKY:
                walkingSpeed = 0.5f;
                searchSpeed = 0.5f;
                chaseSpeed = 0.8f;
                searchRadius = 10f;
                spotLight.color = Color.green;
                break;
        }
    }

    public void InitialisePatrolPaths()
    {
        //Patrol path setup
        waypoints = new Vector3[pathHolder.childCount];
        if (waypoints.Length > 1)
        {
            targetWaypoint = waypoints[targetWaypointIndex];
            transform.LookAt(targetWaypoint);
        }
        //Sets the locations of each waypoint using pathHolder object
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
    }
}
