using System;
using UnityEngine;
using UnityEngine.AI;

public class Wolf : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Animator Anim;
    public LMNT.LMNTSpeech Speech;

    private void Start()
    {
        SpeechManager.Instance.OnSpeechComplete.AddListener(OnSpeechComplete);
        Speech.OnAudioPlayed.AddListener(OnSpeechTalkStarted);
    }

    private void OnSpeechTalkStarted()
    {
        Anim.SetTrigger("Talk");
    }

    private void OnSpeechComplete(string message)
    {
        Talk(message);
    }

    public void MoveToHouse(string houseId)
    {
        var house = House.FindHouseById(houseId);
        Agent.SetDestination(house.HouseDoorPosition.transform.position);
        //dialog about wolf being hungry                
        Debug.Log($"Wolf is moving to {houseId} at position {house.HouseDoorPosition.transform.position}");
    }


    private void FixedUpdate()
    {
        //check if agent is moving, then set Anim to is moving
        if (Agent.velocity.magnitude > 0.1f)
        {
            Anim.SetBool("IsMoving", true);
        }
        else
        {
            Anim.SetBool("IsMoving", false);
            // If the wolf is not moving, check if it has reached its destination
            //if (Agent.remainingDistance <= Agent.stoppingDistance)
            //{
            //    // If the wolf has reached its destination, set the animation to idle
            //    //Anim.SetBool("IsIdle", true);
            //    Agent.SetDestination(transform.position); // Stop the agent from moving
            //}
            //else
            //{
            //    // If the wolf is still moving, set the animation to walking
            //    //Anim.SetBool("IsIdle", false);
            //}
        }
    }

    public void MoveToFirstSpot()
    {
        Agent.SetDestination(GameStateManager.Instance.FirstSpot.transform.position);
    }

    public void MoveToStrawHouse()
    {
        //hungry, lets check // if the straw house is the closest house
        //Speech.dialogue = "Let's go to the straw house, I'm hungry!";
        //StartCoroutine(Speech.Talk());
        SpeechManager.Instance.ToggleReadyToListen();
        MoveToHouse(GameStateManager.Instance.StrawHouseId);
    }

    public void AwaitSpeech()
    {
        SpeechManager.Instance.ToggleReadyToListen();
    }


    public void Talk(string message)
    {
        Speech.dialogue = message;
        StartCoroutine(Speech.Talk());
    }

    public void MoveToWoodHouse()
    {
        MoveToHouse(GameStateManager.Instance.WoodHouseId);
    }

    public void MoveToBrickHouse()
    {
        MoveToHouse(GameStateManager.Instance.BrickHouseId);
    }
}
