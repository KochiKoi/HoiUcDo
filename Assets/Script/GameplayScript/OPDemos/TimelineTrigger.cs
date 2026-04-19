using UnityEngine;
using UnityEngine.Playables; // Import PlayableDirector

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector playableDirector; // Assign your director in the inspector

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasPlayed)
        {
            playableDirector.Play();
            hasPlayed = true;
        }
    }

}
