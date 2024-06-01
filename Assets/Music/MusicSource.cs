using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
    public MusicData musicData;
    public int index;

    public virtual float GetVolume(Vector3 listenerPosition) {
        return 1f;
    }
}
