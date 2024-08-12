// Classe originale utilizzata per l'addestramento

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgilityDog : Agent
{
    
    public float gravity = 20.0f;
    public float jumpHeight = 1f;
    public float force = 0.5f;
    float moveSpeed = 10f;

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
    
    
    


    private bool isObstacleAhead()
    {
        if (raycast != null)
        {
            RayPerceptionInput rayInput = raycast.GetRayPerceptionInput();
            RayPerceptionOutput rayOutput = RayPerceptionSensor.Perceive(rayInput);

            foreach (var ray in rayOutput.RayOutputs)
            {
                if (ray.HitTaggedObject && ray.HitGameObject.CompareTag("Finish"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool isAlberoLeft()
    {
        if (raycast != null)
        {
            RayPerceptionInput rayInput = raycast.GetRayPerceptionInput();
            RayPerceptionOutput rayOutput = RayPerceptionSensor.Perceive(rayInput);

            foreach (var ray in rayOutput.RayOutputs)
            {
                if (ray.HitTaggedObject && ray.HitGameObject.CompareTag("AlberoLeft"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool isAlberoRight()
    {
        if (raycast != null)
        {
            RayPerceptionInput rayInput = raycast.GetRayPerceptionInput();
            RayPerceptionOutput rayOutput = RayPerceptionSensor.Perceive(rayInput);

            foreach (var ray in rayOutput.RayOutputs)
            {
                if (ray.HitTaggedObject && ray.HitGameObject.CompareTag("AlberoRight"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Vai a destra
        if (actions.DiscreteActions[0] == 0)
        {
            
            MoveRight();
            /*
            if (isAlberoLeft())
            {
                MoveRight();
            }
            else
            {
                //Penalità
                AddReward(-0.1f);
            }
            */
            
        }
            
        
        //Vai a sinistra
        if (actions.DiscreteActions[0] == 1)
        {
            
            MoveLeft();
            /*
            if (isAlberoRight())
            {
                MoveLeft();
            }
            else
            {
                //Penalità
                AddReward(-0.1f);
            }
            */
        }
            

        //Salta
        if (actions.DiscreteActions[0] == 2)
        {
            
            Jump();
            /*
            if (isObstacleAhead())
            {
                Jump();
            }
            else
            {
                //Penalizzo un salto non necessario
                AddReward(-0.1f);
            }
            */
        }
            
        
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
        AddReward(-0.1f);
    }
    
    
    float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    
    void MoveRight()
    {
        if (isReady)
        {
            //r.AddForce(Vector3.right * force, ForceMode.Impulse);
            
            // Get the current position
            Vector3 currentPosition = transform.position;
        
            // Set the new position with X = -0.49 and keep Y and Z the same
            Vector3 targetPosition = new Vector3(1f, currentPosition.y, currentPosition.z);
        
            // Move the agent smoothly towards the target position
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    
    void MoveLeft()
    {
        if (isReady)
        {
            //r.AddForce(Vector3.left * force, ForceMode.Impulse);
            
            // Get the current position
            Vector3 currentPosition = transform.position;
        
            // Define the target position with X = -1f, keeping Y and Z the same
            Vector3 targetPosition = new Vector3(-1f, currentPosition.y, currentPosition.z);
        
            // Move the agent smoothly towards the target position
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);

        }
    }
    
    
    

   
}