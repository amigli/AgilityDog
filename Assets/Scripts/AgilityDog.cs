using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Object = System.Object;

public class AgilityDog : Agent
{
    
    public float gravity = 20.0f;
    public float jumpHeight = 1.5f;
    public float force = 1.0f;

    private bool isReady;

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
        /*
        //Aggiungo le informazioni sulla posizione del cane (3 float perché è un vettore)
        sensor.AddObservation(transform.position.x);
        //Debug.Log(transform.position.x);
        
        sensor.AddObservation(transform.position.y);
        //Debug.Log(transform.position.y);
                                                            
        sensor.AddObservation(transform.position.z);
        //Debug.Log(transform.position.z);
      
        if (SC_PlatformTile.instance != null)
        {
            sensor.AddObservation(SC_PlatformTile.instance.currentPosition.x);
            //Debug.Log(SC_PlatformTile.instance.currentPosition.x);

            sensor.AddObservation(SC_PlatformTile.instance.currentPosition.y);
            //Debug.Log(SC_PlatformTile.instance.currentPosition.y);

            sensor.AddObservation(SC_PlatformTile.instance.currentPosition.z);
            //Debug.Log(SC_PlatformTile.instance.currentPosition.z);
        }
        //Debug.Log(sensor.GetObservationSpec().DimensionProperties);

        if (raycast.CompareTag("Ostacolo4") == true);
            Debug.Log("Ostacolo4");
            */
    } 

    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 0)
        {
            Debug.Log("Sto Andando a destra!!!");
            MoveRight();
        }

        if (actions.DiscreteActions[0] == 1)
        {
            Debug.Log("Sto andando a sinistra!!");
            MoveLeft();
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
            
    }

    
    private void OnCollisionStay(Collision collision)
    {
        
        //Premio per la carne
        if (collision.gameObject.CompareTag("Ricompensa") == true)
        {
            //Debug.Log("Collisione con ricompensa");
            AddReward(0.5f);
            SC_GroundGenerator.instance.score += 2;
            //Destroy(collision.gameObject);
            collision.gameObject.SetActive(false);
        }
        

        if (collision.gameObject.CompareTag("WrongRight") || collision.gameObject.CompareTag("WrongLeft")
            || collision.gameObject.CompareTag("WrongUp"))
        {
            AddReward(-0.5f);
            collision.gameObject.SetActive(false);
            SC_GroundGenerator.instance.score += -1;
            EndEpisode();
        }
        
        if (collision.gameObject.CompareTag("Piattaforma") == true)
        {
            isReady = true;
        }

        if (collision.gameObject.CompareTag("Wall") == true)
        {
            AddReward(-0.2f);
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

        //isJumping = true;
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
