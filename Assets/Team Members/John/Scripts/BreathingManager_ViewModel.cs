using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class BreathingManager_ViewModel : MonoBehaviour
{
    [Header("UI: ")]
    public TMP_Text debugText;
    public GameObject nimiUIIcon;
    public Image breathingUIBackdrop, breathingUIBackdrop2;

    [Header("Audio: ")]
    public AudioSource breathingAudioSource;
    public AudioClip stage1InhaleAudio, stage1ExhaleAudio, stage2InhaleAudio, stage2ExhaleAudio;

    [Header("Particles: ")]
    public GameObject ambientParticlesGO;
    public ParticleSystem ambientParticles1, ambientParticles2;
    public float particleFadeValue = 0.1f;
    public Animator ambientParticles2Animator;

    BreathingManager breathingManager;
    void Start()
    {
        debugText.text = "";

        breathingManager = BreathingManager.instance;

        breathingManager.onInhaleEvent += OnInhale;
        breathingManager.onExhaleEvent += OnExhale;
        breathingManager.onHoldAfterInhaleEvent += OnHoldAfterInahle;
        breathingManager.onHoldAfterExhaleEvent += OnHoldAfterExhale;

        breathingManager.onUnpauseParticlesHackEvent += PauseEnvironmentParticles;
        breathingManager.clearTextHackEvent += ClearText;

        breathingManager.onBreathingStartedEvent += ClearText;
        breathingManager.onBreathingFinishedEvent += UpdateBreathingAudioSourceHack;
    }

    void ClearText()
    {
        debugText.text = "";
    }
    void UpdateBreathingAudioSourceHack()
    {
        breathingManager.onBreathingFinishedEvent -= UpdateBreathingAudioSourceHack;

        breathingAudioSource.volume = 0.13f;
    }

    void OnInhale()
    {
        //Init
        breathingAudioSource.Stop();
        debugText.text = "Inhale";

        //Audio
        if (!breathingManager.tutorialComplete)
            breathingAudioSource.clip = stage1InhaleAudio;
        else
            breathingAudioSource.clip = stage2InhaleAudio;

        breathingAudioSource.Play();

        //Tween BreathingUI
        MoveUIRef("x", 0.88f, breathingManager.inhaleTimer);
        breathingUIBackdrop.CrossFadeColor(new Color(1, 1, 1, 1), breathingManager.inhaleTimer, true, true);
        breathingUIBackdrop2.CrossFadeColor(new Color(1, 1, 1, 1), breathingManager.inhaleTimer, true, true);
        iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one * 1.175f, "easetype", iTween.EaseType.easeInOutSine, "time", breathingManager.inhaleTimer));

        //Fade Particles In/Unpause
        iTween.FadeTo(ambientParticlesGO, 1f, breathingManager.inhaleTimer);
        if (breathingManager.tutorialComplete)
            ambientParticles2Animator.Play("AmbientParticleGlow_FadeIn");
        if (breathingManager.pauseEnvironmentParticles)
        {
            PauseEnvironmentParticles(false);
        }
    }
    void OnHoldAfterInahle()
    {
        //Init
        debugText.text = "Hold";

        //Tween UI Ref
        MoveUIRef("y", -0.88f, breathingManager.pauseTimer);

        //Pause Particles
        if (breathingManager.pauseEnvironmentParticles)
        {
            PauseEnvironmentParticles(true);
        }
    }

    void OnExhale()
    {
        //Init
        breathingAudioSource.Stop();
        debugText.text = "Exhale";
        breathingManager = BreathingManager.instance;

        //Audio
        if (!breathingManager.tutorialComplete)
            breathingAudioSource.clip = stage1ExhaleAudio;
        else
            breathingAudioSource.clip = stage2ExhaleAudio;

        breathingAudioSource.Play();

        //Tween BreathingUI
        MoveUIRef("x", -0.88f, breathingManager.exhaleTimer);
        breathingUIBackdrop.CrossFadeColor(new Color(1, 1, 1, 0), BreathingManager.instance.exhaleTimer, true, true);
        breathingUIBackdrop2.CrossFadeColor(new Color(1, 1, 1, 0), BreathingManager.instance.exhaleTimer, true, true);
        iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one, "easetype", iTween.EaseType.easeOutSine, "time", BreathingManager.instance.exhaleTimer));

        //Fade Particles Out/Unpause
        iTween.FadeTo(ambientParticlesGO, particleFadeValue, breathingManager.exhaleTimer);
        if (breathingManager.tutorialComplete)
            ambientParticles2Animator.Play("AmbientParticleGlow_FadeOut");
        if (breathingManager.pauseEnvironmentParticles)
        {
            PauseEnvironmentParticles(false);
        }
    }
    void OnHoldAfterExhale()
    {
        //Init
        debugText.text = "Hold";

        //Tween
        MoveUIRef("y", 0.88f, breathingManager.pauseTimer);

        //Pause Particles
        if (breathingManager.pauseEnvironmentParticles)
        {
            PauseEnvironmentParticles(true);
        }
    }

    void MoveUIRef(string axis, float pos, float timer)
    {
        iTween.MoveTo(nimiUIIcon, iTween.Hash(axis, pos, "islocal", true, "easetype", iTween.EaseType.easeInOutSine, "time", timer));
    }
    void PauseEnvironmentParticles(bool pause)
    {
        if (pause)
        {
            ambientParticles1.Pause();

            if (ambientParticles2.isPlaying)
            {
                ambientParticles2.Pause();
            }

        }
        else
        {
            ambientParticles1.Play();

            if (ambientParticles2.isPaused)
            {
                ambientParticles2.Play();
            }
        }
    }

}
