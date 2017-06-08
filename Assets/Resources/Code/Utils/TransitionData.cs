using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransitionData", menuName = "Transition Data", order = 1)]
[Serializable]
public class TransitionData : ScriptableObject
{
    public float Time = 1.0f;
    public AnimationCurve Curve;
}
