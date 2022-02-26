using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(BoxCollider))]
//[RequireComponent(typeof(AudioSource))]
public class PlayerScript : MonoBehaviour
{


    [Tooltip("Amount of damage when attacking an ennemy")]
    private int DamageOnEnnemy;

    [Tooltip("Movement speed")]
    private float PlayerSpeed = 5;

    [Tooltip("True if the ennemy can move")]
    private bool CanMove = true;

    [Tooltip("True if the ennemy can attack the base")]
    private bool CanAttack = true;

    [SerializeField]
    [Tooltip("Game Camera pour le deplacement dans Unity")]
    Camera myCam;

    [Tooltip("True when holding a turret")]
    private bool IsHoldingATurret = false;


    // Start is called before the first frame update
    void Start()
    {
        InitValues();
        if (myCam == null)
        {
            throw new System.Exception("Camera is not defined in the player script !"); //a supprimer -> utiliser pour le deplacement
        }
    }

    internal void InitValues(int damageOnEnnemy = 5)
    {
        DamageOnEnnemy = damageOnEnnemy;
    }


    // Update is called once per frame
    void Update()
    {
        //a supprimer dans la VR
        if (CanMove) MovingPlayer();
        //a supprimer dans la VR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Physics.CheckSphere
            var e = GameObject.Find("Turret");
            if (IsHoldingATurret)
            {
                IsHoldingATurret = false;
            }
            else StartCoroutine(HoldTurret(e));
        }

    }

    //a supprimer dans la VR
    private IEnumerator HoldTurret(GameObject Turret = null)
    {
        IsHoldingATurret = true;
        Debug.Log("hold");
        while (IsHoldingATurret && Turret != null)
        {
            Vector3 res = transform.position + transform.forward*2 + transform.right*2;
            res.y = Turret.transform.position.y;
            Turret.transform.position = res;
            yield return new WaitForSeconds(2f*Time.deltaTime);
            Debug.Log(Time.deltaTime);
        }
    }


    //a supprimer dans la VR
    private void MovingPlayer()
    {
        float PlayerSensibility = 100;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z))
        {
            transform.position += Time.deltaTime * myCam.transform.forward * PlayerSpeed;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.position += Time.deltaTime * -myCam.transform.forward * PlayerSpeed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.eulerAngles += Vector3.up * Time.deltaTime * PlayerSensibility;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.eulerAngles -= Vector3.up * Time.deltaTime * PlayerSensibility;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= Time.deltaTime * myCam.transform.right * PlayerSpeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position += Time.deltaTime * myCam.transform.right * PlayerSpeed;
        }
    }
}

