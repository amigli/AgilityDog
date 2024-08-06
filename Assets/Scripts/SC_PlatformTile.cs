using System;
using UnityEngine;

public class SC_PlatformTile : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public GameObject[] obstacles; //Objects that contains different obstacle types which will be randomly activated
    public GameObject[] ricompense;
    public GameObject[] penalty;
    
    public GameObject ricompensa;

    public static SC_PlatformTile instance;
    public Vector3 currentPosition;


    void Start()
    {
        instance = this;
    }

    public void ActivateRandomObstacle()
    {
        DeactivateAllObstacles();
        
        System.Random random = new System.Random();
        //Genero un numero casuale, a cui corrispondono gli ostacoli NON di salto
        int randomNumber = random.Next(3, 5);

        switch (randomNumber)
        {
            case 3: int r = random.Next(0, 4);
                if (r == 0 || r == 1)
                {
                    //è la rampa centrale
                    obstacles[0].SetActive(true);
                    ricompense[0].SetActive(true);
                    penalty[0].SetActive(true);
                    penalty[1].SetActive(true);
                    currentPosition = obstacles[0].transform.position;
                }

                if (r == 2)
                {
                    //è la rampa a destra
                    obstacles[2].SetActive(true);
                    ricompense[2].SetActive(true); 
                    penalty[2].SetActive(true);
                    currentPosition = obstacles[2].transform.position;
                }

                if (r == 3)
                {
                    //è la rampa a sinistra
                    obstacles[3].SetActive(true);
                    ricompense[3].SetActive(true); 
                    penalty[3].SetActive(true);
                    currentPosition = obstacles[3].transform.position;
                }
                break;
            case 4: int ra = random.Next(0, 4);
                if (ra == 0 || ra == 1)
                {
                    //è il tunnel centrale
                    obstacles[1].SetActive(true);
                    ricompense[1].SetActive(true); 
                    penalty[0].SetActive(true);
                    penalty[1].SetActive(true);
                    penalty[4].SetActive(true);
                    currentPosition = obstacles[1].transform.position;
                }

                if (ra == 2)
                {
                    //é il tunnel a destra
                    obstacles[4].SetActive(true);
                    ricompense[4].SetActive(true); 
                    penalty[2].SetActive(true);
                    penalty[5].SetActive(true);
                    currentPosition = obstacles[4].transform.position;
                }

                if (ra == 3)
                {
                    //è il tunnel a sinistra
                    obstacles[5].SetActive(true);
                    ricompense[5].SetActive(true); 
                    penalty[3].SetActive(true);
                    penalty[6].SetActive(true);
                    currentPosition = obstacles[5].transform.position;
                }
                
                
                break;
        }

    }

    public void DeactivateAllObstacles()
    {
        for (int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].SetActive(false);
            ricompense[i].SetActive(false);
        }
        
        ricompensa.SetActive(false);
    }

    
}