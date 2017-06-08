using System;
using System.Collections;
using UnityEngine;

public static class AnimUtils
{
    public static IEnumerator LoopAction(float totalTime, Action<float> interpolate)
    {
        for (float time = 0; time < totalTime; time += Time.deltaTime)
        {
            interpolate(time / totalTime);
            yield return null;
        }
        interpolate(1.0f);
    }
}
