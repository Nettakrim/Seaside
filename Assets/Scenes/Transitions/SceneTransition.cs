using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneTransition : MonoBehaviour
{
    public float enterTime = 1;
    public float exitTime = 1;

    public float rampUpSpeed = 1;

    public IEnumerator Cover() {
        float time = 0;

        while (time < 1) {
            CoverUpdate(time);
            yield return null;
            time += Time.unscaledDeltaTime / enterTime;
        }
        CoverUpdate(1);
    }

    protected abstract void CoverUpdate(float time);

    public IEnumerator Uncover() {
        float time = 0;
        
        float rampUp = 0;
        float scale = 1;

        while (time < 1) {
            UncoverUpdate(time);
            yield return null;

            // reduce deltatime on the first few frames to ignore the extreme values as the scene loads
            if (rampUpSpeed > 0) scale = 1-Mathf.Exp(-rampUp);
            rampUp += rampUpSpeed;

            time += Time.unscaledDeltaTime*scale / exitTime;
        }
        UncoverUpdate(1);
    }

    protected abstract void UncoverUpdate(float time);
}