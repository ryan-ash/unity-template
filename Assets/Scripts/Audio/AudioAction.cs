using System.Collections;
using UnityEngine;

public enum AudioActionType { Play, Stop, Reset, SetTo, SelfDestruct }

public class AudioAction : MonoBehaviour {
    public float delay = 0f;
    public int intValue = 0;
    public string subTargetName = "";
    public string targetName;
    public AudioActionType type;
    public AudioGroup targetSpace;

    public void Fire()
    {
        if (delay == 0f)
        {
            Shoot();
            return;
        }

        StartCoroutine("WaitAndShoot");
    }

    private void Shoot()
    {
        if (subTargetName != "")
        {
            AudioElement element = targetSpace.GetElementByName(targetName);
            int sequenceId = element.GetIDByKey(subTargetName);    
            element.sequence.SetTo(sequenceId);
        }

        switch (type)
        {
            case AudioActionType.Stop:
                targetSpace.Stop(targetName);
                break;
            case AudioActionType.Reset:
                targetSpace.GetElementByName(targetName).sequence.Reset();
                break;
            case AudioActionType.SetTo:
                targetSpace.GetElementByName(targetName).sequence.SetTo(intValue);
                break;
            case AudioActionType.SelfDestruct:
                break;
            case AudioActionType.Play:
            default:
                targetSpace.Play(targetName);
                break;
        }

        Die();
    }

    private void Die()
    {
        Destroy(this);
    }

    IEnumerator WaitAndShoot()
    {
        yield return new WaitForSeconds(delay);
        Shoot();
    }
}
