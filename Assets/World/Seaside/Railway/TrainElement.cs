using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrainElement : MonoBehaviour
{
    public abstract void SetTrackData(TrackData trackData);

    public abstract float GetLength();
}
