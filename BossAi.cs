using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BossAi : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> projectile = new List<GameObject>();
    private Vector3 PlayerPosition;

    public Transform player;

    public NavMeshAgent agent;

    public LayerMask whatIsGround, whatIsPlayer;

    Health_Bar Player_heath;

    PlayerStatus playerStatus;

    public GameObject Boss_Health_UI;
    public UnityEngine.UI.Slider slider;
    private float Enemy_health;
    public float Max_Enemy_health;


    //Attacking
    //public int ExpoldeDamage;
    Rigidbody rb;
    public float chaseSpeed;

    //States
    //public float explodeRange, triggerRange, ProjectileSpeed;
    public float triggerRange,SummonRange;
    public bool playerInTriggerRange, playerInSummonRange, Invincible;

    //Summon
    public bool SummonInCoolDown = false;
    private float nextSummonTime = 0f;
    private float summonCooldownDuration = 20f;

    //Shoot
    public bool ShootInCoolDown = false;
    private float nextShootTime = 0f;
    private float shootCoolDownDuration = 6f;

    //For Anim
    private float HandOriginalPositionY;

    //For dead scene
    public Transform head;
    public Transform body;
    public Transform left;
    public Transform right;
    public Transform sword;

    private Rigidbody rb_head;
    private Rigidbody rb_body;
    private Rigidbody rb_left;
    private Rigidbody rb_right;
    private Rigidbody rb_sword;

    private Collider Co_head;
    private Collider Co_body;
    private Collider Co_left;
    private Collider Co_right;
    private Collider Co_sword;


    enum state
    {
        Idle,
        Combat,
        Resting,
        Shooting,
        Summoning,
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
        Invincible = false;

        HandOriginalPositionY = right.localPosition.y;

        //Cooldown reset
        SummonInCoolDown = false;
        ShootInCoolDown = false;

        //Health Set up
        Boss_Health_UI.SetActive(false);
        slider.maxValue = Max_Enemy_health;
        slider.value = Max_Enemy_health;
        Enemy_health = Max_Enemy_health;

        //Dead Scene Set up
        rb_head = head.GetComponent<Rigidbody>();
        rb_body = body.GetComponent<Rigidbody>();
        rb_left = left.GetComponent<Rigidbody>();
        rb_right = right.GetComponent<Rigidbody>();
        rb_sword = sword.GetComponent<Rigidbody>();

        Co_head = head.GetComponent<Collider>();
        Co_body = body.GetComponent<Collider>();
        Co_left = left.GetComponent<Collider>();
        Co_right = right.GetComponent<Collider>();
        Co_sword = sword.GetComponent<Collider>();

        rb_head.isKinematic = true;
        rb_body.isKinematic = true;
        rb_left.isKinematic = true;
        rb_right.isKinematic = true;
        rb_sword.isKinematic = true;

        //Disactive the collider
        Co_head.enabled = false;
        Co_body.enabled = false;
        Co_left.enabled = false;
        Co_right.enabled = false;
        Co_sword.enabled = false;
    }


    // Update is called once per frame
    void Update()
    {
       
        playerInTriggerRange = Physics.CheckSphere(transform.position, triggerRange, whatIsPlayer);
        playerInSummonRange = Physics.CheckSphere(transform.position, SummonRange, whatIsPlayer);

        if (Enemy_health <= 0)
        {
            Debug.Log("Boss Dead");
            states = state.Dead;
        }
        else if (playerStatus.isDead())
        {
            states = state.Idle;
        }else if(playerInTriggerRange && states == state.Idle)
        {
            Debug.Log("Boss Trigger");
            states = state.Combat;
            Boss_Health_UI.SetActive(true);
        }else if (playerInSummonRange && states == state.Combat && !SummonInCoolDown)
        {  
            states = state.Summoning;
        }else if(!playerInSummonRange && states == state.Combat && !ShootInCoolDown)
        {
            states = state.Shooting;
        }


        #region SKill Timer
        if (Time.time >= nextSummonTime)
        {
            SummonInCoolDown = false;
        }
        if (Time.time >= nextShootTime)
        {
            ShootInCoolDown = false;
        }

        #endregion
        #region switch states 

        switch (states)
        {

            case state.Idle:
                Invincible = true;
                break;
            case state.Combat:
                Invincible = false;
                agent.updatePosition = true;
                agent.speed = chaseSpeed;
                agent.SetDestination(player.transform.position);
                break;
            case state.Shooting:
                Invincible = false ;
                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
                Debug.Log("Shoot");
                ShootInCoolDown = true;
                states = state.Resting;
                nextShootTime = Time.time + shootCoolDownDuration;
                StartCoroutine(Attacking());
                break;
            case state.Resting:
                agent.updatePosition = true;
                right.localPosition = new Vector3(right.transform.localPosition.x, HandOriginalPositionY, right.transform.localPosition.z);
                agent.speed = chaseSpeed*3f;
                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
                float desiredDistance = 10f;
                if (playerInSummonRange)
                {
                    Vector3 direction = (transform.position - player.position).normalized;
                    Vector3 newPos = player.position + direction * desiredDistance;
                    agent.SetDestination(newPos);
                }
                
                StartCoroutine(TriggerCombatState());
                break;

            case state.Summoning:
                Invincible = true;
                SummonInCoolDown = true;
                states = state.Resting;
                nextSummonTime = Time.time + summonCooldownDuration;
                agent.updatePosition = false;
               
                Invoke(nameof(Summoning), 1.5f);
                break;

            case state.Dead:
                EnemyDead();
                break;

        }

        #endregion
        var fillColor = slider.fillRect.GetComponent<UnityEngine.UI.Image>();
        if (Invincible)
        {
            fillColor.color = new Color(253 / 255f, 232 / 255f, 111/ 255f);
        }
        else
        {
            fillColor.color = Color.red;
        }

    }

    #region Collison Check
    private float damageCooldown = 0.5f; 
    private float lastDamageTime = 0;
    private int Hit_count = 0;
    void OnTriggerEnter(Collider other)
    {

        bool isCharacterController = other.tag == "Player" &&
          other.GetType().ToString() == "UnityEngine.CharacterController";
        // lowercase attack even though editor shows uppercase
        // Must be within enemy attack range to hit
        //    ^ You can exit range before animation finishes and you wont get hit
        if (other.tag == "Sword"&& Time.time >= lastDamageTime + damageCooldown)
        {
            if (Invincible)
            {
                
            }
            else
            {
                lastDamageTime = Time.time;
                Debug.Log(gameObject.name + " get hit by " + other.tag);
                AudioSource hitBy = other.GetComponent<AudioSource>();
                if (hitBy != null)
                {
                    hitBy.Play();
                }
                EnemyTakeDamage(playerStatus.getPlayerDamage());
                Hit_count++;
                if (Hit_count == 4)
                {
                    states = state.Resting;
                    Hit_count = 0;
                }

            }

        }

    }

    #endregion
    IEnumerator TriggerCombatState()
    {
        yield return new WaitForSeconds(5f);
        states = state.Combat;
    }

    #region Summon
    [SerializeField]
    private List<GameObject> Summoner = new List<GameObject>();

    public GameObject SummonPosition1;
    public GameObject SummonPosition2;

    GameObject clone1;
    GameObject clone2;
    private void Summoning()
    {
        StartCoroutine(SummonAnimation());
        int randomNumber = Random.Range(0, Summoner.Count);
        Debug.Log("Summon");
        clone1 = Instantiate(Summoner[randomNumber],transform.parent);
        //need to be modify
        clone1.transform.position = SummonPosition1.transform.position;
        clone1.SetActive(true);

        randomNumber = Random.Range(0, Summoner.Count);

        clone2 = Instantiate(Summoner[randomNumber],transform.parent);
        //need to be modify
        clone2.transform.position = SummonPosition2.transform.position;
        clone2.SetActive(true);
    }
    private void OnDestroy()
    {
        
    }
    #endregion
    #region UI
    private void EnableExclamationMark()
    {
        //mark alwasy face to mc


    }

    private void DisableExclamationMark()
    {

    }
    #endregion
    #region Attack
    public List<GameObject> Ammo;
    public GameObject SummonPosition3;
    public float projectile_speed;
    List<GameObject> Projectiles;
    IEnumerator Attacking()
    {
        Projectiles = new List<GameObject>();
        int Max = Random.Range(3, 7);
        Debug.Log("Ammo: " + Max);
        for (int i = 0; i < Max; i++)
        {
            int x = Random.Range(0, Ammo.Count);
            Projectiles.Add(Instantiate(Ammo[x]));
        }

        #region attack anim1
        //Anim step one
        float secondsPerStep = 1.5f;
        float elapsedTime = 0;
        float totalDuration = Mathf.Abs(secondsPerStep);

        Vector3 initialPosition = right.transform.localPosition;
        Vector3 targetPosition = new Vector3(right.transform.localPosition.x, right.transform.localPosition.y + 3f, right.transform.localPosition.z);
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / totalDuration;
            right.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, lerpFactor);
            yield return null;
        }

        // Ensure exact target values are met at the end of animation
        right.transform.localPosition = targetPosition;
        #endregion

        //Shoot the sphere
        for (int i = 0; i < Max; i++)
        {
            StartCoroutine(Shoot());
            yield return new WaitForSeconds(1f);
        }

        #region attack anim2
        //step2
        float secondsPerStep2 = 1f;
        float elapsedTime2 = 0;
        float totalDuration2 = Mathf.Abs(secondsPerStep2);

        Vector3 initialPosition2 = right.transform.localPosition;
        Vector3 targetPosition2 = new Vector3(right.transform.localPosition.x, right.transform.localPosition.y - 3f, right.transform.localPosition.z); ;
        while (elapsedTime2 < totalDuration2)
        {
            elapsedTime2 += Time.deltaTime;
            float lerpFactor = elapsedTime2 / totalDuration2;
            right.transform.localPosition = Vector3.Lerp(initialPosition2, targetPosition2, lerpFactor);
            yield return null;
        }

        // Ensure exact target values are met at the end of animation
        right.transform.localPosition = targetPosition2;
        #endregion
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Hand should be down now");
    }

    IEnumerator Shoot()
    {
        if (Projectiles.Count > 0)
        {
            GameObject clone = Projectiles[0];
            
            if (clone != null)
            {  // Check if the object still exists
                clone.transform.position = SummonPosition3.transform.position;
                clone.SetActive(true);
                clone.GetComponent<Collider>().enabled = true;
                clone.transform.SetParent(null);
                Vector3 targetPosition = player.transform.position;
                Vector3 direction = (targetPosition - clone.transform.position).normalized;
                Rigidbody rb = clone.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.velocity = direction * projectile_speed;
                Projectiles.Remove(clone);
                Debug.Log("Finish shoot");
            }

        }

        yield return new WaitForSeconds(1f);
    }

    private void ResetAttack()
    {
    }
    #endregion
    public void EnemyTakeDamage(float damage)
    {
        Enemy_health = Enemy_health - damage;
        if (Enemy_health < 0)
        {
            Enemy_health = 0;
            slider.value = Enemy_health;
        }
        else
        {
            slider.value = Enemy_health;
            
        }

        Debug.Log("Boss Heath: " + Enemy_health);
    }

    void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRange);
            Gizmos.color = Color.red; ;
            Gizmos.DrawWireSphere(transform.position, SummonRange);

        }

    #region Dead
  
    private void EnemyDead()
    {
        agent.updatePosition=false;
        
        head.SetParent(null, true);
        body.SetParent(null, true);
        left.SetParent(null, true);
        right.SetParent(null, true);
        sword.SetParent(null, true);

        Co_head.enabled = true;
        Co_body.enabled = true;
        Co_left.enabled = true;
        Co_right.enabled = true;
        Co_sword.enabled = true;

        rb_head.isKinematic = false;
        rb_body.isKinematic = false;
        rb_left.isKinematic = false;
        rb_right.isKinematic = false;
        rb_sword.isKinematic = false;

        rb_head.useGravity = true;
        rb_body.useGravity = true;
        rb_left.useGravity = true;
        rb_right.useGravity = true;
        rb_sword.useGravity = true;

        Invoke(nameof(DestroyObejct), 2f);
    }
    private void DestroyObejct()
    {

        /*
        Destroy(head);
        Destroy(body);
        Destroy(left);
        Destroy(right);
        */
        Destroy(gameObject);
    }

    #endregion

    #region Animation

    IEnumerator SummonAnimation()
    {
        //step 1
        float secondsPerStep = 1.5f;
        float elapsedTime = 0;
        float totalDuration = Mathf.Abs(secondsPerStep);

        Vector3 initialPosition = right.transform.localPosition;
        Vector3 targetPosition = new Vector3(right.transform.localPosition.x, right.transform.localPosition.y+3f, right.transform.localPosition.z);
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / totalDuration;
            right.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, lerpFactor);
            yield return null;
        }

        // Ensure exact target values are met at the end of animation
        right.transform.localPosition = targetPosition;

        //step2
        float secondsPerStep2 = 1f;
        float elapsedTime2 = 0;
        float totalDuration2 = Mathf.Abs(secondsPerStep2);

        Vector3 initialPosition2 = right.transform.localPosition;
        Vector3 targetPosition2 = new Vector3(right.transform.localPosition.x, right.transform.localPosition.y - 3f, right.transform.localPosition.z);
        while (elapsedTime2 < totalDuration2)
        {
            elapsedTime2 += Time.deltaTime;
            float lerpFactor = elapsedTime2 / totalDuration2;
            right.transform.localPosition = Vector3.Lerp(initialPosition2, targetPosition2, lerpFactor);
            yield return null;
        }

        // Ensure exact target values are met at the end of animation
        right.transform.localPosition = targetPosition2;
        yield return new WaitForSeconds(0.1f);
    }

    #endregion


}
