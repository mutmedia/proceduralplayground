using UnityEngine;
using Utils;

public class PerlinNoiseTest : MonoBehaviour
{

    public float y = 0.5f;
    //public float z = 0.5f;

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.blue;
        for (float x = 0.0f; x < 5; x += 0.1f)
        for (float z = 0.0f; z < 5; z += 0.1f)
        {
        }
        for (int i = 0; i < 10000; i++)
        {
            var p = UnityEngine.Random.insideUnitSphere;

            var noise = PerlinNoise.Generate(p.x + 0.5f, p.y + 0.5f, p.z + 0.5f);

            p = p * (20 + noise);
            Gizmos.DrawSphere(p, 0.1f);
        }
    }

}
