using UnityEngine;
using UnityEngine.EventSystems;

public class MindTree : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (NimiExperienceManager.instance.canInteractWithTree)
        {
            NimiExperienceManager.instance.UIHack(true);
            BreathingManager.instance.BeginBreathingExercise();
        }
        else
            Debug.Log("Interaction Unavailable");
    }
}
