using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
public class EnnemyScript : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    [Tooltip("Amount of hitpoints before dying")]
    private int HitPoints;

    [Tooltip("Amount of max hitpoints")]
    private int MaxHitPoints;

    [Tooltip("Amount of damage when attacking the base")]
    private int DamageOnBase;

    [Tooltip("Money earned when when kill an ennemy")]
    private int MoneyOnDeath = 25;

    [Tooltip("True if the ennemy can move")]
    private bool CanMove = true;

    [Tooltip("True if the ennemy can attack the base")]
    private bool CanAttack = true;

    [Tooltip("Position of the base")]
    private Vector3 Destination;

    [Tooltip("Barre de vie")]
    private Slider slider;

    private void Awake()
    {
        //InitValues();
        slider = GetComponentInChildren<Transform>().GetComponentInChildren<Slider>();
        if (slider == null) throw new Exception("Put a canvas and a slider as children.");
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        //navMeshAgent
    }

    public bool IsDead() => HitPoints <= 0;
    
    /// <summary>
    /// On damaging an ennemy
    /// </summary>
    /// <param name="damage">Amount of damage taken</param>
    public void SetDamage(int damage)
    {
        HitPoints -= damage;
        slider.value = Math.Abs(HitPoints);
        if (HitPoints <= 0)
        {
            HitPoints = 0;
            //GetGameManager.gameManager.OnEnnemyDeath(transform.gameObject, MoneyOnDeath);
            CanMove = false;
            CanAttack = false;
        }
    }

    internal void InitValues(Vector3 destination, int hitpoints = 100, int damageOnBase = 10, float speed = 5f, int moneyOnDeath = 50)
    {
        HitPoints = hitpoints;
        MaxHitPoints = hitpoints;
        slider.maxValue = MaxHitPoints;
        slider.value = MaxHitPoints;
        DamageOnBase = damageOnBase;
        Destination = destination;
        navMeshAgent.speed = speed;
        navMeshAgent.SetDestination(Destination);
        MoneyOnDeath = moneyOnDeath;
    }

    private void FixedUpdate()
    {
        //movement a remplacer par pathfind 
        if (Destination != null && CanMove)
        {
            //Vector3.MoveTowards(transform.position, Destination, Time.deltaTime * 5.0f);
            //Vector3 direction = new Vector3( Destination.x - transform.position.x, 0, Destination.z - transform.position.z ).normalized;
            //Vector3 direction = ( Destination - transform.position).normalized;
            //transform.position += Time.deltaTime * 5.0f * direction;
        }

        AttackTheBase();
    }

    /// <summary>
    /// Si proche a moins de 2m de la base, informe le jeu qu'un ennemi a atteint la base
    /// </summary>
    private void AttackTheBase()
    {
        if (Destination == null) return;
        if (2 > (transform.position - Destination).magnitude && CanAttack)
        {
            CanAttack = false;
            GetGameManager.gameManager.EnnemyReachedDestination(transform.gameObject, DamageOnBase);
        }
    }
}
