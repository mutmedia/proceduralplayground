using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    [SerializeField]
    private Camera referenceCamera;

    [SerializeField]
    private bool reverseFace = false;

    private GameObject parent;

    [SerializeField]
    private Vector3 Axis = Vector3.up;

    [SerializeField]
    private GameObject planet;

    public void OnValidate()
    {

        referenceCamera = referenceCamera ?? Camera.main;
    }

    // Use this for initialization
    void Awake()
    {
        parent = new GameObject();
        parent.name = "Billboard Container";
        parent.transform.parent = this.transform.parent;
        this.transform.parent = parent.transform;

        if (!referenceCamera)
        {
        }
    }

    public void SetAxis(Vector3 axis)
    {
        Axis = axis;
    }

    // Update is called once per frame
    void Update()
    {
        var targetPos = referenceCamera.transform.position;
        var targetOri = Axis;
        parent.transform.LookAt(targetPos, targetOri);
        if (reverseFace)
        {
            parent.transform.Rotate(Axis, 180.0f);
        }
        parent.transform.localPosition = Vector3.zero;
        transform.localPosition = Vector3.zero;
    }
}
