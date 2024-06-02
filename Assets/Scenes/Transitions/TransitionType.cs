using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransitionType : MonoBehaviour
{
    public abstract void CoverUpdate(float time);

    public abstract void UncoverUpdate(float time);
}