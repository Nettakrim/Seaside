using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixerVolumeTransition : TransitionType
{
    public AudioMixer mixer;
    public string parameter;

    public override void CoverUpdate(float time) {
        SetVolume(1-time);
    }

    public override void UncoverUpdate(float time) {
        SetVolume(time);
    }

    public void SetVolume(float volume) {
        mixer.SetFloat(parameter, Mathf.Log(Mathf.Max(volume, 0.01f))*20);
    }
}
