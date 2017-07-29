using UnityEngine;

public enum AudioSequenceType { None, Linear, Random, Trigger }

public class AudioSequence {
    public AudioSequenceType type;

    private int length = 0;
    private int currentPosition = 0;

    private System.Random random;

    public AudioSequence(AudioSequenceType type, int length)
    {
        this.type = type;
        this.length = length;

        if (type == AudioSequenceType.Random)
        {
            random = new System.Random();
        }
    }

    public int Next()
    {
        int result = currentPosition;

        switch (type)
        {
            case AudioSequenceType.Linear:
                currentPosition = (currentPosition < length - 1) ? currentPosition + 1 : 0;
                break;
            case AudioSequenceType.Random:
                currentPosition = random.Next(length);
                break;
            case AudioSequenceType.Trigger:
                currentPosition = (currentPosition == 0) ? 1 : 0;
                break;
        }

        return result;
    }

    public int Reset()
    {
        currentPosition = 0;
        return currentPosition;
    }

    public int SetTo(int value)
    {
        if (value < length)
        {
            currentPosition = value;
        }
        else
        {
            Reset();
        }

        return currentPosition;
    }
}
