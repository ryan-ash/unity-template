using System.Collections.Generic;
using UnityEngine;

public class MusicGroup : AudioGroup {
    [Header("Fade")]
    public bool fadeEnabled = false;
    public float fadeLength = 0f;
    public string defaultTrack = "";
    public LeanTweenType tween;

    private AudioElement currentMusic;

    protected override void Start()
    {
        base.Start();

        foreach (KeyValuePair<string, AudioElement> audioElement in elements)
        {
            audioElement.Value.fade = new FadeConfig(fadeEnabled, fadeLength, tween);
        }

        mute = !PlayerPrefsWrapper.MusicEnabled;

        if (defaultTrack != "")
        {
            Play(defaultTrack);
        }
    }

    public override bool Play(string key)
    {
        if (!base.Play(key))
        {
            return false;
        }

        if (currentMusic != null)
        {
            currentMusic.Stop();
        }

        currentMusic = elements[key];
        return true;
    }

    public override void Mute()
    {
        base.Mute();

        if (currentMusic == null)
        {
            return;
        }

        currentMusic.Stop();
    }

    public override void UnMute()
    {
        base.UnMute();

        if (currentMusic == null)
        {
            return;
        }

        currentMusic.Play();
    }
}
