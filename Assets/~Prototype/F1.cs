using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class F1 : MonoBehaviour
{
    
    public Transform Character;
    public Animator Char_animator;

    GameObject character;
    public float rand_speed;
    public Vector3 defpos = new Vector3(-5, -1.5f, 0);
    public Vector3 defscale = new Vector3(0.08f, 0.08f, 0.08f);


    void Start()
    {
        
        Char_animator = gameObject.GetComponent<Animator>();

        Character = gameObject.GetComponent<Transform>();
        Character.transform.localScale = defscale;
        Character.transform.position = defpos;
        Debug.Log(Character.position);
        
        InvokeRepeating("movingaround", 0, 0.1f);    
    }

    void Update() {

    }


    void FixedUpdate()
    {
        //movingaround();
    }


    void movingaround()
    {
        rand_speed = Random.Range(0.08f, 0.11f);
        Char_animator.speed = rand_speed;
        if (Character.position.x < -6 || Character.position.x > 5)
        {
            Character.Rotate(0, 180, 0);
            Character.transform.Translate(Vector3.right * Time.deltaTime * 7 * rand_speed);
            
        }


        Character.transform.Translate(Vector3.right * Time.deltaTime * 7 * rand_speed);
    }


}


