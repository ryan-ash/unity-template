using System.Collections.Generic;
using UnityEngine;

public class AudioGroup : MonoBehaviour {
    public Dictionary<string, AudioElement> elements;

    public bool mute {
        set
        {
            if (value)
            {
                Mute();
                return;
            }

            UnMute();
        }
    }

    protected virtual void Start()
    {
        AudioElement tempAudioElement;
        elements = new Dictionary<string, AudioElement>();
        foreach (Transform audioElementTransform in transform)
        {
            tempAudioElement = audioElementTransform.GetComponent<AudioElement>();
            tempAudioElement.Init();
            elements[audioElementTransform.gameObject.name] = tempAudioElement;
        }
    }

    public virtual bool Play(string key)
    {
        if (!elements.ContainsKey(key))
        {
            return false;
        }

        elements[key].Play();
        return true;
    }

    public virtual bool Stop(string key)
    {
        if (!elements.ContainsKey(key))
        {
            return false;
        }

        elements[key].Stop();
        return true;
    }

    public virtual AudioElement GetElementByName(string key)
    {
        AudioElement result = null;

        if (elements.ContainsKey(key))
        {
            result = elements[key];
        }

        return result;        
    }

    public virtual void Mute()
    {
        foreach (KeyValuePair<string, AudioElement> audioElement in elements)
        {
            audioElement.Value.Mute();
        }
    }

    public virtual void UnMute()
    {
        foreach (KeyValuePair<string, AudioElement> audioElement in elements)
        {
            audioElement.Value.UnMute();
        }
    }
}
