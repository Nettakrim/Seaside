using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
    public MusicData musicData;
    private int index;

    protected virtual void Start() {
        MusicManager.instance.AddSource(this);
    }

    protected virtual void OnDestroy() {
        MusicManager.instance.RemoveSource(this);
    }

    public virtual float GetVolume(Vector3 listenerPosition) {
        return gameObject.activeInHierarchy ? 1 : 0;
    }

    public void SetIndex(int to) {
        index = to;
    }

    public int GetIndex() {
        return index;
    }
}
