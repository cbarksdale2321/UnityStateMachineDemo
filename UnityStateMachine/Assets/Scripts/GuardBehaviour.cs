using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class GuardBehaviour : MonoBehaviour
{

    public enum State { Patrol, Investigate, Chase};
    State currentState = State.Patrol;


    public Transform player;
    float fovDistance = 20.0f;
    float fovAngle = 60.0f;
    Vector3 lastPlaceSeen;

    //Chase properties
    [SerializeField] private float chasingSpeed = 2.0f;
    [SerializeField] private float chasingRotSpeed = 2.0f;
    [SerializeField] private float chasingAccuracy = 5.0f;

    //Patrol properties
    [SerializeField] private float patrolDistance = 10.0f;
    float patrolWait = 5.0f;
    float patrolTimePassed = 0f;
    float distanceBetweenPlayer;

    //investigation properties
    public float soundRadius = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        patrolTimePassed = patrolWait;
        lastPlaceSeen = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        State tempState = currentState;
        distanceBetweenPlayer = Vector3.Distance(this.transform.position, player.position);
        Debug.Log("Distance between player and guard: " + distanceBetweenPlayer);
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, soundRadius);
        for (int i = 0;  i < hitColliders.Length; i++)
        {
            if (hitColliders[i].tag == "Guard")
            {
                hitColliders[i].GetComponent<GuardBehaviour>().InvestigationPoint(this.transform.position);

            }
        }

        if (CanSee(player))
        {
            currentState = State.Chase;
            
            StartCoroutine(PlaySound());
            if (distanceBetweenPlayer <= 5.0f)
            {
                player.gameObject.SetActive(false);
            }
        }
        else 
        {
            if (currentState == State.Chase)
            {
                currentState = State.Investigate;
            }
            
        }

        switch(currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Investigate:
                Investigate();
                break;
            case State.Chase:
                Chase(player);
                break;

        }
        if (tempState != currentState)
        {
            Debug.Log("Current State: " + currentState);
        }
    }

    private void Chase(Transform player)
    {
        this.GetComponent<NavMeshAgent>().isStopped = true;
        this.GetComponent<NavMeshAgent>().ResetPath();

        Vector3 direction = player.position - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * this.chasingSpeed);

        if (direction.magnitude > this.chasingAccuracy)
        {
            this.transform.Translate(0, 0, Time.deltaTime * this.chasingSpeed);
            lastPlaceSeen = player.position;
        }
    }

    private void Investigate()
    {
        if (this.transform.position == lastPlaceSeen)
        {
            currentState = State.Patrol;
        }
        else
        {
            this.GetComponent<NavMeshAgent>().SetDestination(lastPlaceSeen);
            //Debug.Log("Guard State: " + currentState + " point " + lastPlaceSeen);
        }
    }

    private void Patrol()
    {
        patrolTimePassed += Time.deltaTime;

        if (patrolTimePassed > patrolWait)
        {
            patrolTimePassed = 0f;

            Vector3 patrollingPoint = lastPlaceSeen;
            patrollingPoint += new Vector3(UnityEngine.Random.Range(-patrolDistance, patrolDistance), 0f, UnityEngine.Random.Range(-patrolDistance, patrolDistance));

            this.GetComponent<NavMeshAgent>().SetDestination(patrollingPoint);
        }
    }
    public void InvestigationPoint(Vector3 point)
    {
        lastPlaceSeen = point;
        currentState = State.Investigate;
    }
    IEnumerator PlaySound()
    {
        AudioSource audio = GetComponent<AudioSource>();

        audio.Play();
        yield return new WaitForSeconds(audio.clip.length);
    }

    bool CanSee(Transform player)
    {
        Vector3 direction = player.position - this.transform.position;
        float angle = Vector3.Angle(direction, this.transform.position);

        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, direction, out hit) && hit.collider.gameObject.tag == "Player" && direction.magnitude < fovDistance && angle < fovAngle)
        {
            return true;
        }
        return false;
    }
}
