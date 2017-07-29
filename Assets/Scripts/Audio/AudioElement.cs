using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioElement : MonoBehaviour {
    public bool noFadeOnStart = false;
    public bool noFadeOnStop = false;
    public float offset = 0f;
    public AudioSequenceType sequenceType = AudioSequenceType.None;

    [HideInInspector]
    public AudioSequence sequence;

    [HideInInspector]
    public FadeConfig fade;

    [Header("Callbacks")]
    public GameObject onPlay;
    public GameObject onStop;

    private bool mute = false;
    private AudioSource[] sequenceSources;
    private AudioSource currentSource;
    private Dictionary<string, int> sequenceNameTable;

    public int GetIDByKey(string key)
    {
        if (!sequenceNameTable.ContainsKey(key))
        {
            return -1;
        }

        return sequenceNameTable[key];
    }

    public void Play()
    {
        if (onPlay != null)
        {
            onPlay.SendMessage("Play");
        }

        currentSource = sequenceSources[sequence.Next()];
        // CustomDebug.Log("AudioManager :: playing " + currentSource.gameObject.name + " of " + gameObject.name + " audio element");

        if (mute)
        {
            return;
        }

        if (offset == 0)
        {
            StartPlaying();
            return;
        }

        StartCoroutine("WaitAndPlay");
    }

    public void StartPlaying()
    {
        if (fade.enabled)
        {
            if (noFadeOnStart)
            {
                currentSource.volume = 1f;            
            }
            else
            {
                currentSource.volume = 0f;
                LeanTween.value(currentSource.gameObject, 
                    (float newVolume) => 
                    { 
                        currentSource.volume = newVolume; 
                    },
                    0f, 1f, fade.length
                ).setEase(fade.tween);
            }
        }

        currentSource.Play();
    }

    public void Stop()
    {
        if (onStop != null)
        {
            onStop.SendMessage("Play");
        }

        if (currentSource == null)
        {
            return;
        }

        // CustomDebug.Log("AudioManager :: stopping " + currentSource.gameObject.name + " of " + gameObject.name + " audio element");

        if (fade.enabled)
        {
            if (noFadeOnStop)
            {
                currentSource.volume = 0;
            }
            else
            {
                LeanTween.value(currentSource.gameObject, 
                    (float newVolume) => 
                    { 
                        currentSource.volume = newVolume; 
                    },
                    currentSource.volume, 0, fade.length
                ).setEase(fade.tween).setOnComplete(
                    () =>
                    {
                        currentSource.Stop();
                    }
                );

                return;
            }
        }

        currentSource.Stop();
    }

    public void Mute()
    {
        mute = true;
    }

    public void UnMute()
    {
        mute = false;
    }

    public void Init()
    {
        int sequenceLength = 0;

        if (sequenceType == AudioSequenceType.None)
        {
            sequenceSources = new AudioSource[1] { GetComponent<AudioSource>() };
        }
        else
        {
            sequenceSources = GetComponentsInChildren<AudioSource>();
            sequenceLength = sequenceSources.Length;
        }

        sequenceNameTable = new Dictionary<string, int>();
        for (int i = 0; i < sequenceSources.Length; i++)
        {
            sequenceNameTable[sequenceSources[i].gameObject.name] = i;
        }

        sequence = new AudioSequence(sequenceType, sequenceLength);
    }

    IEnumerator WaitAndPlay()
    {
        yield return new WaitForSeconds(offset);
        StartPlaying();
    }
}
