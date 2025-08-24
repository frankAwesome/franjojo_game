using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class Pig : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Animator Anim;
    public LMNT.LMNTSpeech Speech;

    public int NpcId = 1; // Example NPC ID, adjust as needed
    public House PigHouse;
    public House BackupHouse;
    public bool IsAwaitingResponse = false;
    public PlayableDirector PlayableDirectorBlowUp;

    private void Start()
    {
        SpeechManager.Instance.OnSpeechComplete.AddListener(OnSpeechComplete);
        FraJoJoApi.Instance.OnDialogResponseReceived += DialogResponse;
        Speech.OnAudioPlayed.AddListener(OnSpeechTalkStarted);
        
    }

    private void DialogResponse(GetDialogResponseModel resp)
    {
        if (!IsAwaitingResponse) return;
        IsAwaitingResponse = false;

        // Check if dialogResponse contains a colon
        if (resp.response.dialogResponse.Contains(":"))
        {
            // Split the dialogResponse by colon and take the second part
            var parts = resp.response.dialogResponse.Split(':');
            if (parts.Length > 1)
            {
                Speech.dialogue = parts[1].Trim(); // Set to the second part, trimmed of whitespace
            }
        }
        else
        {
            Speech.dialogue = resp.response.dialogResponse; // Fallback to the original response
        }

        StartCoroutine(Speech.Talk());

        Debug.Log($"Dialog response received: {resp.response.matchedMilestone}");
        if (resp.response.matchedMilestone != "")
        {            
            if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.ApproachStrawHouse)
            {
                if (resp.response.matchedMilestone == "2" && GameStateManager.Instance.ActiveChapterId == 1)
                {
                    //trigger wolf blow scene
                    //
                    Debug.Log($"Milestone matched: {resp.response.matchedMilestone} - Triggering wolf blow scene");
                    GameStateManager.Instance.CurrentState = GameStateManager.GameState.StrawHouse;
                    Invoke(nameof(DelayedBlowup), 5f);
                }
                else
                {
                    //trigger wolf talk
                    //
                    Debug.Log($"Milestone matched: {resp.response.matchedMilestone} - Continuing with wolf talk");
                    FindFirstObjectByType<Wolf>().AwaitSpeech();
                }
            }
            else
            {

            }

            //if (milestone != null)
            //{
            //    Debug.Log($"Milestone matched: {milestone.name} (ID: {milestone.id})");
            //    milestone.completed = true;
            //    //trigger wolf blow scene
            //}
        } else
        {
            if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.ApproachStrawHouse)
            {
                Debug.Log("No milestone matched, continuing with wolf talk.");
                FindFirstObjectByType<Wolf>().AwaitSpeech();
            }
            else
            {
                Debug.Log("No milestone matched, but not in approach state.");            
            }
        }
    }

    public void DelayedBlowup()
    {
        PlayableDirectorBlowUp.Play();
    }

    private void OnSpeechTalkStarted()
    {
        Anim.SetTrigger("Talk");
    }

    private void OnSpeechComplete(string arg0)
    {
        
    }
    
    public void RunToBackupHouse()
    {        
        Anim.SetBool("IsRunning", true);
        Agent.SetDestination(BackupHouse.transform.position);
    }

    public void RunToHouse()
    {
        Debug.Log("Running to pig house: " + PigHouse.HouseDoorPosition.transform.position);
        Agent.SetDestination(PigHouse.transform.position);                    
    }

    public void RandomSpeech(string message)
    {
        if (IsAwaitingResponse) return; // Prevent multiple requests while waiting for a response
        IsAwaitingResponse = true;
        FraJoJoApi.Instance.GetGameStorieParams(1, NpcId, message); // Example active chapter ID 
        //Speech.dialogue = "Get away you fucking ugly wolf ass bitch!";        
        //Speech.dialogue = "You don't know who you playing with. Get your bitch ass out of here before I beat your ass!";        
        //StartCoroutine(Speech.Talk());        
    }

    public void RandomSpeech()
    {
        if (IsAwaitingResponse) return; // Prevent multiple requests while waiting for a response
        IsAwaitingResponse = true;

        var word = "Hi";
        if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.StrawHouse)
        {
            word = "Your house is blown up. Run to your brother!";
        }

        FraJoJoApi.Instance.GetGameStorieParams(1, NpcId, word); // Example active chapter ID 
    }


}
