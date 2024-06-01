using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceMusicSource : MusicSource
{
    public Collider area;

    public float fadeDistance;

    public override float GetVolume(Vector3 listenerPosition) {
        if (!area.gameObject.activeInHierarchy) {
            return 0;
        }
        
        Vector3 closest = area.ClosestPoint(listenerPosition);
        float distance = Vector3.Distance(closest, listenerPosition);
        return 1-Mathf.Clamp01(distance/fadeDistance);
    }
}
