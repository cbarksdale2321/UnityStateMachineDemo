using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogBehaviour : MonoBehaviour
{
    public enum State { Follow, Block, Bark };
    State currentState = State.Follow;
    // Start is called before the first frame update

    //dog properties
    [SerializeField] private float barkDistance;
    private Vector3 dogOffsetPosition;
    [SerializeField] private Vector3 dogOffset;


    //references to guard/player
    public Transform guard;
    public Transform player;
    PlayerBehaviour playerBehaviour;
    public float distanceBetweenGuard;

    //matching player
    [SerializeField] private float runSpeed = 2.0f;
    [SerializeField] private float runRotSpeed = 2.0f;
    [SerializeField] private float runAccuracy = 5.0f;
    void Start()
    {

        playerBehaviour = GetComponent<PlayerBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        dogOffsetPosition = player.position - new Vector3(1, 1, 1);
        distanceBetweenGuard = Vector3.Distance(guard.position, this.transform.position);

        State tempState = currentState;

        if (CanSee(guard))
        {
            if (distanceBetweenGuard <= barkDistance)
            {
                this.currentState = State.Bark;
            }
            else if (distanceBetweenGuard >= 2.0f)
            {
                this.currentState = State.Block;
            }

        }
        else
        {
            this.currentState = State.Follow;

        }

        switch (currentState)
        {
            case State.Follow:
                Follow();
                break;
            case State.Bark:
                Bark();
                break;
            case State.Block:
                Block(guard);
                break;

        }
        if (tempState != currentState)
        {
            Debug.Log("Current State: " + currentState);
        }
    }

    public void Follow()
    {
        this.GetComponent<NavMeshAgent>().SetDestination(dogOffsetPosition);
    }

    public void Bark()
    {
        Debug.Log("Bark");
    }

    public void Block(Transform guard)
    {
        Debug.Log("Blocked.");
    }

    bool CanSee(Transform player)
    {
        Vector3 direction = player.position - this.transform.position;
        float angle = Vector3.Angle(direction, this.transform.position);

        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, direction, out hit) && hit.collider.gameObject.tag == "Guard")
        {
            return true;
        }
        return false;
    }
}
