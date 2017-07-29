public class FadeConfig {
    public bool enabled = false;
    public float length = 0f;
    public LeanTweenType tween;

    public FadeConfig()
    {
        return;
    }

    public FadeConfig(bool enabled, float length, LeanTweenType tween)
    {
        this.enabled = enabled;
        this.length = length;
        this.tween = tween;
    }
}
