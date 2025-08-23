using System;
using UnityEngine;
using UnityEngine.AI;

public class Pig : MonoBehaviour
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

    private void OnSpeechComplete(string arg0)
    {
        
    }

    public void RandomSpeech()
    {
        Speech.dialogue = "Get away you fucking ugly wolf ass bitch!";        
        StartCoroutine(Speech.Talk());        
    }
}
