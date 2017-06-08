using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerFeeder : MonoBehaviour
{

    public float Radius;
    public List<Food> PossibleFood;
    private PlayerController player;
    public Transform Center;

    private void OnValidate()
    {
        Radius = Mathf.Max(0, Radius);
        player = GetComponent<PlayerController>();
    }

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var food in GameObject.FindObjectsOfType<Food>())
        {
            if ((food.transform.position - Center.position).magnitude < Radius)
            {
                StartCoroutine(FeedPlayer(player, food));
            }
        }


    }
    

    public IEnumerator FeedPlayer(PlayerController player, Food food)
    {
        if (food.Eaten) yield break;
        food.Eaten = true;
        player.Grow(food.Mass);
        float initialScale = food.transform.localScale.x;
        yield return AnimUtils.LoopAction(food.FeedTime, (time) =>
        {
            food.transform.localScale = Vector3.one * (1 - time) * (1 - time) * initialScale;
        });
        Destroy(food.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(Center.position, Radius);

    }
}
