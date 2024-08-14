using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgilityDog : Agent
{
    
    public float gravity = 30.0f;
    public float jumpHeight = 1f;
    float moveSpeed = 10f;

    private bool isReady = false;
    private bool isGrounded = false; 

    //Raycasy che percepiscono ostacoli
    public RayPerceptionSensorComponent3D raycast;
    
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
        isGrounded = false;
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
        if (actions.DiscreteActions[0] == 2 && isGrounded)
            Jump();
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = -10000;

        if (Input.GetKey(KeyCode.RightArrow) == true)
            discreteActions[0] = 0;
        
        if (Input.GetKey(KeyCode.LeftArrow) == true)
            discreteActions[0] = 1;

        if (Input.GetKey(KeyCode.Space) == true && isGrounded)
            discreteActions[0] = 2;
    }
    
    private void OnCollisionStay(Collision collision)
    {
        
        //Premio per i prosciutti
        if (collision.gameObject.CompareTag("Ricompensa") == true)
        {
            AddReward(0.5f);
            SC_GroundGenerator.instance.score += 2;
            collision.gameObject.SetActive(false);
        }
        
        //Penalità se va contro i muri laterali
        if (collision.gameObject.CompareTag("Wall") == true)
        {
            AddReward(-0.3f);
            SC_GroundGenerator.instance.score += -1;
        }

        //Penalità se va contro un ostacolo
        if (collision.gameObject.CompareTag("Finish") == true ||
            collision.gameObject.CompareTag("AlberoLeft") == true ||
            collision.gameObject.CompareTag("AlberoRight") == true)
        {
            AddReward(-0.8f);
            EndEpisode();
            SC_GroundGenerator.instance.gameOver = true;
        }
        
        //Incoraggiamento a restare a terra
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
    
    void Jump()
    {
        if (isGrounded)
        {
            r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
        }
        else
        {
            AddReward(-0.4f); // Penalità se tenta di saltare quando non già è in aria
        }
    }
    
    float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
    
    void MoveRight()
    {
        if (isReady)
        {
            Vector3 currentPosition = transform.position;
        
            Vector3 targetPosition = new Vector3(1f, currentPosition.y, currentPosition.z);
        
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    void MoveLeft()
    {
        if (isReady)
        {
            Vector3 currentPosition = transform.position;
        
            Vector3 targetPosition = new Vector3(-1f, currentPosition.y, currentPosition.z);
        
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    private void FixedUpdate()
    {
        if (isAlberoLeft() || isAlberoRight() || isObstacleAhead())
        {
            RequestDecision();
        }
    }

    //Metodo che controlla se avanti all'agente c'è un ostacolo
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

    //Metodo che controlla se in prossimità dell'agente c'è un albero a sinistra
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

    //Metodo che controlla se in prossimità dell'agente c'è un albero a destra
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
}