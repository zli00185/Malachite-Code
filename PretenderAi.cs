using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class PretenderAi : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> projectile = new List<GameObject>();
    private Vector3 PlayerPosition;

    public Transform player;

    public NavMeshAgent agent;

    public LayerMask whatIsGround;

    Health_Bar Player_heath;

    PlayerStatus playerStatus;

    public GameObject exclamationMark;
    public GameObject ExplosionEffect;
    public GameObject RenderGameObeject;
    public GameObject ProjectileGroup;

    private float Enemy_health;
    public float Max_Enemy_health;

    //Animation
    Transform body;
    private float bounceRange = 1.25f;
    private float bounceSecs = 1.25f;
    private float rotationSpeed = 20;
    float bounce_change;
    float position_diff = 0;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public int ExpoldeDamage;
    Rigidbody rb;
    public float chaseSpeed;

    //States
    public float explodeRange, triggerRange, ProjectileSpeed;
    float OrignalPositionY;

    enum state
    {
        Hiding,
        Shooting,
        Exploding,
        Dead
    }

    state states = state.Hiding;

    private void Awake()
    {
        transform.SetParent(null);//so it will move out the folder once activate
        Player_heath = GameObject.Find("UI").GetComponent<Health_Bar>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").transform;
        playerStatus = GameObject.Find("Player").GetComponent<PlayerStatus>();
        agent = GetComponent<NavMeshAgent>();
        exclamationMark.SetActive(false);
        ExplosionEffect.SetActive(false);
        body = GetComponent<Transform>();
        Enemy_health = Max_Enemy_health;
        bounce_change = bounceRange / bounceSecs;

        states = state.Hiding;
        agent.updatePosition = false;

        if (projectile.Count > 0)
        {
            foreach (GameObject Ammo in projectile)
            {
                rb = Ammo.GetComponent<Rigidbody>();
                Collider Projectile_collider = Ammo.GetComponent<Collider>();
                rb.isKinematic = true;
                rb.useGravity = false;
                Projectile_collider.enabled = false;


            }
        }
        ProjectileGroup.SetActive(false);
        ProjectileGroup.transform.position = new Vector3(ProjectileGroup.transform.position.x, -3.5f, ProjectileGroup.transform.position.z);
        SphereCollider[] triggers = GetComponents<SphereCollider>();

        // attackRange
        triggers[0].radius = triggerRange;
        triggers[0].isTrigger = true;

        //explodeRange
        triggers[1].radius = explodeRange;
        triggers[1].isTrigger = true;

     
    }
  
    
    // Update is called once per frame
    void Update()
    { 

        #region logic

        if (Enemy_health <= 0)
        {
            states = state.Dead;
        }
        else if (playerStatus.isDead())
        {
            states = state.Hiding;
        }else if (projectile.Count == 0)
        {
            Invoke(nameof(ReadyToExploade),2f);
            
        }

        switch (states)
        {

            case state.Hiding:
                idleAnimation();
                break;

            case state.Shooting:
                AttackAnimation();
                transform.Rotate(0f, 0.3f, 0f);
                agent.updatePosition = true;
                EnableExclamationMark();
                PlayerPosition = player.position;
                //float number = Random.Range(0.5f, 3f);
                Invoke(nameof(Attacking), 1.5f);
                break;

            case state.Exploding:
                Enemy_health = Max_Enemy_health;
                ExpoldeAnimation();
                PlayerPosition = player.position;
                Chasing();
                break;

            case state.Dead:
                EnemyDead();
                break;

        }
#endregion

    }

    public void TriggerShootingState()
    {
        
        if(states == state.Hiding)
        {
            states = state.Shooting;
        }
        
    }
   

    #region Collison Check
    void OnTriggerEnter(Collider other)
    {

        bool isCharacterController = other.tag == "Player" &&
          other.GetType().ToString() == "UnityEngine.CharacterController";

        if (isCharacterController && states == state.Hiding)
        {
            StartCoroutine(WakeUpAnimation());
            states = state.Shooting;
        }

        // lowercase attack even though editor shows uppercase
        // Must be within enemy attack range to hit
        //    ^ You can exit range before animation finishes and you wont get hit
        if (other.tag == "Sword")
        {
            Debug.Log(gameObject.name + " get hit by " + other.tag);

            AudioSource hitBy = other.GetComponent<AudioSource>();
            if (hitBy != null)
            {
                hitBy.Play();
            }
            // Divide by 2 bc collision will happen on both colliders
            EnemyTakeDamage(playerStatus.getPlayerDamage() / 2);
        }
    }
    private void ReadyToExploade()
    {
        states = state.Exploding;
    }

    #endregion
    #region Chase&Expolde
    private void Chasing()
    {
        //show ExclamationMark
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        Invoke(nameof(Exploding),5f);
    }

    private void Exploding()
    {
        //Anim
        DisableExclamationMark();
        ExplosionEffect.SetActive(true);
        RenderGameObeject.GetComponent<Renderer>().enabled = false;
        ExplosionEffect.transform.LookAt(new Vector3(player.transform.position.x, body.position.y,player.transform.position.z));
        Vector3 range = new Vector3(player.position.x, 0, player.position.z) - new Vector3(body.transform.position.x, 0, body.transform.position.z);
        if (range.magnitude < explodeRange)
        {
            Player_heath.GetHitByDamage(ExpoldeDamage, transform);
        }
        else
        {
            //miss
        }
        explodeRange = 0;
        Invoke(nameof(OnDestroy), 0.7f);
    }

    private void OnDestroy()
    {
        agent.enabled = false;
        Destroy(gameObject);
    }
    #endregion
    #region UI
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
    #endregion
    #region Attack
    private void Attacking()
    {
        //EnableExclamationMark();
        EnableExclamationMark();
        if (!alreadyAttacked)
        {
            //Attack code
            Shoot();

            //delay of attack
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void Shoot()
    {
        if (projectile.Count > 0)
        {
            List<GameObject> firedProjectiles = new List<GameObject>();
           
            GameObject Ammo = projectile[0];
            Collider Projectile_collider = Ammo.GetComponent<Collider>();
            Projectile_collider.enabled = true;

            Ammo.transform.SetParent(null);
            Debug.Log("Shoot!");
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 direction = (targetPosition - Ammo.transform.position).normalized;  // Calculate direction to the player
            rb = Ammo.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            //rb.useGravity = true;

            rb.velocity = direction * ProjectileSpeed;
            firedProjectiles.Add(Ammo);
            Max_Enemy_health -= 10;


            foreach (GameObject fired in firedProjectiles)
            {
                projectile.Remove(fired);
                
            }
        }

        alreadyAttacked = true;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
#endregion
    public void EnemyTakeDamage(float damage)
    {
        Enemy_health = Enemy_health - damage;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
        Gizmos.color = Color.red; ;
        Gizmos.DrawWireSphere(transform.position, explodeRange);

    }

    #region Dead

    public GameObject HealItem;
    private void EnemyDead()
    {
        if (HealItem != null)
        {
            GameObject clone = Instantiate(HealItem);
            clone.transform.localPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);

        }
        agent.updatePosition = false;
        Renderer renderer = RenderGameObeject.GetComponent<Renderer>();
        Material mat = renderer.material;
        mat.SetColor("_EmissionColor", new Color(100 / 255f, 0 / 255f, 0 / 255f));
        DynamicGI.SetEmissive(renderer, new Color(100 / 255f, 0 / 255f, 0 / 255f));

        Invoke(nameof(DestroyObejct), 2f);

        }
    private void DestroyObejct()
    {
            Destroy(gameObject);
        
    }

    #endregion

    #region Animation
    IEnumerator WakeUpAnimation()
    {
        ProjectileGroup.SetActive(true);
        float secondsPerStep = 1.5f;
        float elapsedTime = 0;
        float totalDuration = Mathf.Abs(secondsPerStep);

        Vector3 initialPosition = ProjectileGroup.transform.localPosition;
        Vector3 targetPosition = new Vector3(ProjectileGroup.transform.localPosition.x, 0.5f, ProjectileGroup.transform.localPosition.z);
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpFactor = elapsedTime / totalDuration;
            ProjectileGroup.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, lerpFactor);
            yield return null;
        }

        // Ensure exact target values are met at the end of animation
        ProjectileGroup.transform.localPosition = targetPosition;
    }

    private void idleAnimation()
    {
        // Make it bounce
        body.localPosition += Vector3.up * bounce_change * Time.deltaTime;
        position_diff += bounce_change * Time.deltaTime;
        body.rotation *= Quaternion.Euler(Vector3.up * rotationSpeed * Time.deltaTime);
        if (Mathf.Abs(position_diff) > (bounceRange / 2))
        {
            bounce_change = -bounce_change;
        }

    }

    private void AttackAnimation()
    {
       
        Renderer renderer = RenderGameObeject.GetComponent<Renderer>();
        Material mat = renderer.material;
        mat.SetColor("_EmissionColor", new Color(255/255f, 160/255f, 0/255f)) ;
        DynamicGI.SetEmissive(renderer, new Color(255 / 255f, 160 / 255f, 0 / 255f));
        
    }
    private bool isBlinking = false;
    private float nextBlinkTime = 0f;
    private float BlinkTimeInterval = 0.8f;
    private void ExpoldeAnimation()
    {
        if (Time.time >= nextBlinkTime)
        {
            isBlinking = !isBlinking;
            Renderer renderer = RenderGameObeject.GetComponent<Renderer>();
            Material mat = renderer.material;
            mat.SetColor("_EmissionColor", isBlinking? new Color(250 / 255f, 0 / 255f, 0 / 255f): new Color(100/ 255f, 0 / 255f, 0 / 255f));
            DynamicGI.SetEmissive(renderer, isBlinking ? new Color(250 / 255f, 0 / 255f, 0 / 255f) : new Color(100 / 255f, 0 / 255f, 0 / 255f));
            BlinkTimeInterval = Mathf.Max(0.1f, BlinkTimeInterval * 0.8f); //decay
            nextBlinkTime = Time.time + BlinkTimeInterval;
        }
        
        
    }

   

    #endregion
}
