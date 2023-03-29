using UnityEngine;
using UnityEngine.EventSystems;

public class MindTree : MonoBehaviour, IPointerClickHandler
{
    public AudioSource interactAudioSource;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (NimiExperienceManager.instance.canInteractWithTree)
        {
            NimiExperienceManager.instance.UIHack(true);
            BreathingManager.instance.BeginBreathingExercise();
            interactAudioSource.Play();
        }
        else
            Debug.Log("Interaction Unavailable");
    }
}
