using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlacingTurret : MonoBehaviour
{
    public GameObject turret;
    private CapsuleCollider turretCollider;
    private MeshRenderer turretMesh;

    public Material originalMat;
    public Material transparentMat;

    public string hand;
    public Transform laserStart;
    public GameObject laser;

    private bool pressed = false;
    public float maxDist = 20;

    // Start is called before the first frame update
    void Start()
    {
        //The value of turret should be set set when we pick it up

        //Collision should be disabled on pickup
        turretCollider = turret.GetComponent<CapsuleCollider>();
        turretCollider.enabled = false;

        //Change material
        turretMesh = turret.GetComponent<MeshRenderer>();
        turretMesh.material = transparentMat;

        //We should deactivate some script for shooting here

        //And activate the laser
        laser.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
        if(turret != null)
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
                int layerMask = 1 << 3 + 1 << 7;//Don't detect hand and player
                layerMask = ~layerMask;

                RaycastHit hit;
                Vector3 collisionPoint;

                //Hand pointing raycast
                if (Physics.Raycast(laserStart.position, laserStart.forward, out hit, maxDist, layerMask))
                {
                    collisionPoint = hit.point;
                    
                    laser.transform.localScale = new Vector3(laser.transform.localScale.x, hit.distance * 3 + 1, laser.transform.localScale.z);

                    if(hit.collider.gameObject.tag != "floor")
                    {
                        floorDetection(ref collisionPoint);
                    }

                }else{//If there was no collision so use max placing range
                    collisionPoint = maxDist * laserStart.forward + laserStart.position;

                    floorDetection(ref collisionPoint);

                    laser.transform.localScale = new Vector3(laser.transform.localScale.x, maxDist*3 + 1, laser.transform.localScale.z);
                }

                layerMask = 1 << 3 + 1 << 7;//Don't detect hand and player
                layerMask = ~layerMask;

                //Collider needs to be enabled to test collisions
                turretCollider.enabled = true;

                //Needs a dummy direction different than zero to detect initial collision
                RaycastHit[] allHits = Physics.CapsuleCastAll(collisionPoint + new Vector3(0, turretCollider.radius, 0), collisionPoint + new Vector3(0, turretCollider.height-turretCollider.radius, 0),
                    turretCollider.radius, new Vector3(0, 1, 0), 0f, layerMask);

                Vector3 collisionAvoider = Vector3.zero;

                for(int i  = 0; i < allHits.Length; i++)
                {
                    //Distance equal 0 if collision was before movement started
                    if(allHits[i].distance == 0 && allHits[i].collider.gameObject != turret)
                    {
                        Vector3 dir;
                        float dist;

                        //Compute vector needed to avoid collision
                        Physics.ComputePenetration(turretCollider, collisionPoint, Quaternion.identity, allHits[i].collider, allHits[i].collider.transform.position, Quaternion.identity, out dir, out dist);
                    
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

                //Turret should be a ghost so deactivate collider again
                turretCollider.enabled = false;

                turret.transform.position = collisionPoint + collisionAvoider + new Vector3(0, 0.5f, 0);

                //Adjust the start position of the laser to match the scale
                laser.transform.localPosition = new Vector3(0, 0, laser.transform.localScale.y-1);


                //Test inputs
                InputDevice device = devices[0];

                bool triggerValue;

                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
                {
                    pressed = true;
                }
                else if(pressed == true)//When released place the turret
                {
                    turretCollider.enabled = true;
                    turretMesh.material = originalMat;

                    //We should activate some script or something for shooting here
                    turret = null;

                    laser.SetActive(false);
                }
            }
            else if(devices.Count > 1)
            {
                Debug.Log("Found more than one left hand!");
            }
        }
    }

    private void floorDetection(ref Vector3 collisionPoint)
    {
        int layerMask = 1 << 8;//Floor layer
        RaycastHit hit;

        if (Physics.Raycast(collisionPoint, -Vector3.up, out hit, maxDist, layerMask))
        {
            collisionPoint = hit.point;
        }
    }
}
