using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandVibration : MonoBehaviour
{
    public string hand;
    private byte vibrate = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //If the hand is in contact with a "vibration" object send vibrations
        if(vibrate > 0)
        {
            var devices = new List<InputDevice>();

            if(hand == "left")
            {
                InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            }else{
                InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            }


            if(devices.Count == 1)
            {
                InputDevice device = devices[0];

                UnityEngine.XR.HapticCapabilities capabilities;
                if (device.TryGetHapticCapabilities(out capabilities))
                {
                        if (capabilities.supportsImpulse)
                        {
                            uint channel = 0;
                            float amplitude = 0.5f;
                            float duration = 0.05f;
                            device.SendHapticImpulse(channel, amplitude, duration);
                        }else{
                            Debug.Log("doesn't do haptic");
                        }
                }
            
            }
            else if(devices.Count > 1)
            {
                Debug.Log("Found more than one left hand!");
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.gameObject.tag == "vibration")
        {
            vibrate++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if(other.gameObject.tag == "vibration")
        {
            vibrate--;
        }
    }
}
