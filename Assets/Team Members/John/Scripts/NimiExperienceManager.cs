using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Avatars;
using Liminal.SDK.VR.Input;

public class NimiExperienceManager : MonoBehaviour
{
    public static NimiExperienceManager instance;
    public float introDialogueDelayTime = 3f;
    public float environmentDialogueDelayTime = 2f;
    public float environmentFadeTime = 2f;

    [Header("Nimi: ")]
    public Animator nimiAnimator;
    public AudioSource nimiAudioSource;

    [Header("Dialogue Sequences")]
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
    public Transform dialogueCanvas;
    public bool canInteractWithTree = false;

    [Header("Debugs: ")]
    public bool testFirstEnvironmentAddition = false;
    IVRInputDevice leftInput, rightInput;

    public event Action onTreeRevealEvent;

    private void Awake()
    {
        instance = this;

        if(environmentLight.intensity != 0)
        {
            topLight.intensity = 0f;
            bottomLight.intensity = 0f;
            environmentLight.intensity = 0f;
        }

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

        if(testFirstEnvironmentAddition)
        {
            Debug.Log("Testing Environment Additions");
            EnableFirstEnvironmentAddition();
        }

        var avatar = VRAvatar.Active;

        rightInput = GetInput(VRInputDeviceHand.Right);
        leftInput = GetInput(VRInputDeviceHand.Left);
        UpdateInteractableState();

        BreathingManager.instance.onBreathingStartedEvent += UpdateInteractableState;
    }
    private IVRInputDevice GetInput(VRInputDeviceHand hand)
    {
        var device = VRDevice.Device;
        return hand == VRInputDeviceHand.Left ? device.SecondaryInputDevice : device.PrimaryInputDevice;
    }
    void UpdateInteractableState()
    {
        //VRDevice.Device?.PrimaryInputDevice?.Pointer?.Deactivate();
        rightInput.Pointer.Deactivate();
        //leftInput.Pointer.Deactivate();
        //VRDevice.Device?.SecondaryInputDevice?.Pointer?.Deactivate();
        canInteractWithTree = false;
    }

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

    [SerializeField] bool fadeLights = false;
    IEnumerator MindTreeSequenceCoroutine()
    {

        yield return new WaitForSeconds(1f);

        //Fade Environment In
        //environmentLights.SetActive(true);
        fadeLights = true;
        //onTreeRevealEvent?.Invoke();
        //iTween.FadeTo(mindTreeEnvironment, 1f, 5f);
        /*foreach (GameObject light in environmentLights)
        {
            light.SetActive(true);
        }*/

        //Begin Next Dialogue Sequence
        dialogueCanvas.position = new Vector3(-4.45f, -3f, -2.68f);
        dialogueCanvas.rotation = Quaternion.Euler(0, -40f, 0);

        yield return new WaitForSeconds(environmentDialogueDelayTime);

        mindTreeDialogue.Interact();
        DialogueManager.instance.onDialogueFinishEvent += InitBreathingTutorial;
    }

    void InitBreathingTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= InitBreathingTutorial;

        BreathingManager.instance.UpdateBreathingUIState(1f);
        StartCoroutine(InitTutorialDialogueCoroutine());

        DialogueManager.instance.onDialogueFinishEvent += BreathingManager.instance.BeginBreathingExerciseTutorial;
        BreathingManager.instance.onBreathingFinishedEvent += EndTutorial;
    }
    IEnumerator InitTutorialDialogueCoroutine()
    {
        yield return new WaitForSeconds(BreathingManager.instance.nimiFadeDuration + 2.5f);

        breathingTutorialDialogue.Interact();
    }

    void EndTutorial()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= EndTutorial;

        postBreathingTutorialDialgoue.Interact();

        //Setup tree interaction
        DialogueManager.instance.onDialogueFinishEvent += EnableTreeInteraction;

        //Fake tree interaction for now
        //DialogueManager.instance.onDialogueFinishEvent += BeginBreathingExercisePlaceholder;
    }

    void EnableTreeInteraction()
    {
        DialogueManager.instance.onDialogueFinishEvent -= EnableTreeInteraction;

        canInteractWithTree = true;
        //UIHack(false);
        //BreathingManager.instance.onBreathingFinishedEvent += UpdateUIHack;
        VRDevice.Device?.PrimaryInputDevice?.Pointer?.Activate();
        VRDevice.Device?.SecondaryInputDevice?.Pointer?.Activate();

        BreathingManager.instance.onBreathingFinishedEvent += EnableFirstEnvironmentAddition;
    }

    void BeginBreathingExercisePlaceholder()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginBreathingExercisePlaceholder;

        BreathingManager.instance.BeginBreathingExercise();
    }

    void EnableFirstEnvironmentAddition()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= EnableFirstEnvironmentAddition;

        /*TerrainData terrainData = treeTerrain.terrainData;
        terrainData.wavingGrassSpeed = 0.25f;
        treeTerrain.terrainData = terrainData;*/
        //treeTerrain.terrainData.wavingGrassSpeed = 0.25f;
        fireflies.Play();
        moonRays.Play();
        fallingLeaves.Play();
        iTween.AudioTo(nightAmbience.gameObject, iTween.Hash("audiosource", nightAmbience,"volume", 0.4f, "easetype", iTween.EaseType.easeInOutSine ,"time", 10f));
    }

    [SerializeField] float elapsedTime = 0f;
    private void Update()
    {
        if(fadeLights)
        {
            if (elapsedTime < environmentFadeTime)
            {
                topLight.intensity = Mathf.Lerp(0, 9.2f, elapsedTime / environmentFadeTime);
                bottomLight.intensity = Mathf.Lerp(0, 7.7f, elapsedTime / environmentFadeTime);
                environmentLight.intensity = Mathf.Lerp(0, 0.75f, elapsedTime / environmentFadeTime);

            }
            else
            {
                topLight.intensity = 9.2f;
                bottomLight.intensity = 7.7f;
                environmentLight.intensity = 0.75f;
                fadeLights = false;
            }

            elapsedTime += Time.deltaTime;
        }
    }

    //Hacking turning on & off the UI because the UI canvas are blocking the raycast/interaction from reaching the tree
    void UpdateUIHack()
    {
        UIHack(false);
    }
    public void UIHack(bool turnUIOn)
    {
        dialogueCanvas.gameObject.SetActive(turnUIOn);
        //breathingGO.SetActive(turnUIOn);
    }
}
