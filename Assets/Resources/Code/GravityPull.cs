using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityPull : MonoBehaviour {

    public GameObject planet;
    private Rigidbody rb;
    private Vector3 gravity;
    public Vector3 Gravity
    {
        get
        {
            return gravity;
        }
    }
	// Use this for initialization
	void Awake () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        gravity = (planet.transform.position - transform.position).normalized;
        rb.AddForce(gravity * 10);
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, gravity);
    }
}
