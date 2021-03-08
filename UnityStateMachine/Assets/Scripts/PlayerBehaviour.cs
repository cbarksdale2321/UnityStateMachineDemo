using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PlayerBehaviour : MonoBehaviour
{
    public enum State { Roam, Sneak, Run };
    State currentState = State.Roam;


    public Transform guard;
    float fovDistance = 20.0f;
    float fovAngle = 45.0f;
    Vector3 lastPlaceSeen;

    //Chase properties
    [SerializeField] private float runSpeed = 2.0f;
    [SerializeField] private float runRotSpeed = 2.0f;
    [SerializeField] private float runAccuracy = 5.0f;

    //Patrol properties
    [SerializeField] private float patrolDistance = 10.0f;
    float patrolWait = 5.0f;
    float patrolTimePassed = 0f;


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


        if (CanSee(guard))
        {
            currentState = State.Run;
            
            StartCoroutine(PlaySound());
        }
        else
        {
            if (currentState == State.Run)
            {
                currentState = State.Sneak;
            }
            
        }

        switch (currentState)
        {
            case State.Roam:
                Roam();
                break;
            case State.Sneak:
                Sneak();
                break;
            case State.Run:
                Run(guard);
                break;

        }
        if (tempState != currentState)
        {
            Debug.Log("Current State: " + currentState);
        }
    }

    private void Run(Transform guard)
    {
        this.GetComponent<NavMeshAgent>().isStopped = true;
        this.GetComponent<NavMeshAgent>().ResetPath();

        Vector3 direction = guard.position - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * this.runSpeed);

        if (direction.magnitude > this.runAccuracy)
        {
            this.transform.Translate(0, 0, Time.deltaTime * this.runSpeed);
            lastPlaceSeen = guard.position;
        }
    }

    private void Sneak()
    {
        if (this.transform.position == lastPlaceSeen)
        {
            currentState = State.Roam;
        }
        else
        {
            this.GetComponent<NavMeshAgent>().SetDestination(lastPlaceSeen);
            
        }
    }

    private void Roam()
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
        currentState = State.Sneak;
    }
    IEnumerator PlaySound()
    {
        AudioSource audio = GetComponent<AudioSource>();

        audio.Play();
        yield return new WaitForSeconds(audio.clip.length);
    }

   public bool CanSee(Transform player)
    {
        Vector3 direction = player.position - this.transform.position;
        float angle = Vector3.Angle(direction, this.transform.position);

        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, direction, out hit) && hit.collider.gameObject.tag == "Guard" && direction.magnitude < fovDistance && angle < fovAngle)
        {
            return true;
        }
        return false;
    }
}
