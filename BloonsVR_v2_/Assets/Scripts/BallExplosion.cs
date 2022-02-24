using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallExplosion : MonoBehaviour
{
    public GameObject explosionEffect;
    [HideInInspector] public bool hasExploded = false;

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

            if(objCollider.tag == "Enemy")
            {
                /*if(objCollider.TryGetComponent(out EnemyController enemyController))
                {
                    enemyController.TakeDamage(15);
                }*/
            }
        }
    }
}
