using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Food : MonoBehaviour
{
    public float Mass;
    public float FeedTime;
    public float MinHeight = 0.0f;
    public float MaxHeight = 1.0f;
    public List<float> OcurringBiomes;
    [SerializeField]
    private float Size;
    public float CurrentSize = 0.0f;
    public float Height = 0;
    public Vector3 initialPosition;
    public float InitialZoomScale;
    public Vector2 InitialOffset;
    public TransitionData GrowTransition;
    public float GrowTime
    {
        get
        {
            return GrowTransition.Time;
        }
    }
    public AnimationCurve GrowthCurve
    {
        get
        {
            return GrowTransition.Curve;
        }
    }

    public bool Eaten = false;

    private float ScaleToMass(float f)
    {
        return Mathf.Sqrt(0.01f * f);
    }

    public void Setup()
    {
        Mass = ScaleToMass(Size);
        FeedTime = 1.0f;
        Eaten = false;
        CurrentSize = 0.0f;
    }

    public void OnValidate()
    {
        Setup();
    }

    public void Awake()
    {
        Setup();
    }

    public void Start()
    {
        StartCoroutine(
            AnimUtils.LoopAction(
                GrowTime,
                (t) => CurrentSize = GrowthCurve.Evaluate(t) * Size
            )
        );
    }

    public void Update()
    {
    }
}
