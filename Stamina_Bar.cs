using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class Stamina_Bar : MonoBehaviour
{
    // Start is called before the first frame update

    private float timer = 0;
    public Slider slider;
    public float AttackStaminaCost = 15;
    public PlayerStatus status;
    private bool tired;
    private bool shield;

    void Start()
    {
        slider.maxValue = 100;
        slider.value = 100;
        status = GameObject.Find("Player").GetComponent<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        shield = status.IsShielded();

        if (slider.value < slider.maxValue)
        {
            timer += Time.deltaTime;
            if (timer >= 0.5f) // When 0.5 second has passed
            {
                if (slider.value<=50 && !shield) {
                    slider.value += 4;
                }
                else if(!shield) 
                {
                    slider.value += 2;
                    
                }
                timer = 0; // Reset the timer
            }
        }
        else
        {
            slider.value = 100;
        }

        tired = status.IsTired();
        var fillColor = slider.fillRect.GetComponent<Image>();
         // Set color to yellow
 
        if (tired)
        {
            fillColor.color = Color.yellow;
        }
        else
        {
            fillColor.color = Color.white;
        }

    }
    public void CostStaminaForAttack()
    {
        CostStamina(AttackStaminaCost);
    }

    public void CostStaminaForShielded(int damage)
    {
        CostStamina(damage);
    }

    private void CostStamina(float cost)
    {

        float cur = slider.value;
        if (cur - cost > 0)
        {
            slider.value = cur - cost;
        }
        else
        {
            Debug.Log("Tired!");
            slider.value = 0;
            status.BecomeTired(0);
        }
    }
    public float getCurrentStamina()
    {
        return slider.value;
    }
}

