using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TurretScript : MonoBehaviour
{
    private float RotationSpeed = 20.0f;
    private int Damage = 20;
    private float ProjectileSpeed = 30.0f;
    private float FireRate = 0.5f;
    private List<GameObject> EnemiesInZone = new List<GameObject>();
    private bool CanAtack = true;
    private GameObject CurrentTarget = null;
    private bool IsReloading = false;

    [SerializeField]
    private GameObject Projectile;


    // Start is called before the first frame update
    void Start()
    {
        InitSettings();
        StartCoroutine(RotateAndShoot());
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
    /// Allow to enable or disable turret shoot
    /// </summary>
    /// <param name="val">True to active shoot</param>
    public void SetCanAttack(bool val) => CanAtack = val;

    internal void InitValues(float range = 40)
    {
        transform.GetComponent<BoxCollider>().size = new Vector3(range, 10f, range);
        CurrentTarget = null;
        EnemiesInZone = new List<GameObject>();
    }

    private IEnumerator RotateAndShoot()
    {
        while (true)
        {
            if (CurrentTarget != null)
            {
                //face the Target
                var targetRotation = Quaternion.LookRotation(CurrentTarget.transform.position - transform.position);
                //var str = Mathf.Min(8 * Time.deltaTime, 1.0f);
                var str = Mathf.Min(RotationSpeed * Time.deltaTime, 1);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation * Quaternion.Euler(0,1,0), str);
                

                if (!IsReloading && CanAtack && Math.Abs(Quaternion.Dot(targetRotation, transform.rotation)) > 0.9f)
                {
                    //shoot
                    IsReloading = true;
                    var projectile = Instantiate<GameObject>(Projectile, transform.position, Quaternion.identity);
                    projectile.GetComponent<ProjectileScript>().InitValues(CurrentTarget, Damage, ProjectileSpeed);
                    StartCoroutine(Reload());
                } else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(FireRate);
        IsReloading = false;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateCurrentTarget();
        DeleteDeadtarget();
    }


    private void DeleteDeadtarget()
    {
        GameObject toDelete = null;
        //boucle pour enelever les target mortes
        foreach (var item in EnemiesInZone)
        {
            if (item == null)
            {
                EnemiesInZone.Remove(item);
                break;
            }
            var ennemyScript = item.GetComponent<EnnemyScript>();
            if (ennemyScript.IsDead())
            {
                toDelete = item;
                if (CurrentTarget == item) CurrentTarget = null;
                break;
            }
        }
        if (toDelete != null) EnemiesInZone.Remove(toDelete);
    }


    private void UpdateCurrentTarget()
    {
        if (EnemiesInZone.Count <= 0) return;
        if (CurrentTarget == null)
        {
            //boucle pour update la nouvelle target
            foreach (var item in EnemiesInZone)
            {
                if (item == null)
                {
                    EnemiesInZone.Remove(item);
                    break;
                }
                var ennemyScript = item.GetComponent<EnnemyScript>();
                if (!ennemyScript.IsDead())
                {
                    CurrentTarget = item;
                    break;
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.GetComponent<EnnemyScript>()) EnemiesInZone.Add(other.gameObject);
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.transform.GetComponent<EnnemyScript>())
        {
            EnemiesInZone.Remove(other.gameObject);
            if ( other.gameObject == CurrentTarget) CurrentTarget = null;
        }
    }


}
