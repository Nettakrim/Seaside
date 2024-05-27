using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackData : MonoBehaviour
{
    public Transform[] trackPoints;

    public float movementSpeed;
    public float rotationSpeed;

    public void SpawnTrain(TrainType trainType) {
        Vector3 start = trackPoints[0].transform.position;
        Vector3 direction = (start - trackPoints[1].transform.position).normalized;
        float length = 0f;
        float prevLength = 0f;

        foreach (TrainSection section in trainType.sections) {
            int count = Random.Range(section.minLength, section.maxLength);
            for (int i = 0; i < count; i++) {
                TrainElement trainElement = Instantiate(section.elements[Random.Range(0, section.elements.Length)], transform);
                trainElement.SetTrackData(this);
                
                length += prevLength/2;
                prevLength = trainElement.GetLength();
                length += prevLength/2;

                trainElement.transform.position = start + direction*length;
            }
        }
    }
}
