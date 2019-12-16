using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets.Characters.ThirdPerson
{

    public enum GuardClass
    {
        LEADER, CHASER, SNEAKY, INFORMER
    }

    public class GuardFSM : MonoBehaviour
    {
        //AI fields
        public NavMeshAgent agent;
        public ThirdPersonCharacter character;
        public GameObject guardObject;
        public Transform pathHolder;
        public Transform player;
        public GameObject objective;
        public GameObject[] guards;
        public GuardClass guardClass;
        public State currentState;
        public Vector3[] waypoints;
        public bool alerted;
        public bool reachedTarget;
        public float turnSpeed;
        public float waitTime;

        private NavMeshPath currentPath;
        private Light spotLight;
        private Vector3 searchDirection;
        private Vector3 searchWaypoint;
        private Vector3 targetWaypoint;
        private bool active;
        private float walkingSpeed;
        private float searchSpeed;
        private float chaseSpeed;
        private int targetWaypointIndex;

        //The different states that the guard AI can be in. Can only be in one state at a time
        public enum State
        {
            PATROLLING, PURSUING, SEARCHING,
        }

        //Initialisation
        public void Start()
        {
            switch (guardClass)
            {
                case GuardClass.CHASER:
                    chaseSpeed = 1f;
                    searchSpeed = 1f;
                    break;
                case GuardClass.INFORMER:
                    chaseSpeed = 0.8f;
                    searchSpeed = 0.8f;
                    break;
                case GuardClass.LEADER:
                    chaseSpeed = 0.8f;
                    searchSpeed = 0.5f;
                    break;
                case GuardClass.SNEAKY:
                    chaseSpeed = 0.8f;
                    searchSpeed = 0.5f;
                    break;
            }

            //Initialise fields
            searchWaypoint = Vector3.zero;
            walkingSpeed = 0.5f;
            turnSpeed = 1f;
            agent = guardObject.GetComponent<NavMeshAgent>();
            character = guardObject.GetComponent<ThirdPersonCharacter>();
            spotLight = guardObject.GetComponentInChildren<Light>();
            guards = GameObject.FindGameObjectsWithTag("Guard");
            waypoints = new Vector3[pathHolder.childCount];
            agent.updatePosition = true;
            agent.updateRotation = false;
            targetWaypointIndex = 0;
            alerted = false;
            reachedTarget = false;
            currentPath = new NavMeshPath();
            agent.CalculatePath(transform.position, currentPath);
            searchDirection = Vector3.zero;
            currentState = State.PATROLLING;
            active = true;
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
            active = true;

            //Start finite state machine
            StartCoroutine("FiniteStateMachine");
        }

        /*
         * Runs the finite state machiene so long as the guard is active
         */
        private IEnumerator FiniteStateMachine()
        {
            while (active)
            {
                //Plays chase theme only if player is being persued
                bool chase = false;
                foreach (GameObject guard in guards)
                {
                    if (guard.GetComponent<GuardFSM>().currentState == State.PURSUING)
                    {
                        chase = true;
                    }
                }
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMusic>().chase = chase;

                //Reset level if player is caught
                if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < 1)
                {
                    SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
                }

                //Checks for player in line of sight
                UpdateAwareness();
                //Run different behaviour based on current state
                switch (currentState)
                {
                    case State.PATROLLING:
                        Patrol();
                        break;
                    case State.SEARCHING:
                        Search();
                        break;
                    case State.PURSUING:
                        Pursue();
                        break;
                }
                yield return null;
            }
        }


        //Checks to see if player is visible to guard
        void UpdateAwareness()
        {
            if (GetComponent<AISight>().PlayerVisible)
            {
                currentState = State.PURSUING;
            }
            else if (!GetComponent<AISight>().PlayerVisible && alerted)
            {
                currentState = State.SEARCHING;
            }
            else
            {
                currentState = State.PATROLLING;
            }
        }


        public void Patrol()
        {
            switch (guardClass)
            {
                case GuardClass.CHASER:
                    spotLight.color = Color.red;
                    break;
                case GuardClass.LEADER:
                    spotLight.color = Color.blue;
                    break;
                case GuardClass.INFORMER:
                    spotLight.color = Color.yellow;
                    break;
                case GuardClass.SNEAKY:
                    spotLight.color = Color.green;
                    break;

            }
            agent.speed = walkingSpeed;
            if (Vector3.Distance(transform.position, waypoints[targetWaypointIndex]) >= 0.2f)
            {
                agent.isStopped = false;
                agent.SetDestination(waypoints[targetWaypointIndex]);
                character.Move(agent.desiredVelocity, false, false);
            }
            else if (Vector3.Distance(transform.position, waypoints[targetWaypointIndex]) < 0.2f)
            {
                agent.isStopped = true;
                targetWaypointIndex += 1;
                if (targetWaypointIndex >= waypoints.Length)
                {
                    targetWaypointIndex = 0;
                }
            }
            else
            {
                character.Move(Vector3.zero, false, false);
            }
        }

        public void Pursue()
        {
            agent.speed = chaseSpeed;
            switch (guardClass)
            {
                case GuardClass.CHASER:
                    spotLight.color = Color.red;
                    break;
                case GuardClass.LEADER:
                    spotLight.color = Color.blue;
                    break;
                case GuardClass.INFORMER:
                    spotLight.color = Color.yellow;
                    break;
                case GuardClass.SNEAKY:
                    spotLight.color = Color.green;
                    break;

            }
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            reachedTarget = false;
            if (guardClass == GuardClass.LEADER)
            {
                AlertAllGuards(10, transform.position, this.gameObject.GetComponent<AISight>().lastPlayerSighting);
            }
            if (Vector3.Distance(transform.position, player.position) > 0.5f)
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                agent.CalculatePath(player.position, currentPath);
                agent.SetPath(currentPath);
                character.Move(agent.desiredVelocity, false, false);
            }
            else
            {
                //Else statement in case of unexpected conditions
            }
        }

        public void Search()
        {
            switch (guardClass)
            {
                case GuardClass.CHASER:
                    spotLight.color = Color.red;
                    agent.speed = searchSpeed;
                    //If search direction not already updated, update it 
                    Explore(transform.position, 10f);
                    break;
                case GuardClass.LEADER:
                    spotLight.color = Color.blue;
                    agent.speed = searchSpeed;
                    //If the AI has not yet reached the player's last seen location, set this as target destination
                    if (!reachedTarget)
                    {
                        if (Vector3.Distance(transform.position, GetComponent<AISight>().lastPlayerSighting) > 0.5)
                        {
                            agent.CalculatePath(GetComponent<AISight>().lastPlayerSighting, currentPath);
                            if (currentPath.status == NavMeshPathStatus.PathComplete)
                            {
                                agent.isStopped = false;
                                agent.speed = chaseSpeed;
                                agent.SetPath(currentPath);
                                character.Move(agent.desiredVelocity, false, false);
                            }
                            else
                            {
                                Explore(transform.position, 7f);
                            }
                            
                        }
                        else if (Vector3.Distance(transform.position, GetComponent<AISight>().lastPlayerSighting) <= 0.5)
                        {
                            //Reached location player was last seen at
                            reachedTarget = true;
                            transform.LookAt(GetComponent<AISight>().lastPlayerSighting * Time.deltaTime);
                            agent.isStopped = true;
                            character.Move(Vector3.zero, false, false);
                            agent.ResetPath();
                            agent.velocity = Vector3.zero;
                        }
                        else
                        {
                            //Else statement in case of unexpected conditions
                        }
                    }
                    //If AI has reached player's last seen location, begin Exploring
                    else
                    {
                        Explore(transform.position, 7f);
                    }
                    break;
                case GuardClass.SNEAKY:
                    spotLight.color = Color.green;
                    if (!reachedTarget)
                    {
                        if (Vector3.Distance(transform.position, GetComponent<AISight>().lastPlayerSighting) > 0.5)
                        {
                            agent.CalculatePath(GetComponent<AISight>().lastPlayerSighting, currentPath);
                            if (currentPath.status == NavMeshPathStatus.PathComplete)
                            {
                                agent.isStopped = false;
                                agent.speed = chaseSpeed;
                                agent.SetPath(currentPath);
                                character.Move(agent.desiredVelocity, false, false);
                            }
                            else
                            {
                                Explore(player.position, 10f);
                            }

                        }
                        else if (Vector3.Distance(transform.position, GetComponent<AISight>().lastPlayerSighting) <= 0.5)
                        {
                            //Reached location player was last seen at
                            reachedTarget = true;
                            transform.LookAt(GetComponent<AISight>().lastPlayerSighting * Time.deltaTime);
                            agent.isStopped = true;
                            character.Move(Vector3.zero, false, false);
                            agent.ResetPath();
                            agent.velocity = Vector3.zero;
                        }
                        else
                        {
                            //Else statement in case of unexpected conditions
                        }
                    }
                    else
                    {
                        Explore(player.position, 10f);
                    }
                    //If the AI has not yet reached the player's last seen location, set this as target destination
                    
                    break;
                case GuardClass.INFORMER:
                    spotLight.color = Color.yellow;
                    int leadersFound = 0;
                    int leaderIndex = 0;
                    List<GameObject> leaders = new List<GameObject>();
                    Vector3 targetLeaderPosition = Vector3.zero;

                    foreach (GameObject guard in guards)
                    {
                        if (guard.GetComponent<GuardFSM>().guardClass == GuardClass.LEADER)
                        {
                            leadersFound++;
                            leaders.Add(guard);
                        }
                    }

                    if (leadersFound > 1)
                    {
                        float[] distances = new float[leadersFound];
                        for (int i = 0; i < leadersFound; i++)
                        {
                            distances[i] = Vector3.Distance(transform.position, leaders[i].transform.position);
                        }
                        targetLeaderPosition = leaders[Array.IndexOf(distances, distances.Min())].transform.position;
                    }
                    else if (leadersFound == 1)
                    {
                        targetLeaderPosition = leaders[0].transform.position;
                        leaderIndex = 0;
                    }
                    else
                    {
                        //No leaders found
                    }
                    //If leader found is alerted to player's prescence
                    if (!leaders[leaderIndex].GetComponent<GuardFSM>().alerted)
                    {
                        //See if there is a valid path to the leader
                        agent.CalculatePath(targetLeaderPosition, currentPath);
                        if (currentPath.status == NavMeshPathStatus.PathComplete)
                        {
                            agent.isStopped = false;
                            agent.speed = chaseSpeed;
                            agent.SetPath(currentPath);
                            character.Move(agent.desiredVelocity, false, false);
                            if (Vector3.Distance(transform.position, targetLeaderPosition) < 2)
                            {
                                leaders[leaderIndex].GetComponent<GuardFSM>().alerted = true;
                                leaders[leaderIndex].GetComponent<GuardFSM>().currentState = State.SEARCHING;
                                leaders[leaderIndex].GetComponent<AISight>().lastPlayerSighting = this.gameObject.GetComponent<AISight>().lastPlayerSighting;
                                AlertAllGuards(10, transform.position, this.gameObject.GetComponent<AISight>().lastPlayerSighting);
                                reachedTarget = true;
                            }
                        }
                        else
                        {
                            //If leader can't be reached, wander
                            Explore(transform.position, 7f);
                        }


                    }
                    //Travel to leader if leader hasn't been alerted
                    else if (Vector3.Distance(transform.position, targetLeaderPosition) > 2 && !reachedTarget)
                    {
                        //Test if there is a valid path to leader
                        agent.CalculatePath(targetLeaderPosition, currentPath);
                        if (currentPath.status == NavMeshPathStatus.PathComplete)
                        {
                            agent.isStopped = false;
                            agent.speed = walkingSpeed;
                            agent.SetPath(currentPath);
                            character.Move(agent.desiredVelocity, false, false);
                        }
                        else
                        {
                            //If there is no valid path to the leader, explore around current area
                            Explore(transform.position, 7f);
                        }
                        
                    }
                    //Stop when close to leader who needs to be alerted
                    else if (Vector3.Distance(transform.position, targetLeaderPosition) < 2 && !reachedTarget)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                        character.Move(Vector3.zero, false, false);
                    }
                    //If leader alerted, explore around area close to leader
                    else if (reachedTarget)
                    {
                        Explore(targetLeaderPosition, 7f);
                    }
                    break;
            }

        }

        public void AlertAllGuards(float radius, Vector3 center, Vector3 playerPosition)
        {
            guards = GameObject.FindGameObjectsWithTag("Guard");
            foreach (GameObject guard in guards)
            {
                if (Vector3.Distance(center, guard.transform.position) < radius && !guard.GetComponent<GuardFSM>().alerted)
                {
                    guard.GetComponent<GuardFSM>().alerted = true;
                    guard.GetComponent<GuardFSM>().currentState = State.SEARCHING;
                    guard.GetComponent<GuardFSM>().reachedTarget = false;
                    guard.GetComponent<AISight>().lastPlayerSighting = playerPosition;
                }
                
            }
        }

        /*
         * Explore
         * 
         * 
         * Randomly explores within a given radius of a given target location. Uses waypoints set on navmesh
         * 
         */
        private void Explore(Vector3 location, float radius)
        {
            if (this.searchDirection == Vector3.zero)
            {
                searchDirection = player.position - GetComponent<AISight>().lastPlayerSighting;
            }

            //If the AI has not yet reached the player's last seen location, set this as target destination
            if (!reachedTarget)
            {
                searchWaypoint = GetComponent<AISight>().lastPlayerSighting;
                if (Vector3.Distance(transform.position, GetComponent<AISight>().lastPlayerSighting) > 0.5)
                {                  
                    agent.CalculatePath(GetComponent<AISight>().lastPlayerSighting, currentPath);
                    if (currentPath.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.isStopped = false;
                        agent.speed = chaseSpeed;
                        agent.SetPath(currentPath);
                        character.Move(agent.desiredVelocity, false, false);
                    }
                    else
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                        agent.velocity = Vector3.zero;
                        character.Move(Vector3.zero, false, false);
                    }
                    
                }
                else if (Vector3.Distance(transform.position, GetComponent<AISight>().lastPlayerSighting) <= 0.5)
                {
                    //Reached location player was last seen at
                    reachedTarget = true;
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    character.Move(Vector3.zero, false, false);
                    transform.LookAt(GetComponent<AISight>().lastPlayerSighting * Time.deltaTime);
                }
                else
                {
                    //Else statement in case of unexpected conditions
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    character.Move(Vector3.zero, false, false);
                }
            }
            //If searching after reaching player's last known location, randomly explores the navmesh until behaviour changes
            else
            {
                //Travel to current waypoint. Tests distance to waypoint, ignoring y direction due to some walls being included in navmesh
                if (Vector3.Distance(transform.position, new Vector3(searchWaypoint.x, transform.position.y, searchWaypoint.z)) > 2)
                {
                    agent.CalculatePath(searchWaypoint, currentPath);
                    //Check path is valid
                    if (currentPath.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.isStopped = false;
                        agent.speed = searchSpeed;
                        agent.SetPath(currentPath);
                        character.Move(agent.desiredVelocity, false, false);
                    }
                    //If no valid path to waypoint, create new waypoint
                    else
                    {
                        Vector3 searchLocation = UnityEngine.Random.insideUnitSphere * radius;
                        searchLocation += location;
                        if (NavMesh.SamplePosition(searchLocation, out NavMeshHit hit, radius, 1))
                        {
                            searchWaypoint = hit.position;
                        }
                    }
                    
                }
                else
                //Waypoint reached, create new waypoint
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    character.Move(Vector3.zero, false, false);
                    Vector3 searchLocation = UnityEngine.Random.insideUnitSphere * radius;
                    searchLocation += location;
                    if (NavMesh.SamplePosition(searchLocation, out NavMeshHit hit, radius, 1))
                    {
                        searchWaypoint = hit.position;
                    }
                }

            }

        }


    }

}
