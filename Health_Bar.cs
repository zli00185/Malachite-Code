using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UI;

public class Health_Bar : MonoBehaviour
{
    private DeadScene deadScene;

    private PlayerStatus status;

    private Stamina_Bar stamina;
    // Start is called before the first frame update
    public Slider slider;
    public float guardLimit;
    public float angle;
    float cur;
    public GameObject HealMark;

    public AudioSource blockHit;
    public AudioSource takeHit;
    public AudioSource die;

    public Transform head;
    public Transform body;
    public Transform left;
    public Transform right;
    public Transform shield;
    public Transform sword;

    private Rigidbody rb_head;
    private Rigidbody rb_body;
    private Rigidbody rb_left;
    private Rigidbody rb_right;
    private Rigidbody rb_shield;
    private Rigidbody rb_sword;

    private Collider Co_head;
    private Collider Co_body;
    private Collider Co_left;
    private Collider Co_right;
    private Collider Co_shield;
    private Collider Co_sword;

    void Start()
    {
        slider.maxValue = 100;
        slider.value = 100;
        cur = slider.maxValue;
        deadScene = GameObject.Find("UI").GetComponent<DeadScene>();
        status = GameObject.Find("Player").GetComponent<PlayerStatus>();
        stamina = GameObject.Find("UI").GetComponent<Stamina_Bar>();

        //body part
        rb_head = head.GetComponent<Rigidbody>();
        rb_body = body.GetComponent<Rigidbody>();
        rb_left = left.GetComponent<Rigidbody>();
        rb_right = right.GetComponent<Rigidbody>();
        rb_shield = shield.GetComponent<Rigidbody>();
        rb_sword = sword.GetComponent<Rigidbody>();

        Co_head = head.GetComponent<Collider>();
        Co_body = body.GetComponent<Collider>();
        Co_left = left.GetComponent<Collider>();
        Co_right = right.GetComponent<Collider>();
        Co_shield = shield.GetComponent<Collider>();
        Co_sword = sword.GetComponent<Collider>();

        rb_head.isKinematic = true;
        rb_body.isKinematic = true;
        rb_left.isKinematic = true;
        rb_right.isKinematic = true;
        rb_shield.isKinematic = true;
        rb_sword.isKinematic = true;


        //Disactive the collider
        Co_head.enabled = false;
        Co_body.enabled = false;
        Co_left.enabled = false;
        Co_right.enabled = false;
        Co_shield.enabled = false;
        Co_sword.enabled = false;

        HealMark.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (cur > 0)
        {
            slider.value = cur;
        }
        else
        {
            cur = 0;
            slider.value = 0;
            Debug.Log("Player dead!");
            //Game over scene active
            dead();
        }


        var fillColor = slider.fillRect.GetComponent<Image>();
        // Set color to yellow  
        if (cur >= 0.5*slider.maxValue)
        {
            fillColor.color = Color.green;
        }
        else
        {
            fillColor.color = Color.red;
        }

    }

    public void GetHitByDamage(int damage, Transform source)
    {
        bool shielded = status.IsShielded();
        Vector2 sourceVec = new Vector2(source.position.x - body.position.x, source.position.z - body.position.z);
        Vector2 forwardVec = new Vector2(body.forward.x, body.forward.z);

        angle = Vector2.Angle(sourceVec, forwardVec);

        if(shielded && (angle <= guardLimit))
        {
            
            stamina.CostStaminaForShielded(damage*2);
            blockHit.Play();
        }
        else
        {
            cur = slider.value - damage;
            Debug.Log("current health: " + slider.value);
            takeHit.Play();
            if (cur <= 0)
            {
                die.Play();
            }

            Debug.Log("current health: " + cur);
        }
       
        //playerstatus.getHit();
    }

    public void Heal(int amount)
    {
        bool isDead = status.isDead();
        if (!isDead)
        {
            HealMark.SetActive(true);
            if (cur + amount > 100)
            {
                cur = 100;
            }
            else
            {
                cur += amount;
            }
            Invoke(nameof(DisableHealMark), 1.5f);
        }
        
    }

    private void DisableHealMark()
    {
        HealMark.SetActive(false);
    }

    private void dead()
    {

        status.PlayerDie();

        head.SetParent(null, true);
        body.SetParent(null, true);
        left.SetParent(null, true);
        right.SetParent(null, true);
        shield.SetParent(null, true);
        sword.SetParent(null, true);

        Co_head.enabled = true;
        Co_body.enabled = true;
        Co_left.enabled = true;
        Co_right.enabled = true;
        Co_shield.enabled = true;
        Co_sword.enabled = true;

        rb_head.isKinematic = false;
        rb_body.isKinematic = false;
        rb_left.isKinematic = false;
        rb_right.isKinematic = false;
        rb_shield.isKinematic = false;
        rb_sword.isKinematic = false;

        rb_head.useGravity = true;
        rb_body.useGravity = true;
        rb_left.useGravity = true;
        rb_right.useGravity = true;
        rb_shield.useGravity = true;
        rb_sword.useGravity = true;

        //end Scene here
        deadScene.deadSceneActivate();
    }
}