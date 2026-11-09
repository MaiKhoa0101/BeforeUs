using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Enemy : MonoBehaviour
{
    
    public Transform Character;

    public Animator Char_animator;
    public float rand_speed;

    public Vector3 chg_scale= new Vector3(0.01f, 0.01f, 0.01f);
    public Vector3 defpos = new Vector3(-5, -2.5f, 0);
    public Vector3 defscale = new Vector3(0.04f, 0.04f, 0.04f);
    
    
    void Start()
    {
        Char_animator = gameObject.GetComponent<Animator>();

        Character = gameObject.GetComponent<Transform>();
        Character.transform.localScale = defscale;
        Character.transform.position = defpos;
        
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
        
        rand_speed = Random.Range(0.09f, 0.11f);
        Char_animator.speed = rand_speed;
        if (Input.GetKey("d")){
            Character.transform.eulerAngles = new (0, 0, 0);
            Character.transform.Translate(Vector3.right * Time.deltaTime * 2);
        }
        if (Input.GetKey("a"))
        {
            Character.transform.eulerAngles = new Vector3(0,180,0);
            if (Character.position.x > -5)
            {
                Character.transform.Translate(Vector3.right * Time.deltaTime * 2);
            }
            else
            {
                Character.transform.Translate(Vector3.right * Time.deltaTime * 2);
            }
        }
        if (Input.GetKey("w"))
        {
            Character.transform.Translate(Vector3.up * Time.deltaTime * 2);
            
        }
        if (Input.GetKey("s"))
        {
            Character.transform.Translate(Vector3.down * Time.deltaTime * 2);
            
        }

        if (Input.GetKey("space"))
        {
            Character.transform.eulerAngles = Vector3.up;
            Character.transform.Translate(Vector3.up * Time.deltaTime * 10);
            Character.transform.eulerAngles = Vector3.down;
            Character.transform.Translate(Vector3.down * Time.deltaTime * 10);
        }

        if (Character.position.x < -6 || Character.position.x > 5)
        {
            Character.transform.Translate(-defscale*Time.deltaTime*10);
        }


        //Character.transform.Translate(Vector3.right * Time.deltaTime * 7 *rand_speed);
    }


}






/*
void OnGUI()
{
    //Create a Label in Game view for the Slider
    GUI.Label(new Rect(0, 25, 40, 60), "Speed");
    //Create a horizontal Slider to control the speed of the Animator. Drag the slider to 1 for normal speed.

    m_MySliderValue = GUI.HorizontalSlider(new Rect(45, 25, 200, 60), m_MySliderValue, 0.0F, 1.0F);
    //Make the speed of the Animator match the Slider value
    Char_animator.speed = 0.1f;
}
*/