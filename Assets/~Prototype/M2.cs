using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class M2 : MonoBehaviour
{

    public Transform Character;

    public Animator Char_animator;
    public float rand_speed;

    public Vector3 defpos = new (-4, -2f, 0);
    public Vector3 defscale = new (0.1f, 0.1f, 0.1f);



    void Start()
    {
        Char_animator = gameObject.GetComponent<Animator>();

        Character = gameObject.GetComponent<Transform>();
        Character.transform.localScale = defscale;
        Character.transform.position = defpos;

        InvokeRepeating("movingaround",0, 0.1f);

    }

    void Update()
    {

    }


    void FixedUpdate()
    {
        //movingaround();
    }


    void movingaround()
    {
        rand_speed = Random.Range(0.085f, 0.13f);
        Char_animator.speed = rand_speed;
        if (Character.position.x < -6 || Character.position.x > 6)
        {
            Character.Rotate(0, 180, 0);
            Character.transform.Translate(Vector3.right * Time.deltaTime * 7 * rand_speed);

        }


        Character.transform.Translate(Vector3.right * Time.deltaTime * 7 * rand_speed);
    }

    void Changestatus()
    {
        //
    }



}


