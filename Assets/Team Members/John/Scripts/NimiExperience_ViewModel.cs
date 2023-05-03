using UnityEngine;

public class NimiExperience_ViewModel : MonoBehaviour
{
    [Header("Environment Lights")]
    public Light stage3ExtraRimLight;
    public Light topLight, bottomLight, rimLight;

    [Header("Environment Light Colour Config")]
    public Color stage1TopLightColour;
    public Color stage1BottomLightColour, stage1RimLightColour;
    public Color stage2TopLightColour, stage2BottomLightColour, stage2RimLightColour;
    public Color stage3TopLightColour, stage3BottomLightColour, stage3RimLightColour, stage3ExtraRimColour;
    [HideInInspector]
    public Color topLightStartColour, bottomLightStartColour, rimLightStartColour; //environmentLightStartValue;

    [Header("Breathing Stage Environment Light Config")]
    [Tooltip("The colour the lights will fade into during the breathing stage so they aren't as bright as the default environment")]
    public Color topLightBreatheFadeInColour;
    [Tooltip("The colour the lights will fade into during the breathing stage so they aren't as bright as the default environment")]
    public Color bottomLightBreatheFadeInColour, stage2TopLightBreatheFadeInColour, stage2BottomLightBreatheFadeInColour;

    [Header("First Environment Additions")]
    public ParticleSystem fireflies;
    public ParticleSystem moonRays, fallingLeaves, glowAmbientParticles;
    public AudioSource cricketAmbience, owlAmbiene, windAmbience, windFlutesAmbience;
    public GameObject constellations;
    public Material moonSkybox;

    [Header("Second Environment Additions")]
    public GameObject aurora;
    public AudioSource auroraAudioSource;
    float stage3LightTransitionTimer;

    float environmentFadeTime;

    private void Awake()
    {
        topLightStartColour = topLight.color;
        bottomLightStartColour = bottomLight.color;
        rimLightStartColour = rimLight.color;

        topLight.color = Color.black;
        bottomLight.color = Color.black;
        rimLight.color = Color.black;
    }
    void Start()
    {
        NimiExperienceManager.instance.onRevealMindTreeEvent += MindTreeIntroSequence;
        NimiExperienceManager.instance.onStage1EnvironmentEvent += Stage1EnvironmentUpgrade;
        NimiExperienceManager.instance.onStage2EnvironmentEvent += Stage2EnvironmentUpgrade;

        BreathingManager.instance.onFadeBreathingLightsInEvent += FadeLights;

        stage3LightTransitionTimer = NimiExperienceManager.instance.stage3LightTransitionTimer;
        environmentFadeTime = NimiExperienceManager.instance.environmentFadeTime;
    }

    void MindTreeIntroSequence()
    {
        NimiExperienceManager.instance.onRevealMindTreeEvent -= MindTreeIntroSequence;

        //Fade Environment In
        windFlutesAmbience.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", windFlutesAmbience, "volume", 1f, "easetype", iTween.EaseType.easeInOutSine, "time", 3f));

        iTween.ColorTo(topLight.gameObject, stage1TopLightColour, environmentFadeTime);
        iTween.ColorTo(bottomLight.gameObject, stage1BottomLightColour, environmentFadeTime);
        iTween.ColorTo(rimLight.gameObject, stage1RimLightColour, environmentFadeTime);
    }

    void Stage1EnvironmentUpgrade()
    {
        NimiExperienceManager.instance.onStage1EnvironmentEvent -= Stage1EnvironmentUpgrade;

        fallingLeaves.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", windAmbience, "volume", 0.5f, "easetype", iTween.EaseType.easeInOutSine, "time", 4f));
        fireflies.Play();
        moonRays.Play();
        iTween.AudioTo(cricketAmbience.gameObject, iTween.Hash("audiosource", cricketAmbience, "volume", 0.48f, "easetype", iTween.EaseType.easeInOutSine, "time", 12f));
        owlAmbiene.Play();
        RenderSettings.skybox = moonSkybox;
        glowAmbientParticles.Play();
        constellations.SetActive(true);
    }

    void Stage2EnvironmentUpgrade()
    {
        NimiExperienceManager.instance.onStage2EnvironmentEvent -= Stage2EnvironmentUpgrade;

        //Begin Aurora Here
        aurora.SetActive(true);
        auroraAudioSource.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", auroraAudioSource, "volume", 0.45f, "easetype", iTween.EaseType.easeInOutSine, "time", 7f));

        //Tween Environment Lights
        stage3ExtraRimLight.gameObject.SetActive(true);
        iTween.ColorTo(topLight.gameObject, stage3TopLightColour, stage3LightTransitionTimer);
        iTween.ColorTo(bottomLight.gameObject, stage3BottomLightColour, stage3LightTransitionTimer);
        iTween.ColorTo(rimLight.gameObject, stage3RimLightColour, stage3LightTransitionTimer);
        iTween.ColorTo(stage3ExtraRimLight.gameObject, stage3ExtraRimColour, stage3LightTransitionTimer);
    }

    void FadeLights(bool fadeIn, float timer)
    {
        if (fadeIn)
        {
            //Different Light Intensity during breathing
            if (BreathingManager.instance.breathingInProgress)
            {
                if (BreathingManager.instance.inTutorial)
                {
                    iTween.ColorTo(topLight.gameObject, topLightBreatheFadeInColour, timer);
                    iTween.ColorTo(bottomLight.gameObject, bottomLightBreatheFadeInColour, timer);

                }
                else
                {
                    iTween.ColorTo(topLight.gameObject, stage2TopLightBreatheFadeInColour, timer);
                    iTween.ColorTo(bottomLight.gameObject, stage2BottomLightBreatheFadeInColour, timer);
                }

            }
            else
            {
                //Fade back to default environment intensity
                iTween.ColorTo(topLight.gameObject, stage2TopLightColour, timer);
                iTween.ColorTo(bottomLight.gameObject, stage2BottomLightColour, timer);
                if (rimLight.color != stage2RimLightColour)
                {
                    iTween.ColorTo(rimLight.gameObject, stage2RimLightColour, timer + 8f);
                }
            }
        }
        else
        {
            iTween.ColorTo(topLight.gameObject, Color.black, timer);
            iTween.ColorTo(bottomLight.gameObject, Color.black, timer);
        }
    }
}
