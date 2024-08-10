using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgilityDog : Agent
{
    
    public float gravity = 20.0f;
    public float jumpHeight = 1f;
    public float force = 0.5f;

    private bool isReady = false;
    private bool isGrounded = false; 

    //Raycasy che percepiscono ostacoli e ricompense
    public RayPerceptionSensorComponent3D raycast;
    
    //Raycast che percepiscono i muri laterali
    public RayPerceptionSensorComponent3D wallsRaycast;
    
    Rigidbody r;
    bool grounded = false;
    Vector3 defaultScale;
    
    
    public override void Initialize()
    {
        r = GetComponent<Rigidbody>();
        r.constraints = RigidbodyConstraints.FreezePositionZ;
        r.freezeRotation = true;
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
        bool wallLeft = false;
        bool wallRight = false;
        
        if (raycast != null)
        {
            // Get ray perception results
            RayPerceptionInput rayInput = raycast.GetRayPerceptionInput();
            RayPerceptionOutput rayOutput = RayPerceptionSensor.Perceive(rayInput);

            foreach (var ray in rayOutput.RayOutputs)
            {
                if (ray.HitTaggedObject)
                {
                    //Debug.Log("Tag of hit object: " + ray.HitGameObject.tag);
                    
                    // Check if the hit object has the tag "AlberoLeft"
                    if (ray.HitGameObject.CompareTag("AlberoLeft"))
                    {
                        // Calculate the distance from the agent to the hit object
                        //float distance = Vector3.Distance(transform.position, ray.HitGameObject.transform.position);
                        //Debug.Log("Distance to AlberoLeft: " + distance);
                        alberoLeft = true;
                        //MoveRight();
                    }

                    if (ray.HitGameObject.CompareTag("AlberoRight"))
                    {
                        alberoRight = true;
                        //MoveLeft();
                    }

                    if (ray.HitGameObject.CompareTag("Finish"))
                    {
                        ostacolo = true;
                        //Jump();
                    }
                    
                }
            }
        }
        else
        {
            Debug.LogError("RayPerceptionSensorComponent3D is not set up properly.");
        }
                    
        sensor.AddObservation(alberoLeft ? 1f : 0f);
        sensor.AddObservation(alberoRight ? 2f : 0f);
        sensor.AddObservation(ostacolo ? 3f : 0f);
        
        if (wallsRaycast != null)
        {
            // Get ray perception results
            RayPerceptionInput rayInputWalls = wallsRaycast.GetRayPerceptionInput();
            RayPerceptionOutput rayOutputWalls = RayPerceptionSensor.Perceive(rayInputWalls);

            foreach (var ray in rayOutputWalls.RayOutputs)
            {
                if (ray.HitTaggedObject)
                {
                    //Debug.Log("Tag of hit object: " + ray.HitGameObject.tag);

                    if (ray.HitGameObject.CompareTag("WallLeft"))
                    {
                        wallLeft = true;
                    }

                    if (ray.HitGameObject.CompareTag("WallRight"))
                    {
                        wallRight = true;
                    }
                    
                }
            }
        }
        else
        {
            Debug.LogError("RayPerceptionSensorComponent3D is not set up properly.");
        }
        
        sensor.AddObservation(wallLeft ? 4f : 0f);
        sensor.AddObservation(wallRight ? 5f : 0f);
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
        if(actions.DiscreteActions[0] == 2 && isGrounded && isReady)
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
            //Debug.Log("Sono nella rilevazione ostacolo");
            EndEpisode();
            SC_GroundGenerator.instance.gameOver = true;
        }
        
        if (collision.gameObject.CompareTag("Piattaforma") == true)
        {
            isReady = true;
            isGrounded = true;
            AddReward(0.3f);
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Piattaforma"))
        {
            isGrounded = false; // Non più a terra
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Piattaforma") == true)
        {
            isReady = true;
            isGrounded = true;
            AddReward(0.3f);
        }
    }


    void Jump()
    {
        if (isReady)
        {
            isReady = false;
            r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
        }
        AddReward(-0.005f);
    }
    
    
    float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    
    void MoveRight()
    {
        if (isReady)
        {
            r.AddForce(Vector3.right * force, ForceMode.Impulse);
        }
    }

    
    void MoveLeft()
    {
        if (isReady)
        {
            r.AddForce(Vector3.left * force, ForceMode.Impulse);

        }
    }
    
    
    void FixedUpdate()
    {
        // We apply gravity manually for more tuning control
        //r.AddForce(new Vector3(0, -gravity * r.mass, 0));
        
        //RequestDecision();
        
        //grounded = false;
    }
    
    

    
   
}