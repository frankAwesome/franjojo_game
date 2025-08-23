using System;
using UnityEngine;
using UnityEngine.AI;

public class Pig : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Animator Anim;
    public LMNT.LMNTSpeech Speech;

    public House PigHouse;
    public bool IsAwaitingResponse = false;

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
        Speech.dialogue = resp.response.dialogResponse;
        StartCoroutine(Speech.Talk());       
    }

    private void OnSpeechTalkStarted()
    {
        Anim.SetTrigger("Talk");
    }

    private void OnSpeechComplete(string arg0)
    {
        
    }

    public void RunToHouse()
    {
        if (Agent != null)
        {
            Agent.SetDestination(PigHouse.HouseDoorPosition.transform.position);            
        }
    }

    public void RandomSpeech()
    {
        if (IsAwaitingResponse) return; // Prevent multiple requests while waiting for a response
        IsAwaitingResponse = true;
        FraJoJoApi.Instance.GetGameStorieParams(1, "Hi"); // Example active chapter ID 
        //Speech.dialogue = "Get away you fucking ugly wolf ass bitch!";        
        //Speech.dialogue = "You don't know who you playing with. Get your bitch ass out of here before I beat your ass!";        
        //StartCoroutine(Speech.Talk());        
    }


}
