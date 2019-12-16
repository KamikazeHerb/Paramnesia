using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AivoTree;
using UnityStandardAssets.Characters.ThirdPerson;
using System;
using UnityEngine.SceneManagement;

public class BehaviourTree : MonoBehaviour
{
    public TreeNode<GuardContext> tree;
    public GameObject[] guards;
    //public GuardContext context;

    // Start is called before the first frame update
    public void Start()
    {

        guards = GameObject.FindGameObjectsWithTag("Guard");

        //Behaviour tree setup
        //Action Node Setup

        var AlertNearbyGuards = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            ctx.AlertAllGuards(10, ctx.transform.position, ctx.getSearchWaypoint());
            return AivoTreeStatus.Success;
        });

        //Pursue behaviour Action nodes
        var isPlayerVisible = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.IsPlayerVisible();
        });

        var moveToPlayer = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            //Debug.Log(ctx.guardClass.ToString() + "Chasing Player at: " + ctx.player.transform.position.ToString());
            return ctx.MoveToTarget(ctx.player.transform.position);
        });

        //Search Behaviour Action nodes

        var GuardAware = new ActionNode<GuardContext>((timeTick, ctx) => 
        {
            if (ctx.alerted)
            {
                return AivoTreeStatus.Success;
            }
            else
            {
                return AivoTreeStatus.Failure;
            }
        });

        //Chaser search Action nodes
        var IsChaser = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.IsCorrectGuardClass(GuardClass.CHASER);
        });

        var SearchWaypoint = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            if (ctx.guardClass == GuardClass.SNEAKY)
            {
                Debug.Log("Sneakies are chasers or leaders");
            }
            return ctx.DetermineSearchWaypoint(ctx.transform.position, ctx.GetSearchRadius());
        });

        var MoveToSearchWaypoint = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.MoveToTarget(ctx.getSearchWaypoint());
        });

        

        //Leader search Action nodes
        var IsLeader = new ActionNode<GuardContext>((timeTick, GuardData) =>
        {
            return GuardData.IsCorrectGuardClass(GuardClass.LEADER);
        });

        //Informer search Action nodes

        var IsInformer = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.IsCorrectGuardClass(GuardClass.INFORMER);
        });

        var FindNearestLeader = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.FindNearestLeader();
        });

        var IsTargetLeaderAlerted = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.IsTargetLeaderAlerted();
        });

        var TravelToTargetLeader = new ActionNode<GuardContext>((timeTick, ctx) => 
        {
            return ctx.MoveToTarget(ctx.GetTargetLeader().transform.position);
        });

        var AlertTargetLeader = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            if (Vector3.Distance(ctx.GetTargetLeader().transform.position, ctx.transform.position) < 2)
            {
                ctx.GetTargetLeader().GetComponent<GuardContext>().alerted = true;
                ctx.GetTargetLeader().GetComponent<GuardContext>().SetSearchWaypoint(ctx.lastPlayerSighting);
                return AivoTreeStatus.Success;
            }
            else
            {
                return AivoTreeStatus.Success;
            }
            
        });

        var FindWaypointNearLeader = new ActionNode<GuardContext>((timeTick, ctx) =>
       {
           if (ctx.guardClass == GuardClass.SNEAKY)
           {
               Debug.Log("Sneakies are informers");
           }
           return ctx.DetermineSearchWaypoint(ctx.GetTargetLeader().transform.position, ctx.GetSearchRadius());
       });

        //Sneaky Search Nodes

        var IsSneaky = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.IsCorrectGuardClass(GuardClass.SNEAKY);
        });

        var FindWaypointNearPlayer = new ActionNode<GuardContext>((timeTick, GuardData) => 
        {
            return GuardData.DetermineSearchWaypoint(GameObject.FindGameObjectWithTag("Player").transform.position, GuardData.GetSearchRadius());
        });

        //Patrol Nodes

        var NextPatrolWaypoint = new ActionNode<GuardContext>((timeTick, ctx) =>
        {
            return ctx.DetermineNextPatrolWaypoint();
        });

        var MoveToPatrolWaypoint = new ActionNode<GuardContext>((timeTick, ctx) => 
        {
            return ctx.MoveToTarget(ctx.GetWaypoints()[ctx.GetTargetWaypointIndex()]);
        });

        //Sequence and Selector Nodes
        var StandardPursue = new SequenceNode<GuardContext>(isPlayerVisible, moveToPlayer);

        var LeaderPursue = new SequenceNode<GuardContext>(IsLeader, isPlayerVisible, AlertNearbyGuards, moveToPlayer);

        var Pursue = new SelectorNode<GuardContext>(LeaderPursue, StandardPursue);

        var ChaserSearch = new SequenceNode<GuardContext>(IsChaser, SearchWaypoint, MoveToSearchWaypoint);

        var LeaderSearch = new SequenceNode<GuardContext>(IsLeader, AlertNearbyGuards, SearchWaypoint, MoveToSearchWaypoint);

        var InformLeader = new SequenceNode<GuardContext>(FindNearestLeader, IsTargetLeaderAlerted, TravelToTargetLeader, AlertTargetLeader);

        var ExploreNearLeader = new SequenceNode<GuardContext>(FindWaypointNearLeader, MoveToSearchWaypoint);

        var InformerSearch = new SelectorNode<GuardContext>(InformLeader, ExploreNearLeader);

        var CheckInformerSearch = new SequenceNode<GuardContext>(IsInformer, InformerSearch);

        var SneakySearch = new SequenceNode<GuardContext>(IsSneaky, FindWaypointNearPlayer, MoveToSearchWaypoint);

        var Search = new SelectorNode<GuardContext>(ChaserSearch, LeaderSearch, CheckInformerSearch, SneakySearch);

        var Aware = new SequenceNode<GuardContext>(GuardAware, Search);

        var Patrol = new SequenceNode<GuardContext>(NextPatrolWaypoint, MoveToPatrolWaypoint);
        //Root of tree
        tree = new SelectorNode<GuardContext>(Pursue, Aware, Patrol);
    }

    // Update is called once per frame
    public void Update()
    {
        bool chase = false;
        var status = tree.Tick((long)Time.deltaTime, this.gameObject.GetComponent<GuardContext>());
        foreach (GameObject guard in guards)
        {
            //Reset level if player is caught
            if (Vector3.Distance(guard.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < 1)
            {
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
            }
            if (guard.GetComponent<GuardContext>().IsPlayerVisible() == AivoTreeStatus.Success)
            {
                chase = true;
            }
        }
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMusic>().chase = chase;
    }
    
}

