using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CatchThrow : MonoBehaviour
{
    //Common
    private GameObject ball;

    //Calling Ball 
    public Transform ballContainer;
    private bool callingBall = false;
    private SphereCollider collider;
    private float initialDistance;
    private Vector3 callDirection;
    private float currentVelocity;

    //Throwing Ball
    private Rigidbody ballRigidbody;
    private bool inHand = false;
    private List<float> velocityX;
    private List<float> velocityY;
    private List<float> velocityZ;
    private int maxSample;
    private float sampleTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        maxSample = (int)(sampleTime/0.02);

        velocityX = new List<float>();
        velocityY = new List<float>();
        velocityZ = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        var leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);

        if(leftHandDevices.Count == 1)
        {
            InputDevice device = leftHandDevices[0];

            bool triggerValue;

            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
            {
                if(inHand == true)
                {
                    ballRigidbody.MovePosition(transform.position);
                }
                else if(callingBall == false)
                {
                    tryCallBall();

                }else{//Calling the ball

                    callBall(ref device);
                }
            }else{
                //If we stop calling the ball
                if(callingBall == true)
                {
                    callingBall = false;
                    collider.enabled = true;
                    ball = null;
                    ballRigidbody.velocity = new Vector3(0, 0, 0);
                }
                else if(inHand == true)//if we are thowring/dropping the ball
                {
                    collider.enabled = true;
                    ball = null;
                    inHand = false;
                    ballRigidbody.isKinematic = false;

                    //Compute some throw velocity
                    ballRigidbody.velocity = getVelocityMedian();
                }
            }
        }
        else if(leftHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }
    }

    //Take measures of the velocity at a regular timing
    void FixedUpdate()
    {
        if(inHand == true)
        {
            appendVelocity(ballRigidbody.velocity);
        }
    }

    //Look if were pointing approximately at a ball
    private void tryCallBall()
    {
        int i = 0;
        bool found = false;

        while(found  == false && i < ballContainer.childCount)
        {
            Vector3 handToBall = ballContainer.GetChild(i).transform.position - transform.position;
            
            float angle = Vector3.Angle(handToBall, transform.forward);

            //If the hand points at a less than 30 degree angle at the ball
            //We call it to the hand
            if(Mathf.Abs(Vector3.Angle(handToBall, transform.forward)) < 30)
            {
                ball = ballContainer.GetChild(i).gameObject;
                callingBall = true;

                //Deactivate collision while calling it 
                collider = ball.GetComponent<SphereCollider>();
                collider.enabled = false;
                ballRigidbody = ball.GetComponent<Rigidbody>();
                ballRigidbody.isKinematic = true;

                callDirection = transform.position - ball.transform.position;
                initialDistance = callDirection.magnitude;
                callDirection = callDirection.normalized;

                found = true;
            }

            i++;
        }
    }

    //Interpolation to get the ball to the hand in x amount of time 
    //no matter if the player moves after calling the ball
    private void callBall(ref InputDevice device)
    {
        //Update direction to match new hands position
        callDirection = (transform.position - ball.transform.position).normalized;

        ball.transform.position += Mathf.SmoothDamp(0, initialDistance, ref currentVelocity, initialDistance*0.025f) * callDirection;

        if((transform.position - ball.transform.position).sqrMagnitude < 0.5f)
        {
            callingBall = false;
            inHand = true;
            ballRigidbody.isKinematic = true;

            //Try to send vibration
            UnityEngine.XR.HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    float amplitude = 1f;
                    float duration = 0.25f;
                    device.SendHapticImpulse(channel, amplitude, duration);
                }else{
                    Debug.Log("doesn't do haptic");
                }
            }
        }
    }

    private void appendVelocity(Vector3 velocity)
    {
        if(velocityX.Count >= maxSample)
        {
            popVelocity();
        }

        velocityX.Add(velocity.x);
        velocityY.Add(velocity.y);
        velocityZ.Add(velocity.z);
    }

    private void popVelocity()
    {
        velocityX.RemoveAt(0);
        velocityY.RemoveAt(0);
        velocityZ.RemoveAt(0);
    }

    private Vector3 getVelocityMedian()
    {
        Vector3 velocity = new Vector3(0, 0, 0);
        
        //Compute somekind weitghted average but without dividing by total weight
        float x = 0;
        float y = 0;
        float z = 0;

        float sum = 0;

        int middle = (int)(velocityX.Count/2) + velocityX.Count%2;

        for(int i = 0; i < velocityX.Count; i++)
        {
            //weight = (i/1.3-count/2)² + (count/2)²
            float weight = -((i/1.3f - middle)*(i/1.3f - middle)) + middle*middle;

            sum += weight;

            x += velocityX[i] * weight;
            y += velocityY[i] * weight;
            z += velocityZ[i] * weight;
        }

        x /= velocityX.Count;
        y /= velocityY.Count;
        z /= velocityZ.Count;

        velocity = new Vector3(x, y, z);

        //Average the computed velocity with the median to maybe reduce a little bit the imprecision of the sensors
        //Probably not usefull
        velocityX.Sort();
        velocityY.Sort();
        velocityZ.Sort();

        velocity = (velocity + new Vector3(velocityX[(int)(velocityX.Count/2)], velocityY[(int)(velocityX.Count/2)], velocityZ[(int)(velocityX.Count/2)]))/2;

        velocityX.Clear();
        velocityY.Clear();
        velocityZ.Clear();

        return velocity;
    }
}
