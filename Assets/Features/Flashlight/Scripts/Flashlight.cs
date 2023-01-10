using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    //Editor Available Variable
    [SerializeField] private bool drainBattery = true;
    [SerializeField] private float battery = 1f;
    [SerializeField] private float batteryMax = 1f;
    [SerializeField] private float batteryMin = 0f;
    [SerializeField] private float batteryDrainRate = 0.1f;
    [SerializeField] private float batteryRechargeRate = 0.1f;
    [SerializeField] private Light flashlightLight;
    public bool IsFlashlightOn => flashlightLight.enabled;

    //Private Variable

    //Lifecycle Methods
    private void Start()
    {
        // flashlightLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (flashlightLight.enabled && drainBattery)
        {
            if (battery > batteryMin)
            {
                DrainBattery();
            }
            else
            {
                flashlightLight.enabled = false;
            }
        }
        else
        {
            if (battery < batteryMax)
            {
                //Wait for delay                
                RechargeBattery();
            }
        }

        
    }

    //Methods
    public void ToggleFlashlight()
    {
        flashlightLight.enabled = !flashlightLight.enabled;

        if (battery <= 0)
        {
            flashlightLight.enabled = false;
        }
    }

    private void RechargeBattery()
    {
        battery += batteryRechargeRate * Time.deltaTime;
        battery = Mathf.Clamp(battery, batteryMin, batteryMax);
    }

    private void DrainBattery()
    {
        battery -= batteryDrainRate * Time.deltaTime;
        battery = Mathf.Clamp(battery, batteryMin, batteryMax);
    }

}
