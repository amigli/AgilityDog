using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class SC_IRPlayer : MonoBehaviour
{
    public float gravity = 20.0f;
    public float jumpHeight = 2.5f;
    public float force = 1.0f;
    
    
    Rigidbody r;
    bool grounded = false;
    Vector3 defaultScale;
    bool crouch = false;
    

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Rigidbody>();
        r.constraints = RigidbodyConstraints.FreezePositionZ;
        r.freezeRotation = true;
        r.useGravity = false;
        defaultScale = transform.localScale;
    }

    void Update()
    {
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            r.AddForce(Vector3.right * force, ForceMode.Impulse);
        }
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            r.AddForce(Vector3.left * force, ForceMode.Impulse);
        }
        
        /*
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, 0.0f);

        // Aggiungi una forza orizzontale al Rigidbody
        r.AddForce(movement * 2.0f);


        //Vai a destra
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //transform.Translate(Vector3.right * 1.0f * Time.deltaTime);
            r.AddForce(right * 3.0f, ForceMode.Impulse);
        }

        //Vai a sinistra
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * 1.0f * Time.deltaTime);
        }
        */

        //Crouch
        crouch = Input.GetKey(KeyCode.S);
        if (crouch)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(defaultScale.x, defaultScale.y * 0.4f, defaultScale.z), Time.deltaTime * 7);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, Time.deltaTime * 7);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // We apply gravity manually for more tuning control
        r.AddForce(new Vector3(0, -gravity * r.mass, 0));

        grounded = false;
    }

    void OnCollisionStay()
    {
        grounded = true;
    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Finish")
        {
            //print("GameOver!");
            SC_GroundGenerator.instance.gameOver = true;
        }
    }
}