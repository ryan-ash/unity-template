using System.Collections.Generic;

public class SFXGroup : AudioGroup {
    protected override void Start()
    {
        base.Start();

        foreach (KeyValuePair<string, AudioElement> audioElement in elements)
        {
            audioElement.Value.fade = new FadeConfig();
        }

        mute = !PlayerPrefsWrapper.SoundFXEnabled;
    }
}
