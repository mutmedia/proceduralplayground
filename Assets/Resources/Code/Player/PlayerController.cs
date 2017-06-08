using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{

    public Camera PlayerCamera;
    public float CamDistance;
    private Vector3 CamToPlayer;
    public float Scale;

    [Range(1.0f, 100.0f)]
    public float GrowthPercentage = 10.0f;
    public AnimationCurve GrowthCurve;
    public float GrowthAnimationTime;

    public float VSpeed;
    public float HSpeed;
    private float angle;

    public GameObject PlayerObject;
    private ProceduralTerrain _terrain;
    public ProceduralTerrain terrain
    {
        get
        {
            if (_terrain == null) _terrain = FindObjectOfType<ProceduralTerrain>();
            return _terrain;
        }
    }

    public void SetStuff()
    {
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerCamera = Camera.main;
        PlayerCamera.transform.parent = this.transform;
        PlayerObject.transform.parent = this.transform;
        CamToPlayer = (PlayerCamera.transform.localPosition - PlayerObject.transform.localPosition).normalized;
        //transform.localScale = Vector3.one * Scale;
        PlayerCamera.transform.localPosition = PlayerObject.transform.localPosition + CamToPlayer * CamDistance;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        terrain.ZoomScale = Scale;
    }


    public void OnValidate()
    {
        CamDistance = Mathf.Max(0.1f, CamDistance);
        Scale = Mathf.Max(0.01f, Scale);
        GrowthAnimationTime = Mathf.Max(0.1f, GrowthAnimationTime);
        SetStuff();
    }

    // Use this for initialization
    void Awake()
    {
        SetStuff();
    }

    private Vector3 mousePosition = new Vector3();
    private Vector3 lastMousePosition = new Vector3();

    public float Sensitivity = 0.1f;
    // Update is called once per frame
    void Update()
    {
        mousePosition = Input.mousePosition;
        angle += (mousePosition.x - lastMousePosition.x) * Sensitivity;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        terrain.ZoomScale = Scale;
        var movement = (Input.GetAxis("Horizontal") * HSpeed * transform.right +
            Input.GetAxis("Vertical") * VSpeed * transform.forward) * terrain.ZoomScale * Time.deltaTime;

        terrain.BaseOffset += new Vector2(movement.x, movement.z);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        //transform.position += new Vector3(HSpeed *Scale* Input.GetAxis("Horizontal"), 0, VSpeed *Scale* Input.GetAxis("Vertical"));
        PlayerCamera.transform.localPosition = PlayerObject.transform.localPosition + CamToPlayer * CamDistance;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Grow();
        }
        lastMousePosition = mousePosition;
    }

    public void Grow()
    {
        StartCoroutine(DoGrow(Scale, Scale * (GrowthPercentage + 100.0f) / 100.0f));
    }

    public void Grow(float deltaGrowth)
    {
        StartCoroutine(DoGrow(Scale, Scale + deltaGrowth));
    }

    private IEnumerator DoGrow(float initialScale, float targetScale)
    {
        var animationTime = 0.0f;
        while (animationTime < GrowthAnimationTime)
        {
            var delta = GrowthCurve.Evaluate(animationTime / GrowthAnimationTime);
            Scale = (1 - delta) * initialScale + delta * targetScale;
            //Debug.Log(GrowthCurve.Evaluate(animationTime / GrowthAnimationTime));
            yield return new WaitForFixedUpdate();
            animationTime += Time.deltaTime;
        }
        Scale = targetScale;
    }
}