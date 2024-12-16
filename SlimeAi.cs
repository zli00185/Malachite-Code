using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class SlimeAi : MonoBehaviour
{
    Transform player;

    NavMeshAgent agent;

    public LayerMask whatIsGround;

    Health_Bar Player_heath;

    PlayerStatus playerStatus;

    public GameObject exclamationMark;
    public GameObject TransformObject;
    public GameObject HealItem;

    public float Max_Enemy_health;
    private float Enemy_health;

    public AudioSource death;

    //Patrolling
    Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public float patrollSpeed;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public int attackDamage;
    Rigidbody rb;
    public float chaseSpeed;

    //States
    public float sightRange, attackRange;
    float OrignalPositionY;

    enum state 
    {
        Idle,
        Persuing,
        Attacking,
        Dead
    }

    state states = state.Idle;

    private void Awake()
    {
        transform.SetParent(null);//so it will move out the folder once activate
        Player_heath = GameObject.Find("UI").GetComponent<Health_Bar>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").transform;
        playerStatus = GameObject.Find("Player").GetComponent<PlayerStatus>();
        agent = GetComponent<NavMeshAgent>();
        exclamationMark.SetActive(false);
        walkPointSet = false;

        SphereCollider[] triggers = GetComponents<SphereCollider>();
        Enemy_health = Max_Enemy_health;

        // sightRange
        triggers[0].radius = sightRange;
        triggers[0].isTrigger = true;

        // attackRange
        triggers[1].radius = attackRange;
        triggers[1].isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {

        if ( Enemy_health <= 0 ) {
            states = state.Dead;
        } else if( playerStatus.isDead() ) {
            states = state.Idle;
        }


            switch (states) {

          case state.Idle:
            Patroling();
            break;

          case state.Persuing:
            Chasing();
            break;

          case state.Attacking:
            Attacking();
            break;

          case state.Dead:
            OrignalPositionY = GetComponentInChildren<Renderer>().bounds.min.y;
            //Debug.Log("OrignalPositionVector: "+ OrignalPositionY);
            DisableExclamationMark();
            StartCoroutine(Slime_dead());
            break;

        }

    }
    private float damageCooldown = 0.5f;
    private float lastDamageTime = 0;
    void OnTriggerEnter(Collider other) {

        bool isCharacterController = other.tag == "Player" &&
          other.GetType().ToString() == "UnityEngine.CharacterController"; 

        if( isCharacterController && states < state.Attacking) {
            states++;
        }

        // lowercase attack even though editor shows uppercase
        // Must be within enemy attack range to hit
        //    ^ You can exit range before animation finishes and you wont get hit
        if( other.tag == "Sword" && states == state.Attacking && Time.time >= lastDamageTime + damageCooldown) {

            AudioSource hitBy = other.GetComponent<AudioSource>();
            if (hitBy != null)
            {
                hitBy.Play();
            }
          // Divide by 2 bc collision will happen on both colliders
          EnemyTakeDamage(playerStatus.getPlayerDamage());
          lastDamageTime = Time.time;
        }
    }

    void OnTriggerExit(Collider other) {

        bool isCharacterController = other.tag == "Player" &&
          other.GetType().ToString() == "UnityEngine.CharacterController"; 

        if( isCharacterController && states > state.Idle) {
            states--;
        }
    }

    private void Patroling()
    {
        DisableExclamationMark();
        agent.speed = patrollSpeed;
        if (!walkPointSet)
        {
            SeachWalkPoint();
        }else{
            agent.SetDestination(walkPoint);
        }
       
        Vector3 distanceToWalkPoint = transform.position- walkPoint;
        //Debug.Log("patroling: finding, distance to point: " + distanceToWalkPoint.magnitude);
        if (distanceToWalkPoint.magnitude < 2f)
        {
            Debug.Log("patroling: reach destinypoint");
            walkPointSet = false;
        }
    }

    private void SeachWalkPoint()
    {
        //Debug.Log("search walk point");
        walkPoint = RandomNavmeshLocation();
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;

            //Vector3 distanceToWalkPoint = transform.position - walkPoint;
            //Debug.Log("patroling: finding, distance to point: " + distanceToWalkPoint.magnitude);
        }
    }

    //Return a random point in NavmeshSurface
    private Vector3 RandomNavmeshLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkPointRange;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkPointRange, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
    private void Chasing()
    {
        //show ExclamationMark
        EnableExclamationMark();
        
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        walkPoint = player.position;
        //Debug.Log("chasing speed: " + agent.speed);
    }

    private void EnableExclamationMark()
    {
        //mark alwasy face to mc
        exclamationMark.transform.LookAt(player.transform.position);
        exclamationMark.SetActive(true);

    }

    private void DisableExclamationMark()
    {
        exclamationMark.SetActive(false);
    }

    private void Attacking()
    {
        DisableExclamationMark();
        Vector3 distanceToPlayer = transform.position - player.position;

        if (!alreadyAttacked)
        {
            //Attack code
            Attack();

            //delay of attack
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void Attack()
    {
        //Attack Happen during anim
        StartCoroutine(AttackAnim1());
       
        //StartCoroutine(AttackAnim2());
        alreadyAttacked = true;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void EnemyTakeDamage(float damage)
    {
        Enemy_health = Enemy_health - damage;
        if (Enemy_health <= 0)
        {
            death.Play();
        }
        StartCoroutine(IndicateHit());
    }

    IEnumerator IndicateHit() {
        Renderer r = GetComponentInChildren<Renderer>();
        // r.material.SetColor("_EmissionColor", Color.white * 0.5f );
        r.material.SetColor("_EmissionColor", Color.red * 0.5f );

        r.material.EnableKeyword("_EMISSION");
        yield return new WaitForSeconds(0.3f);

        r.material.DisableKeyword("_EMISSION");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

    }

    IEnumerator Slime_dead()
    {
        yield return StartCoroutine(DeadAnim());
    }

    IEnumerator DeadAnim()
    {
        //Drop Heal Item
        if (HealItem != null)
        {
            //Debug.Log("Clone");
            GameObject clone = Instantiate(HealItem);
            clone.transform.localPosition =new  Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1f, gameObject.transform.position.z);
            clone.SetActive(true);
            HealItem = null;
        }

        //Debug.Log("in deadAnim");
        int secondsPerStep = 4;
        float elapsedTime = 0;
        float totalDuration = Mathf.Abs(secondsPerStep);
        Vector3 initialScale = gameObject.transform.localScale;
        Vector3 targetScale = new Vector3(gameObject.transform.localScale.x,  0.15f, gameObject.transform.localScale.z);

        Vector3 initialPosition = gameObject.transform.localPosition;
        //Vector3 targetPosition = new Vector3(gameObject.transform.localPosition.x, 0, gameObject.transform.localPosition.z);
        Vector3 targetPosition = new Vector3(gameObject.transform.localPosition.x, OrignalPositionY, gameObject.transform.localPosition.z);
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / totalDuration;
            gameObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, lerpFactor);
            gameObject.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, lerpFactor);
            yield return null;
        }

        // Ensure exact target values are met at the end of animation
        gameObject.transform.localScale = targetScale;
        gameObject.transform.localPosition = targetPosition;
        Destroy(gameObject, 5f);
    }
    private int HitCount = 0;
    IEnumerator AttackAnim1()
    {
       
        //First Step
        float secondsPerStep = 0.5f;
        float elapsedTime = 0;
        float totalDuration = Mathf.Abs(secondsPerStep);
        Vector3 initialScale = gameObject.transform.localScale; 
        //Vector3 targetScale = new Vector3(gameObject.transform.localScale.x, 0.5f, gameObject.transform.localScale.z);
        Vector3 targetScale = new Vector3(0.8f*gameObject.transform.localScale.x, 1.5f*gameObject.transform.localScale.x, 0.8f * gameObject.transform.localScale.z);

        Vector3 initialPosition = gameObject.transform.localPosition;
        Vector3 targetPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y+2f, gameObject.transform.localPosition.z);
        while (elapsedTime < totalDuration&& states != state.Dead)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / totalDuration;
            gameObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, lerpFactor);
            gameObject.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, lerpFactor);
            yield return null;
        }
        gameObject.transform.localScale = targetScale;
        gameObject.transform.localPosition = targetPosition;
        
        
        //Second step 
        float secondsPerStep1 = 0.25f;
        float elapsedTime1 = 0;
        float totalDuration1 = Mathf.Abs(secondsPerStep1);
        Vector3 initialScale1 = gameObject.transform.localScale;
        Vector3 targetScale1 = new Vector3(2f* gameObject.transform.localScale.x, 0.1f* gameObject.transform.localScale.y, 2f* gameObject.transform.localScale.z);

        Vector3 initialPosition1 = gameObject.transform.localPosition;
        // 2?
        Vector3 targetPosition1 = new Vector3(gameObject.transform.localPosition.x,gameObject.transform.localPosition.y - 2f, gameObject.transform.localPosition.z);
        while (elapsedTime1 < totalDuration1 && states != state.Dead)
        {
            elapsedTime1 += Time.deltaTime;
            float lerpFactor = elapsedTime1 / totalDuration1;
            gameObject.transform.localScale = Vector3.Lerp(initialScale1, targetScale1, lerpFactor);
            gameObject.transform.localPosition = Vector3.Lerp(initialPosition1, targetPosition1, lerpFactor);
            yield return null;
        }
        gameObject.transform.localScale = targetScale1;
        gameObject.transform.localPosition = targetPosition1;

        //Apply Damage
        if ( states == state.Attacking )
        {
            Debug.Log("Slime do " + attackDamage);
            Player_heath.GetHitByDamage(attackDamage, transform);
            if(HitCount == 1)
            {
                playerStatus.BecomeSink(1.5f);
                HitCount = 0;
            }
            else
            {
                HitCount += 1;
            }
            
        }
      
        //Third step
        float secondsPerStep2 = 1.25f;
        float elapsedTime2 = 0;
        float totalDuration2 = Mathf.Abs(secondsPerStep2);
        Vector3 initialScale2 = gameObject.transform.localScale;
        //Vector3 targetScale2 = new Vector3(gameObject.transform.localScale.x, 2f, gameObject.transform.localScale.z);

        Vector3 initialPosition2 = gameObject.transform.localPosition;
       //Vector3 targetPosition2 = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y - 1, gameObject.transform.localPosition.z);
        while (elapsedTime2 < totalDuration2 && states != state.Dead)
        {
            elapsedTime2 += Time.deltaTime;
            float lerpFactor = elapsedTime2 / totalDuration2;
            gameObject.transform.localScale = Vector3.Lerp(initialScale2, initialScale, lerpFactor);
            gameObject.transform.localPosition = Vector3.Lerp(initialPosition2, initialPosition, lerpFactor);
            yield return null;
        }
        gameObject.transform.localScale = initialScale;
        gameObject.transform.localPosition = initialPosition;
    }
}
