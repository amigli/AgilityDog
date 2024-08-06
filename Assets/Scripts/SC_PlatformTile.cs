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
        int randomNumber = random.Next(0, 4);

        switch (randomNumber)
        {
            case 0: 
                // Genero l'ostacolo di salto
                obstacles[0].SetActive(true);
                ricompense[0].SetActive(true);
                ricompense[1].SetActive(true);
                ricompense[2].SetActive(true);
                break;
            case 1: 
                // Genero l'albero a destra
                obstacles[1].SetActive(true);
                ricompense[3].SetActive(true);
                penalty[0].SetActive(true);
                break;
            case 2:
                // Genero l'albero a sinistra
                obstacles[2].SetActive(true);
                ricompense[4].SetActive(true);
                penalty[1].SetActive(true);
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