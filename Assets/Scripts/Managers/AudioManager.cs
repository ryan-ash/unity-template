using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour {
    
    public Transform audioRoot;

    public AudioGroup musicGroupComponent;
    public AudioGroup soundFXGroupComponent;

    private string[] consecutiveTriggersDelimiter = new string[] { "//" };
    private char triggerElementsDelimiter = '/';

    public static AudioManager instance;

    void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
    void Start() 
    {
        PlayerPrefsWrapper.toggleMusicEvent += OnToggleMusic;
        PlayerPrefsWrapper.toggleSoundFXEvent += OnToggleSoundFX;
    }

    void OnDestroy() 
    {
        PlayerPrefsWrapper.toggleMusicEvent -= OnToggleMusic;
        PlayerPrefsWrapper.toggleSoundFXEvent -= OnToggleSoundFX;
    }

    void OnToggleMusic(bool b) 
    {
        musicGroupComponent.mute = !b;
    }

    void OnToggleSoundFX(bool b) 
    {
        foreach (Transform audioSpace in audioRoot)
        {
            SFXGroup targetSpace = audioSpace.gameObject.GetComponent<SFXGroup>();
            if (targetSpace != null)
                targetSpace.mute = !b;
        }
    }

    public void PlaySound(string triggerLine)
    {
        string[] consecutiveTriggers = triggerLine.Split(consecutiveTriggersDelimiter, StringSplitOptions.None);
        ParseTriggers(consecutiveTriggers);
    }
    
    private void ParseTriggers(string[] consecutiveTriggers)
    {
        AudioAction currentAction;

        foreach (string trigger in consecutiveTriggers)
        {
            currentAction = GenerateAction(trigger.Trim().Split(triggerElementsDelimiter));
            currentAction.Fire();
        }
    }

    private AudioAction GenerateAction(string[] cylinder)
    {
        AudioAction action = gameObject.AddComponent<AudioAction>() as AudioAction;

        // we always start from space; sfx is more used thus it can be omitted
        int i = 0;
        switch (cylinder[i])
        {
            case "Music":
                action.targetSpace = musicGroupComponent;
                i++;
                break;
            case "SFX":
                action.targetSpace = soundFXGroupComponent;
                i++;
                break;
            default:
                foreach (Transform audioSpace in audioRoot) {
                    if (cylinder[i] == audioSpace.gameObject.name) {
                        AudioGroup targetSpace = audioSpace.gameObject.GetComponent<AudioGroup>();
                        if (targetSpace != null) {
                            action.targetSpace = targetSpace;
                            i++;
                            break;
                        }
                    }
                }
                if (action.targetSpace == null) {
                    // fallback to SFX
                    action.targetSpace = soundFXGroupComponent;                    
                }
                break;
        }

        // then we pick the target
        action.targetName = cylinder[i];

        // check if audio group is properly initialized
        if (action.targetSpace.elements == null)
        {
            Debug.LogWarning("AudioManager :: initialization is not complete; don't run sounds on Start or Awake, use built in DefaultTrack system");
            action.type = AudioActionType.SelfDestruct;
            return action;
        }

        // and check if the target exists
        if (action.targetSpace.GetElementByName(action.targetName) == null)
        {
            Debug.LogWarning("AudioManager :: couldn't find '" + cylinder[i] + "' in " + action.targetSpace.gameObject.name);
            action.type = AudioActionType.SelfDestruct;
            return action;
        }

        // check if cylinder is empty
        i++;
        if (i >= cylinder.Length) return action;

        // now we try to parse sub target
        if (action.targetSpace.GetElementByName(action.targetName).GetIDByKey(cylinder[i]) != -1)
        {
            action.subTargetName = cylinder[i];

            // turn the cylinder and check if it's the end
            i++;
            if (i >= cylinder.Length) return action;
        }

        // check for action, return the cylinder to previous position if we don't find it (Play will be used by default)
        switch (cylinder[i])
        {
            case "Start":
            case "Play":
                action.type = AudioActionType.Play;
                break;
            case "Stop":
                action.type = AudioActionType.Stop;
                break;
            case "Reset":
                action.type = AudioActionType.Reset;
                break;
            case "SetTo":
                action.type = AudioActionType.SetTo;
                i++;
                action.intValue = Int32.Parse(cylinder[i]) - 1;
                break;
            default:
                i--;
                break;
        }

        // turn cylinder and check if it's empty for the last time
        i++;
        if (i >= cylinder.Length) return action;

        // check for delay
        float delayInSeconds = 0f;
        bool isFloat = float.TryParse(cylinder[i], out delayInSeconds);
        if (isFloat)
        {
            action.delay = delayInSeconds;
            return action;
        }

        // report a call problem if we haven't parsed the whole cylinder by now
        Debug.LogWarning("AudioManager :: don't know what '" + cylinder[i] + "' is");

        return action;
    }
}
