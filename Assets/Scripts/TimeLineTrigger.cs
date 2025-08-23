using UnityEngine;
using UnityEngine.Playables;

public class TimeLineTrigger : MonoBehaviour
{
    public bool HasTriggered = false;
    public GameStateManager.GameState RequiredState = GameStateManager.GameState.Start;
    public PlayableDirector TimelineToPlay;


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter called with object: {other.name}");
        var wolf = other.GetComponent<Wolf>();
        if (wolf != null && !HasTriggered && GameStateManager.Instance.CurrentState == RequiredState)
        {
            HasTriggered = true;
            if (TimelineToPlay != null)
            {
                Debug.Log($"Triggering timeline: {TimelineToPlay.name} for wolf: {wolf.name}");
                TimelineToPlay.Play();
            }
            else
            {
                Debug.LogWarning("TimelineToPlay is not assigned in TimeLineTrigger.");
            }            
        }
    }
}
