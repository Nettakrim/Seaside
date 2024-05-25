using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    private bool spawnReady = false;

    [SerializeField] private int minutes = 10;

    public Transform[] tracks;

    public TrainElement trainElementTemp;

    public float movementSpeed;
    public float rotationSpeed;

    void Update() {
        if (Input.GetKeyDown(KeyCode.V)) {
            SpawnTrain();
        }

        DateTime time = DateTime.Now;
        if (time.Minute%minutes == 0) {
            if (spawnReady) {
                spawnReady = false;
                SpawnTrain();
            }
        } else {
            spawnReady = true;
        }
    }

    void SpawnTrain() {
        TrainElement trainElement = Instantiate(trainElementTemp);
        trainElement.trainSpawner = this;
        trainElement.transform.position = tracks[0].transform.position;
    }
}
