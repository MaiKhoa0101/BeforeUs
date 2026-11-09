using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnMobs : MonoBehaviour
{
    public GameObject mobs;
    public float SpawnRate=2;
    private float timer = 0;
    private int Population = 3;
    private int NumberOfMob;

    void Start()
    {
        
        NumberOfMob = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (timer > SpawnRate){
            SpawnNewMob(1);
            timer = 0;
            if (NumberOfMob > Population)
            {

                NumberOfMob -= 1;
            }
        }
        else
        {
            timer += Time.deltaTime ;
        }
    }

    void SpawnNewMob(int number)
    {
        Instantiate(mobs, transform.position, transform.rotation);
        NumberOfMob += 1;
        
    }
    
}
