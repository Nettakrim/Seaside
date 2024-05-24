using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainElement : MonoBehaviour
{
    [NonSerialized] public TrainSpawner trainSpawner;

    private int index;

    void Update() {
        Transform next = trainSpawner.tracks[index];
        transform.position = Vector3.MoveTowards(transform.position, next.position, trainSpawner.trainSpeed*Time.deltaTime);
        if (Vector3.Distance(transform.position, next.position) < 1) {
            index++;
            if (index >= trainSpawner.tracks.Length) return;
        }
    }
}
