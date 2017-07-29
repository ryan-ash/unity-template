using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour {
    
    public AudioGroup musicGroupComponent;
    public AudioGroup soundFXGroupComponent;

    private string[] consecutiveTriggersDelimiter = new string[] { "//" };
    private char triggerElementsDelimiter = '/';

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
        soundFXGroupComponent.mute = !b;
    }

    public void FireTrigger(string triggerLine)
    {
        string[] consecutiveTriggers = triggerLine.Split(consecutiveTriggersDelimiter, StringSplitOptions.None);
        ParseTriggers(consecutiveTriggers);
    }
    
    private void ParseTriggers(string[] consecutiveTriggers)
    {
        AudioAction currentAction;

        foreach (string trigger in consecutiveTriggers)
        {
            currentAction = GenerateAction(trigger.Split(triggerElementsDelimiter));
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
                i++;
                goto default;
            default:
                action.targetSpace = soundFXGroupComponent;
                break;
        }

        // then we pick the target
        action.targetName = cylinder[i];

        // and check if it exists
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
