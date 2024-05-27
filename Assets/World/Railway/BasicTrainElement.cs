using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTrainElement : TrainElement
{
    private TrackData trackData;

    private int index;
    private Transform next;

    [SerializeField] private Renderer randomHue;
    [SerializeField] private Animator wheelAnimator;

    [SerializeField] private Collider boundsCollider;

    [SerializeField] private float lengthPadding;

    void Start() {
        float hue = UnityEngine.Random.value;
        foreach (Material material in randomHue.materials) {
            material.SetFloat("_HueRotation", hue);
        }
        wheelAnimator.SetFloat("Speed", trackData.movementSpeed);

        next = trackData.trackPoints[1];
        transform.rotation = Quaternion.LookRotation(transform.position-next.position);
    }

    void Update() {
        if (Vector3.Distance(transform.position, next.position) < 1) {
            index++;
            if (index >= trackData.trackPoints.Length) {
                Destroy(gameObject);
                return;
            }
            next = trackData.trackPoints[index];
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.position-next.position), trackData.rotationSpeed*Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, next.position, trackData.movementSpeed*Time.deltaTime);
    }

    public override void SetTrackData(TrackData trackData) {
        this.trackData = trackData;
    }

    public override float GetLength() {
        return (boundsCollider.bounds.extents.z*2)+lengthPadding;
    }
}
