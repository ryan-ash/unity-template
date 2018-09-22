using System.Collections.Generic;
using UnityEngine;

public class MusicGroup : AudioGroup {
    public bool multitrack = false;
    [Header("Fade")]
    public bool fadeEnabled = false;
    public float fadeLength = 0f;
    public string[] defaultTracks;
    public LeanTweenType tween;

    private List<AudioElement> currentMusic;

    protected override void Start()
    {
        base.Start();

        currentMusic = new List<AudioElement>();

        foreach (KeyValuePair<string, AudioElement> audioElement in elements)
        {
            audioElement.Value.fade = new FadeConfig(fadeEnabled, fadeLength, tween);
        }

        mute = !PlayerPrefsWrapper.MusicEnabled;

        if (defaultTracks != null && defaultTracks.Length > 0)
        {
            foreach (string defaultTrack in defaultTracks)
                Play(defaultTrack);
        }
    }

    public override bool Play(string key)
    {
        if (!base.Play(key))
        {
            return false;
        }

        if (currentMusic.Count > 0 && !multitrack)
        {
            currentMusic[0].Stop();
            currentMusic.RemoveAt(0);
        }

        Unqueue(key);
        currentMusic.Add(elements[key]);

        return true;
    }

    public override bool Stop(string key)
    {
        if (!base.Stop(key))
        {
            return false;
        }

        Unqueue(key);

        return true;
    }

    public override void Mute()
    {
        base.Mute();

        if (currentMusic == null || currentMusic.Count == 0)
        {
            return;
        }

        foreach(AudioElement activeTrack in currentMusic)
        {
            activeTrack.Stop();
        }
    }

    public override void UnMute()
    {
        base.UnMute();

        if (currentMusic == null || currentMusic.Count == 0)
        {
            return;
        }

        foreach(AudioElement activeTrack in currentMusic)
        {
            activeTrack.Play();
        }
    }

    private void Unqueue(string key)
    {
        int trackIndex = currentMusic.IndexOf(elements[key]);

        if (trackIndex != -1)
            currentMusic.RemoveAt(trackIndex);
    }
}
