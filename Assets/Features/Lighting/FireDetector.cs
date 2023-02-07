using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDetector : MonoBehaviour
{
    private float nextActionTime = 0.0f;
    public float periodOn = 1f;
    public float periodOff = 6f;
    private Light _light;
    public bool lightOn = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
    }

    void Update () {
        if (Time.time > nextActionTime) {
            if (!lightOn)
            {
                nextActionTime += periodOn;
                _light.enabled = true;
                lightOn = true;
            }
            else
            {
                nextActionTime += periodOff;
                _light.enabled = false;
                lightOn = false;
            }
        }
    }
}
