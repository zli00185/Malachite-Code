using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class ProjectileCollisonCheck : MonoBehaviour
{
    private SlimeAi enemy_ai;
    private PlayerStatus playerStatus;
    private Health_Bar Health_Bar;
    private int damage;
    public int type;

    private void Start()
    {
        damage = 10;
        enemy_ai = GetComponent<SlimeAi>();
        playerStatus = GameObject.Find("Player").GetComponent<PlayerStatus>();
        Health_Bar = GameObject.Find("UI").GetComponent<Health_Bar>();

        if (type == 1)
        {
            
        }
        else if (type == 2)
        {

        }
        else if (type == 3)
        {

        }
        else
        {

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(gameObject.name + " get hit by " + other.tag);
        //Type 1 cause stun normal
        //Type 2 cause sink blue
        //Type 3 cause tired yellow
        //Else double damage 
        if (other.tag == "Player")
        {
            bool shield = playerStatus.IsShielded();
            if (shield)
            {
                if (type == 1)
                {
                    Health_Bar.GetHitByDamage(damage, transform);
                }
                else if(type == 2)
                {
                    Health_Bar.GetHitByDamage(damage, transform);
                }
                else if(type == 3)
                {
                    playerStatus.BecomeTired(3f);
                }
                else
                {
                    Health_Bar.GetHitByDamage(damage*2, transform);
                }
                

            }
            else
            {
                if (type == 1)
                {
                    playerStatus.getStun(1.5f);
                    Health_Bar.GetHitByDamage(damage, transform);
                }
                else if (type == 2)
                {
                    playerStatus.BecomeSink(3f);
                    Health_Bar.GetHitByDamage(damage, transform);
                }
                else if (type == 3)
                {
                    playerStatus.BecomeTired(3f);
                }
                else
                {
                    Health_Bar.GetHitByDamage(damage * 2, transform);
                }


               
            }
        }

        Debug.Log(gameObject.name + " get hit by " + other.name);
        Destroy(gameObject,0.2f);
       
    }
    private void OnTriggerStay(Collider other)
    {


    }
    private void OnTriggerExit(Collider other)
    {


    }
}
