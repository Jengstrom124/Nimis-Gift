using System.Collections;
using UnityEngine;
using System;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Avatars;
using Liminal.SDK.VR.Input;
using UnityEngine.Rendering;

public class NimiExperienceManager : MonoBehaviour
{
    public static NimiExperienceManager instance;
    public float introDialogueDelayTime = 3f;
    public float environmentDialogueDelayTime = 2f;
    public float environmentFadeTime = 2f;
    public Material moonSkybox;

    [Header("Nimi: ")]
    public Animator nimiAnimator;
    public AudioSource nimiAudioSource;

    [Header("Dialogue Sequences")]
    [Tooltip("Delay before breathing Exercise starts after dialogue")]
    public float firstPhaseDelay = 3f;
    public DialogueTrigger introDialogue, mindTreeDialogue;
    public DialogueTrigger breathingTutorialDialogue, postBreathingTutorialDialgoue;

    [Header("Environment Additions")]
    public ParticleSystem fireflies;
    public ParticleSystem moonRays, fallingLeaves;
    public AudioSource nightAmbience;
    //public Terrain treeTerrain;

    [Header("Hacks")]
    public GameObject mindTreeEnvironment;
    public GameObject environmentLights;
    public Light topLight, bottomLight, environmentLight;
    public float topLightStartValue, bottomLightStartValue, environmentLightStartValue;
    public Transform dialogueCanvas;

    [Header("Debugs: ")]
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLights = false;
    //public Color startingAmbientLightColour, startingAmbientEquatorColour, startingAmbientGroundColour;
    IVRInputDevice leftInput, rightInput;
    AmbientMode gradientAmbientMode;

    //public event Action onTreeRevealEvent;

    private void Awake()
    {
        instance = this;

        topLightStartValue = topLight.intensity;
        bottomLightStartValue = bottomLight.intensity;
        environmentLightStartValue = environmentLight.intensity;

        if (environmentLight.intensity != 0)
        {
            topLight.intensity = 0f;
            bottomLight.intensity = 0f;
            environmentLight.intensity = 0f;
        }

        /*startingAmbientLightColour = RenderSettings.ambientLight;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientEquatorColor = Color.black;
        RenderSettings.ambientGroundColor = Color.black;*/
        //Check this
        gradientAmbientMode = RenderSettings.ambientMode;
        RenderSettings.ambientMode = AmbientMode.Skybox;

        /*if (environmentLights.activeSelf)
            environmentLights.SetActive(false);*/

        /*foreach (GameObject light in environmentLights)
        {
            light.SetActive(false);
        }*/
    }

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager.instance.onDialogueFinishEvent += RevealMindTree;

        StartCoroutine(InitSequenceCoroutine());

