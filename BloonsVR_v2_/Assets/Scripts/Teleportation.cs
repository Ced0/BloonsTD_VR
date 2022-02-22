using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Teleportation : MonoBehaviour
{
    public Transform bodyProj;
    public CapsuleCollider bodyCollider;

    public string hand;
    public GameObject laser;
    public Transform laserStart;
    public Transform player;
    public Transform camera;

    private Vector3 teleportPoint;
    private bool teleporting = false;
    public float teleportMaxDist;
    private Vector3 teleportDir;
    private float currentVelocity;
    private float distance;

    private bool pressed = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(teleporting == false)
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

                bool triggerValue;

                //Display laser and future position when trigger pressed
                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
                {
                    pressed = true;

                    
                    int layerMask = 1 << 3 + 1 << 7;//Don't detect hand and player
                    layerMask = ~layerMask;

                    RaycastHit hit;
                    Vector3 collisionPoint;

                    //Hand pointing raycast
                    if (Physics.Raycast(laserStart.position, laserStart.forward, out hit, teleportMaxDist, layerMask))
                    {
                        collisionPoint = hit.point;
                        
                        laser.transform.localScale = new Vector3(laser.transform.localScale.x, hit.distance * 3 + 1, laser.transform.localScale.z);

                        if(hit.collider.gameObject.tag != "floor")
                        {
                            floorDetection(ref collisionPoint);
                        }

                    }else{//If there was no collision so use max teleport range
                        collisionPoint = teleportMaxDist * laserStart.forward + laserStart.position;

                        floorDetection(ref collisionPoint);

                        laser.transform.localScale = new Vector3(laser.transform.localScale.x, teleportMaxDist*3 + 1, laser.transform.localScale.z);
                    }

                    layerMask = 1 << 3 + 1 << 7;//Don't detect hand and player
                    layerMask = ~layerMask;

                    //Needs a dummy direction different than zero to detect initial collision
                    RaycastHit[] allHits = Physics.CapsuleCastAll(collisionPoint + new Vector3(0, bodyCollider.radius, 0), collisionPoint + new Vector3(0, bodyCollider.height-bodyCollider.radius, 0),
                    bodyCollider.radius, new Vector3(0, 1, 0), 0f, layerMask);

                    Vector3 collisionAvoider = Vector3.zero;

                    for(int i  = 0; i < allHits.Length; i++)
                    {
                        //Distance equal 0 if collision was before movement
                        if(allHits[i].distance == 0)
                        {
                            Vector3 dir;
                            float dist;

                            //Compute vector needed avoid collision
                            Physics.ComputePenetration(bodyCollider, collisionPoint, Quaternion.identity, allHits[i].collider, allHits[i].collider.transform.position, Quaternion.identity, out dir, out dist);
                        
                            dir *= dist;

                            if(Mathf.Abs(collisionAvoider.x) < Mathf.Abs(dir.x))
                            {
                                collisionAvoider.x = dir.x;
                            }

                            if(Mathf.Abs(collisionAvoider.y) < Mathf.Abs(dir.y))
                            {
                                collisionAvoider.y = dir.y;
                            }

                            if(Mathf.Abs(collisionAvoider.z) < Mathf.Abs(dir.z))
                            {
                                collisionAvoider.z = dir.z;
                            }
                        }
                    }

                    //Activate ghost and laser
                    laser.SetActive(true);
                    bodyProj.gameObject.SetActive(true);

                    teleportPoint = collisionPoint + collisionAvoider;
                    bodyProj.position = collisionPoint + collisionAvoider + new Vector3(0, 0.5f, 0);

                    //Adjust the start position of the laser to match the scale
                    laser.transform.localPosition = new Vector3(0, 0, laser.transform.localScale.y-1);

                }
                else if(pressed == true)//When released, start dash/teleport
                {   
                    //Use the position of the camera and not the center of the room
                    teleportPoint -= new Vector3(camera.localPosition.x, 0, camera.localPosition.z);

                    distance = Vector3.Distance(teleportPoint, player.position);

                    teleportDir = (teleportPoint - player.position).normalized;

                    //Deactivate ghost and laser
                    bodyProj.gameObject.SetActive(false);
                    laser.SetActive(false);

                    teleporting = true;
                    bodyCollider.enabled = false;//We dont collide during the dash
                    
                    pressed = false;
                    
                }
            }
            else if(devices.Count > 1)
            {
                Debug.Log("Found more than one left hand!");
            }

        }else{

            //Interpolation to create a dash 
            //20 meter in 2 seconds
            player.position += Mathf.SmoothDamp(0, distance, ref currentVelocity, distance*0.1f) * teleportDir;

            if((teleportPoint - player.position).sqrMagnitude < 0.5f)
            {
                teleporting = false;
                bodyCollider.enabled = true;//Reactivate collision when finished
            }

        }
    }

    private void floorDetection(ref Vector3 collisionPoint)
    {
        int layerMask = 1 << 8;//Floor layer
        RaycastHit hit;

        if (Physics.Raycast(collisionPoint, -Vector3.up, out hit, teleportMaxDist, layerMask))
        {
            collisionPoint = hit.point;
        }
    }
}
