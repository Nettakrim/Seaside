using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrack {
    public MusicData musicData;
    public AudioSource audioSource;

    private MusicTarget volumeTarget;

    public MusicTrack(MusicData musicData, AudioSource audioSource) {
        this.musicData = musicData;
        this.audioSource = audioSource;
        audioSource.clip = musicData.audioClip;

        volumeTarget = new MusicTarget();
    }

    public void CalculateMusicSource(MusicSource musicSource, Vector3 listenerPosition) {
        volumeTarget.SetTarget(musicSource.GetVolume(listenerPosition));
    }

    public void Reset() {
        volumeTarget.ResetTarget();
    }

    public void Update(float deltaDecay) {
        volumeTarget.UpdateCurrent(deltaDecay);
        audioSource.volume = volumeTarget.GetCurrent();
    }



    private class MusicTarget {
        private float target;
        private float current;

        public void ResetTarget() {
            target = 0;
        }

        public void SetTarget(float to) {
            if (to > target) {
                target = to;
            }
        }

        public float GetCurrent() {
            return current;
        }

        public void UpdateCurrent(float deltaDecay) {
            current = target+(current-target)*deltaDecay;
        }
    }
}