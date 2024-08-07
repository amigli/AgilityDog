using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Object = System.Object;

public class AgilityDog : Agent
{
    
    public float gravity = 20.0f;
    public float jumpHeight = 1f;
    public float force = 0.3f;

    private bool isReady;

    //Raycasy che percepiscono ostacoli e ricompense
    public RayPerceptionSensorComponent3D raycast;
    
    Rigidbody r;
    bool grounded = false;
    Vector3 defaultScale;

    
    
    
    public override void Initialize()
    {
        r = GetComponent<Rigidbody>();
        r.constraints = RigidbodyConstraints.FreezePositionZ;
        r.freezeRotation = true;
        //r.useGravity = false;
        defaultScale = transform.localScale;
        
    }
    
    
    public override void OnEpisodeBegin()
    {
        isReady = true;
    }
    
    
    public override void CollectObservations(VectorSensor sensor)
    {

        bool alberoLeft = false;
        bool alberoRight = false; 
        bool ostacolo = false;
        
        if (raycast != null)
        {
            // Get ray perception results
            RayPerceptionInput rayInput = raycast.GetRayPerceptionInput();
            RayPerceptionOutput rayOutput = RayPerceptionSensor.Perceive(rayInput);

            foreach (var ray in rayOutput.RayOutputs)
            {
                if (ray.HitTaggedObject)
                {
                    Debug.Log("Tag of hit object: " + ray.HitGameObject.tag);
                    
                    // Check if the hit object has the tag "AlberoLeft"
                    if (ray.HitGameObject.CompareTag("AlberoLeft"))
                    {
                        // Calculate the distance from the agent to the hit object
                        //float distance = Vector3.Distance(transform.position, ray.HitGameObject.transform.position);
                        //Debug.Log("Distance to AlberoLeft: " + distance);
                        alberoLeft = true;
                        MoveRight();
                    }

                    if (ray.HitGameObject.CompareTag("AlberoRight"))
                    {
                        alberoRight = true;
                        MoveLeft();
                    }

                    if (ray.HitGameObject.CompareTag("Finish"))
                    {
                        ostacolo = true;
                        Jump();
                    }
                    
                }
            }
        }
        else
        {
            Debug.LogError("RayPerceptionSensorComponent3D is not set up properly.");
        }
                    
        //sensor.AddObservation(alberoLeft);
        //sensor.AddObservation(alberoRight);
        //sensor.AddObservation(ostacolo);
    } 

    
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Vai a destra
        if (actions.DiscreteActions[0] == 0)
            MoveRight();

        //Vai a sinistra
        if (actions.DiscreteActions[0] == 1)
            MoveLeft();
        
        //Salta
        if(actions.DiscreteActions[0] == 2)
            Jump();
        
    }
    
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        // Resetto tutti i valori a 0 di default
        discreteActions[0] = -10000;

        if (Input.GetKey(KeyCode.RightArrow) == true)
            discreteActions[0] = 0;
        
        if (Input.GetKey(KeyCode.LeftArrow) == true)
            discreteActions[0] = 1;

        if (Input.GetKey(KeyCode.Space) == true)
            discreteActions[0] = 2;

    }

    
    private void OnCollisionStay(Collision collision)
    {
        
        //Premio per i prosciutti
        if (collision.gameObject.CompareTag("Ricompensa") == true)
        {
            //Debug.Log("Ricompensa");
            AddReward(0.5f);
            SC_GroundGenerator.instance.score += 2;
            //Destroy(collision.gameObject);
            collision.gameObject.SetActive(false);
        }
        
        //Penalità se fa la collisione con muro laterale o salta in presenza di
        //una ricompensa a terra
        if (collision.gameObject.CompareTag("Wall") == true)
        {
            //Debug.Log("Collisione con muro laterale o penalità salto con albero");
            AddReward(-0.3f);
            //collision.gameObject.SetActive(false);
            SC_GroundGenerator.instance.score += -1;
            //EndEpisode();
        }

        //Penalità se va a sbattere contro un ostacolo
        if (collision.gameObject.CompareTag("Finish") == true ||
            collision.gameObject.CompareTag("AlberoLeft") == true ||
            collision.gameObject.CompareTag("AlberoRight") == true)
        {
            //Debug.Log("Collisione con ostacolo o alberi");
            AddReward(-0.5f);
            Debug.Log("Sono nella rilevazione ostacolo");
            //EndEpisode();
            //SC_GroundGenerator.instance.gameOver = true;
        }
        
        if (collision.gameObject.CompareTag("Piattaforma") == true)
        {
            isReady = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Piattaforma") == true)
        {
            isReady = true;
        }
    }


    void Jump()
    {
        if (isReady)
        {
            isReady = false;
            r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
        }
    }
    
    
    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    
    void MoveRight()
    {
        if (isReady)
        {
            r.AddForce(Vector3.right * force, ForceMode.Impulse);
            //transform.position = new Vector3(transform.position.x + 2.0f, transform.position.y, transform.position.z);
            //transform.position += new Vector3(+7.0f * Time.deltaTime, 0f, 0f);
            //r.AddForce(right * force, ForceMode.Impulse);
        }
    }

    
    void MoveLeft()
    {
        if (isReady)
        {
            r.AddForce(Vector3.left * force, ForceMode.Impulse);
            //transform.position = new Vector3(transform.position.x - 2.0f, transform.position.y, transform.position.z);
            //transform.position += new Vector3(-7.0f * Time.deltaTime, 0f, 0f);
            //r.AddForce(left * force, ForceMode.Impulse);

        }
    }
    
    
    void FixedUpdate()
    {
        // We apply gravity manually for more tuning control
        //r.AddForce(new Vector3(0, -gravity * r.mass, 0));

        //RequestDecision();
        
        grounded = false;
    }
    
   
}
