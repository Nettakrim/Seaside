using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TrainType : ScriptableObject
{
    public TrainSection[] sections;
}

[Serializable]
public class TrainSection
{
    public TrainElement[] elements;
    public int minLength;
    public int maxLength;
}