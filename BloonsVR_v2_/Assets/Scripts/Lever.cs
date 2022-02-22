using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Lever : MonoBehaviour
{
    public Transform pivotPoint;
    private bool position = false;

    public Transform leftHand;
    private bool leftHandOn = false;
    private bool leftHandInteracting = false;
    private bool leftHandReleased = true;

    public Transform rightHand;
    private bool rightHandOn = false;
    private bool rightHandInteracting = false;
    private bool rightHandReleased = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //Left hand
        var devices = new List<InputDevice>();
        
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);

        if(devices.Count == 1)
        {
            InputDevice device = devices[0];

            //Handles interactions for the left hand
            interaction(in leftHand, in device, in leftHandOn, ref leftHandInteracting, ref leftHandReleased);

        }
        else if(devices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }else{
            leftHandInteracting = false;
        }

        
        //Right hand
        devices = new List<InputDevice>();
        
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if(devices.Count == 1)
        {
            InputDevice device = devices[0];

            //Handles interactions for the right hand
            interaction(in rightHand, in device, in rightHandOn, ref rightHandInteracting, ref rightHandReleased);

        }
        else if(devices.Count > 1)
        {
            Debug.Log("Found more than one right hand!");
        }else{
            rightHandInteracting = false;
        }


        //If no hand is interacting reset lever to it's last locked position
        if(leftHandInteracting == false && rightHandInteracting == false)
        {//Lever dropped
            if(position == false)
            {
                pivotPoint.localEulerAngles = new Vector3(45, 0, 0);
            }else{
                pivotPoint.localEulerAngles = new Vector3(-45, 0, 0);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "leftHand")
        {
            leftHandOn = true;
        }else if(other.gameObject.tag == "rightHand")
        {
            rightHandOn = true;
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "leftHand")
        {
            leftHandOn = false;
        }else if(other.gameObject.tag == "rightHand")
        {
            rightHandOn = false;
        } 
    }

    //Handles interactions
    private void interaction(in Transform hand, in InputDevice device, in bool handOn, ref bool interacting, ref bool released)
    {

        bool triggerValue;

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
        {
            //If the hand is interacting with did not reached any lock position since it took it or 
            //or is trying to grab it right now and has released the trigger since it last locked a position 
            if((handOn == true && released == true) || interacting == true)
            {
                interacting = true;
                released = false;

                Vector3 upForward = (transform.up + transform.forward);
                Vector3 relativeHandPos = (hand.position - transform.position);

                //Converts hand relative pos 3d vector and flatten it on 2d plan of the local z and y axis 
                //allowing us to calculate the 2d angle of the rotation on local x axis
                pivotPoint.localEulerAngles = new Vector3(Vector3.SignedAngle(new Vector3(relativeHandPos.x * upForward.x, relativeHandPos.y * upForward.y, relativeHandPos.z * upForward.z), transform.up, -transform.right), 0, 0);

                float angle  = pivotPoint.localEulerAngles.x > 180 ? pivotPoint.localEulerAngles.x-360 : pivotPoint.localEulerAngles.x;

                //If the lever has reached the threshold for the other position
                if(angle < -20 && position == false)
                {
                    pivotPoint.localEulerAngles = new Vector3(-45, 0, 0);
                    position = true;
                    interacting = false;

                }else if(angle > 20 && position == true)
                {
                    pivotPoint.localEulerAngles = new Vector3(45, 0, 0);
                    position = false;
                    interacting = false;
                }

            }
        }else{
            interacting = false;
            released = true;
        }
    }

}
