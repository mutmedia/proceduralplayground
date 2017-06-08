using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BillboardController : MonoBehaviour
{

    public List<GameObject> Billboards;
    public Transform LookAt;

    private void OnValidate()
    {
        Billboards = new List<GameObject>();
    }

    private void Update()
    {
        foreach (var billboard in GameObject.FindGameObjectsWithTag("Billboard"))
        {
            if (billboard == null) continue;
            Vector3 lookVector = LookAt.position - billboard.transform.position;
            var rotationY = Mathf.Atan2(-lookVector.x, -lookVector.z) * 180.0f / Mathf.PI;
            billboard.transform.rotation = Quaternion.AngleAxis(rotationY, Vector3.up);
        }

        Billboards.RemoveAll((billboard) => billboard == null);
    }

    public void OnDrawGizmosSelected()
    {

        Billboards.ForEach((billboard) =>
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(billboard.transform.position, LookAt.position);
            });
    }
}
