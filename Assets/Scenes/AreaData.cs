using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaData : MonoBehaviour
{
    public static AreaData instance;

    public Transform[] playerSpawns;

    public GameObject flyingMusic;
    public GameObject[] disableWhileFlying;

    void Awake() {
        instance = this;
    }
}
