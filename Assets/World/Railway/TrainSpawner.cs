using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    private bool spawnReady = false;

    [SerializeField] private int minutes = 10;

    public TrainType[] trainTypes;

    public TrackData[] tracks;

    void Update() {
        if (Input.GetKeyDown(KeyCode.V)) {
            SpawnRandomTrain();
        }

        DateTime time = DateTime.Now;
        if (time.Minute%minutes == 0) {
            if (spawnReady) {
                spawnReady = false;
                SpawnRandomTrain();
            }
        } else {
            spawnReady = true;
        }
    }

    void SpawnRandomTrain() {
        tracks[UnityEngine.Random.Range(0, tracks.Length)].SpawnTrain(trainTypes[UnityEngine.Random.Range(0, trainTypes.Length)]);
    }
}