        /*var avatar = VRAvatar.Active;

        rightInput = GetInput(VRInputDeviceHand.Right);
        leftInput = GetInput(VRInputDeviceHand.Left);
        UpdateInteractableState();

        BreathingManager.instance.onBreathingStartedEvent += UpdateInteractableState;*/
    }
    /*private IVRInputDevice GetInput(VRInputDeviceHand hand)
    {
        var device = VRDevice.Device;
        return hand == VRInputDeviceHand.Left ? device.SecondaryInputDevice : device.PrimaryInputDevice;
    }
    void UpdateInteractableState()
    {
        VRDevice.Device?.PrimaryInputDevice?.Pointer?.Deactivate();
        rightInput.Pointer.Deactivate();
        //leftInput.Pointer.Deactivate();
        //VRDevice.Device?.SecondaryInputDevice?.Pointer?.Deactivate();
        canInteractWithTree = false;
    }*/

    IEnumerator InitSequenceCoroutine()
    {
        yield return new WaitForSeconds(introDialogueDelayTime);

        introDialogue.Interact();
    }

    void RevealMindTree()
    {
        DialogueManager.instance.onDialogueFinishEvent -= RevealMindTree;

        //Update Nimi
        nimiAnimator.SetTrigger("OnTreeSpawn");

        StartCoroutine(MindTreeSequenceCoroutine());
    }

    IEnumerator MindTreeSequenceCoroutine()
    {
        yield return new WaitForSeconds(1f);

        //Fade Environment In
        fadeLights = true;
        //onTreeRevealEvent?.Invoke();
        //iTween.FadeTo(mindTreeEnvironment, 1f, 5f);

        //Update Dialogue Location
        dialogueCanvas.position = new Vector3(-4.45f, -3f, -2.68f);
        dialogueCanvas.rotation = Quaternion.Euler(0, -40f, 0);

        yield return new WaitForSeconds(environmentDialogueDelayTime);

        //Begin Next Dialogue Sequence
        mindTreeDialogue.Interact();
        DialogueManager.instance.onDialogueFinishEvent += InitBreathingTutorial;
    }

    void InitBreathingTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= InitBreathingTutorial;

        //Fade in Breathing UI/Fade Out Nimi
        BreathingManager.instance.UpdateBreathingUIState(1f);
        StartCoroutine(InitTutorialDialogueCoroutine());

        DialogueManager.instance.onDialogueFinishEvent += BreathingManager.instance.BeginBreathingExerciseTutorial;
        BreathingManager.instance.onBreathingFinishedEvent += EndTutorial;
    }
    IEnumerator InitTutorialDialogueCoroutine()
    {
        //Wait for UI to fade in & begin next dialogue sequence
        yield return new WaitForSeconds(BreathingManager.instance.nimiFadeDuration + 2.5f);

        breathingTutorialDialogue.Interact();
    }

    void EndTutorial()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= EndTutorial;

        //Tutorial Environment Addons
        /*TerrainData terrainData = treeTerrain.terrainData;
        terrainData.wavingGrassSpeed = 0.25f;
        treeTerrain.terrainData = terrainData;*/
        //treeTerrain.terrainData.wavingGrassSpeed = 0.25f;
        fallingLeaves.Play();

        postBreathingTutorialDialgoue.Interact();

        //Setup tree interaction
        //DialogueManager.instance.onDialogueFinishEvent += EnableTreeInteraction;

        //Start First Phase of Breathing
        DialogueManager.instance.onDialogueFinishEvent += BeginFirstPhasBreathingExercise;
    }

    /*void EnableTreeInteraction()
    {
        DialogueManager.instance.onDialogueFinishEvent -= EnableTreeInteraction;

        canInteractWithTree = true;
        //UIHack(false);
        //BreathingManager.instance.onBreathingFinishedEvent += UpdateUIHack;
        VRDevice.Device?.PrimaryInputDevice?.Pointer?.Activate();
        VRDevice.Device?.SecondaryInputDevice?.Pointer?.Activate();

        BreathingManager.instance.onBreathingFinishedEvent += EnableFirstEnvironmentAddition;
    }*/

    void BeginFirstPhasBreathingExercise()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginFirstPhasBreathingExercise;

        BreathingManager.instance.BeginBreathingExercise(firstPhaseDelay);
        BreathingManager.instance.onBreathingFinishedEvent += PostFirstPhaseBreathing;
    }
    public void PostFirstPhaseBreathing()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= PostFirstPhaseBreathing;

        //Update Environment
        EnableFirstEnvironmentAddition();

        //Start Dialogue

        //Start new breathing after dialogue
    }

    void EnableFirstEnvironmentAddition()
    {
        fireflies.Play();
        moonRays.Play();
        iTween.AudioTo(nightAmbience.gameObject, iTween.Hash("audiosource", nightAmbience, "volume", 0.4f, "easetype", iTween.EaseType.easeInOutSine, "time", 10f));
        RenderSettings.skybox = moonSkybox;
        RenderSettings.ambientMode = gradientAmbientMode;
        /*RenderSettings.ambientLight = startingAmbientLightColour;
        RenderSettings.ambientEquatorColor = startingAmbientEquatorColour;
        RenderSettings.ambientGroundColor = startingAmbientGroundColour;*/
    }

    private void Update()
    {
        if(fadeLights)
        {
            if (elapsedTime < environmentFadeTime)
            {
                topLight.intensity = Mathf.Lerp(0, topLightStartValue, elapsedTime / environmentFadeTime);
                bottomLight.intensity = Mathf.Lerp(0, bottomLightStartValue, elapsedTime / environmentFadeTime);
                environmentLight.intensity = Mathf.Lerp(0, environmentLightStartValue, elapsedTime / environmentFadeTime);

            }
            else
            {
                topLight.intensity = topLightStartValue;
                bottomLight.intensity = bottomLightStartValue;
                environmentLight.intensity = environmentLightStartValue;
                fadeLights = false;
            }

            elapsedTime += Time.deltaTime;
        }
    }

    public void UpgradeEnvironmentDebug()
    {
        EnableFirstEnvironmentAddition();
    }
}
