using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject explosionEffect;
    [HideInInspector] public bool hasExploded = false;
    [HideInInspector] public Transform enemyLocked = null;
    public float gravityField;
    public Rigidbody body;
    public float maxDist;

    [HideInInspector] public bool isPulled = false;

    //Apply some kind of gravitational force field
    void Update()
    {
        if(enemyLocked != null && hasExploded == false && isPulled == false)
        {
            Vector3 dir = enemyLocked.position - transform.position;
            float dist = dir.magnitude/maxDist;
            dir = dir.normalized;
            body.velocity += dir * gravityField * dist;
        }
    }

    //If an enemy enter the trigger zone of the gravity field
    //and that we don't have an enemy locked in our gravity field
    //Then we add him
    private void OnTriggerEnter(Collider other)
    {
        if(enemyLocked == null && hasExploded == false && isPulled == false)
        {
            if(other.tag == "enemy")
            {
                enemyLocked = other.transform;
            }
        }
    }

    //Explode on any collision
    void OnCollisionEnter(Collision collision)
    {
        if(hasExploded == false)
        {
            hasExploded = true;
            Explode();
        }
    }

    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 7);

        Instantiate(explosionEffect, transform.position, transform.rotation);
        
        for(int i = 0; i < colliders.Length; i++)
        {
            GameObject objCollider = colliders[i].gameObject;

            //If is an enemy apply dammage
            if(objCollider.tag == "enemy")
            {
                if(objCollider.TryGetComponent(out EnnemyScript enemyController))
                {
                    enemyController.SetDamage(15);
                }
            }
        }
    }
}
