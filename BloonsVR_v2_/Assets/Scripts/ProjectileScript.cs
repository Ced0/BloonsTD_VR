using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ProjectileScript : MonoBehaviour
{

    private int Damage = 5;
    private float Speed = 30.0f;
    private GameObject Target = null;
    private float DestroyOnImpactDelay = 0.0f;

    [Tooltip("All target touched ")]
    private List<GameObject> EnemiesTouched;

    [Tooltip("To keep direction of target until target die")]
    private Vector3 Direction;
    private void Start()
    {
        Destroy(transform.gameObject, 7.0f);
        InitSettings();
    }

    /// <summary>
    /// Initialize components settings
    /// </summary>
    private void InitSettings()
    {
        transform.GetComponent<BoxCollider>().isTrigger = true;
        var rb = transform.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.mass = 1;
        rb.drag = 0;
        rb.angularDrag = 0;
        rb.useGravity = false;
    }

    /// <summary>
    /// Initialise les valeurs du projectile
    /// </summary>
    /// <param name="target"></param>
    /// <param name="damage"></param>
    /// <param name="speed"></param>
    /// <param name="destroyOnImpactDelay"></param>
    public void InitValues(GameObject target, int damage = 5, float speed = 30.0f,  float destroyOnImpactDelay = 0.0f)
    {
        Damage = damage;
        Speed = speed;
        Target = target;
        DestroyOnImpactDelay = destroyOnImpactDelay;
        EnemiesTouched = new List<GameObject>();
    }

    void FixedUpdate()
    {
        if (Target != null)
        {
            Direction = Target.transform.position - transform.position; //suis la position de l ennemi
        }
        transform.position += Direction.normalized * Speed * Time.deltaTime; //continue meme si l ennemi n existe plus
    }

    /// <summary>
    /// On trigger -> Only on collision with ennemy, destory component and damage ennemy
    /// </summary>
    /// <param name="other">Object in collision</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnnemyScript>() != null && !EnemiesTouched.Contains(other.gameObject) && Target != null)
        {
            EnemiesTouched.Add(other.gameObject);
            other.GetComponent<EnnemyScript>().SetDamage(Damage);
            Destroy(transform.gameObject, DestroyOnImpactDelay);
        }
    }
}
