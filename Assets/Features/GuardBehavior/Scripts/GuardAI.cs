using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class GuardAI : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints;
    public Transform[] lookpoints;
    int waypointIndex;
    Vector3 target;
    public float turnRate;
    bool Looking = false;
    bool nextLook = false;
    Coroutine LookRoutine;
    int guardCD = 0;
    public bool allowGameOver = true;
    bool playerFollow = false;
    float viewDistance = 2000f;
    float viewAngle = 15f;
    public Transform player;
    public Animator guard;
    public AudioSource[] m_AudioSource;
    public AudioClip[] m_VoiceTrigger;
    public AudioClip[] m_VoiceCalm;
    public AudioClip[] m_VoiceCatch;
    public AudioClip[] m_VoiceRando;
    public bool m_switch = false;
    public bool m_playSound = false;
    
    // Start is called before the first frame update
    void Start()
    {
        m_AudioSource = GetComponents<AudioSource>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        Destination();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerFollow)
        {
            navMeshAgent.SetDestination(player.transform.position);
            if (Vector3.Distance(transform.position, player.position) < 2f)
            {
                // Replace with game over scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
            }
        }
        if (!Looking){
            if(Vector3.Distance(transform.position,target)<1)
            {
                if (waypointIndex == 2 || waypointIndex == 6)
                {
                    m_AudioSource[0].enabled = false;
                }
                else if (waypointIndex == 3 || waypointIndex == 7)
                {
                    m_AudioSource[0].enabled = true;
                    m_AudioSource[1].clip = m_VoiceRando[Random.Range(0, m_VoiceRando.Length)]; 
                    m_AudioSource[1].PlayOneShot(m_AudioSource[1].clip); 
                }
                IterateIndex();
                Destination();
                if(waypoints[waypointIndex].CompareTag("LookPoint"))
                {
                    nextLook = true;
                    m_AudioSource[1].clip = m_VoiceTrigger[Random.Range(0, m_VoiceTrigger.Length)]; 
                    m_AudioSource[1].PlayOneShot(m_AudioSource[1].clip);
                    return;
                }
            }
        }
        if(nextLook)
        {
            StopToLook();
        }
        //Debug.Log("nextLook: " + nextLook.ToString());
        //Debug.Log("Looking: " + Looking.ToString());
        if(Looking)
        {
            if(allowGameOver)
            {
                if (GameObject.FindObjectOfType<Flashlight>().IsFlashlightOn && PlayerDetection(player))
                {
                    m_AudioSource[1].clip = m_VoiceCatch[Random.Range(0, m_VoiceCatch.Length)]; 
                    m_AudioSource[1].PlayOneShot(m_AudioSource[1].clip); 
                    // Implement Game Over
                    Debug.Log("Game Over");
                    playerFollow = true;
                }
            }
            if(LookRoutine == null)
            {
                LookRoutine = StartCoroutine(LookAround());
                Destination();
                guardCD = 2;
            }
        }
    }
    void StopToLook()
    {
        //Debug.Log("remaining: " + navMeshAgent.remainingDistance);
        //Debug.Log("stopping: " + navMeshAgent.stoppingDistance);
        if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                    navMeshAgent.isStopped = true;
                    Looking = true;
                    guard.SetBool("isLooking", true);
                    nextLook = false;
            }
    }
    void Destination()
    {
        if(guardCD > 0 && waypoints[waypointIndex].CompareTag("LookPoint"))
        {
            IterateIndex();
            guardCD--;
            if(guardCD < 0)
            {
                guardCD = 0;
            }
        }
        target = waypoints[waypointIndex].position;
        Debug.Log(target.ToString());
        navMeshAgent.SetDestination(target);
    }
    void IterateIndex()
    {
        waypointIndex++;
        if(waypointIndex == waypoints.Length)
        {
            waypointIndex = 0;
        }
    }
    IEnumerator LookAround()
    {
        // Debug.Log("Coroutine Time");
        float timeLA;
        Quaternion initialRot = transform.rotation;
        Quaternion targetRotLeft = initialRot * Quaternion.Euler(0,-45,0);
        Quaternion targetRotRight = initialRot * Quaternion.Euler(0,90,0);
        timeLA = 0;
        while(timeLA < 1.0f){
        // Mid to Left
        // Debug.Log("Left :" + timeLA);
        transform.rotation = Quaternion.Lerp(initialRot,targetRotLeft,timeLA); //Quaternion.RotateTowards(initialRot, targetRotLeft, turnRate * Time.deltaTime);
        timeLA += Time.deltaTime;
        // Debug.Log("Left after: " + timeLA);
        yield return new WaitForSeconds(0.01f);
        }
        timeLA = 0;
        while(timeLA < 1){
        // Left to Right
        // Debug.Log("Right :" + timeLA);
        transform.rotation = Quaternion.Lerp(targetRotLeft, targetRotRight, timeLA);
        timeLA += Time.deltaTime;
        // Debug.Log("Right after: " + timeLA);
        yield return new WaitForSeconds(0.01f);
        }
        timeLA = 0;
        while(timeLA < 1){
        // Right to Mid
        // Debug.Log("Mid :" + timeLA);
        transform.rotation = Quaternion.Lerp(targetRotRight, initialRot, timeLA);
        timeLA += Time.deltaTime;
        // Debug.Log("Mid after: " + timeLA);
        yield return new WaitForSeconds(0.01f);
        }   
        // Debug.Log("Test");
        Looking = false;
        guard.SetBool("isLooking", false);
        LookRoutine = null;
        navMeshAgent.isStopped = false;
        m_AudioSource[1].clip = m_VoiceCalm[Random.Range(0, m_VoiceCalm.Length)]; 
        m_AudioSource[1].PlayOneShot(m_AudioSource[1].clip); 
    }
    bool PlayerDetection(Transform player)
    {
        if(Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, direction);
            if(angle < viewAngle / 2)
            {
                if(!Physics.Linecast(transform.position, player.position))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
