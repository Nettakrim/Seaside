using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackData : MonoBehaviour
{
    public Transform[] trackPoints;

    public float movementSpeed;
    public float rotationSpeed;

    public float SpawnTrain(TrainType trainType) {
        Vector3 start = trackPoints[0].transform.position;
        Vector3 direction = (start - trackPoints[1].transform.position).normalized;
        float length = 0f;
        float halfLength = 0f;

        // train types consist of multiple sections with different pools of elements and a random length range
        // a passenger train for example, is one section of length 1 that is always a locomotive, and another section of around 3-5 which is always passenger carts

        foreach (TrainSection section in trainType.sections) {
            int count = Random.Range(section.minLength, section.maxLength);
            for (int i = 0; i < count; i++) {
                TrainElement trainElement = Instantiate(section.elements[Random.Range(0, section.elements.Length)], transform);
                trainElement.SetTrackData(this);
                
                // length additions are split between loops to center each element
                length += halfLength;
                halfLength = trainElement.GetLength()/2;
                length += halfLength;

                trainElement.transform.position = start + direction*length;
            }
        }

        length += halfLength;
        return length;
    }

    public float GetLength() {
        float length = 0;
        for (int i = 1; i < trackPoints.Length; i++) {
            length += Vector3.Distance(trackPoints[i-1].position, trackPoints[i].position);
        }
        return length;
    }
}
