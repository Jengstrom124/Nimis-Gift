using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentLight : MonoBehaviour
{
    public Light myLight;
    public float intensityValue;

    //Debug
    public bool fadeOnStart;

    private void Awake()
    {
        myLight.intensity = 0f;
    }

    private void Start()
    {
        NimiExperienceManager.instance.onTreeRevealEvent += TweenLight;

        if (fadeOnStart)
            TweenLight();
    }

    void TweenLight()
    {
        NimiExperienceManager.instance.onTreeRevealEvent -= TweenLight;

        //Wanted to fade lights in
    }
}
