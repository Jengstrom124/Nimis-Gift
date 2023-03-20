using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingManager : MonoBehaviour
{
    [Header("Breathing Settings")]
    [Tooltip("Timer in seconds")]
    public float inhaleTimer;
    public float pauseTimer, exhaleTimer;
    public float targetDuration = 30f;

    [Header("Debug/Refernces: ")]
    [SerializeField] bool beginOnStart = false;
    [SerializeField] bool breathingInProgress = false;
    [SerializeField] float breathingTimer;
    [SerializeField] bool inhale, pause, exhale = false;


    private void Start()
    {
        if(beginOnStart)
            StartCoroutine(BreathingExcerciseCoroutine());
    }

    IEnumerator BreathingExcerciseCoroutine()
    {
        breathingInProgress = true;
        breathingTimer = 0f;

        do
        {
            //Inhale
            inhale = true;

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            inhale = false;
            pause = true;

            yield return new WaitForSeconds(pauseTimer);

            //exhale
            pause = false;
            exhale = true;

            yield return new WaitForSeconds(exhaleTimer);

            exhale = false;
        }
        while (breathingTimer < targetDuration);

        breathingInProgress = false;
    }

    private void Update()
    {
        if (breathingInProgress)
            breathingTimer += Time.deltaTime;
    }
}
