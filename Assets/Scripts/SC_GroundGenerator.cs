using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_GroundGenerator : MonoBehaviour
{
    public Camera mainCamera;
    public Transform startPoint; //Point from where ground tiles will start
    public SC_PlatformTile tilePrefab;
    public float movingSpeed = 8;
    public int tilesToPreSpawn = 1; //How many tiles should be pre-spawned
    public int tilesWithoutObstacles = 0; //How many tiles at the beginning should not have obstacles, good for warm-up
    
    List<SC_PlatformTile> spawnedTiles = new List<SC_PlatformTile>();
    int nextTileToActivate = -1;
    [HideInInspector]
    public bool gameOver = false;
    static bool gameStarted = false;
    public float score = 0;
    
    private int rewardCount = 0; // Contatore delle ricompense raccolte

    //private List<Vector3> posizioni;
    
    public static SC_GroundGenerator instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //posizioni = new List<Vector3>();

        Vector3 spawnPosition = startPoint.position;
        int tilesWithNoObstaclesTmp = tilesWithoutObstacles;
        for (int i = 0; i < tilesToPreSpawn; i++)
        {
            spawnPosition -= tilePrefab.startPoint.localPosition;
            SC_PlatformTile spawnedTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity) as SC_PlatformTile;
            if(tilesWithNoObstaclesTmp > 0)
            {
                spawnedTile.DeactivateAllObstacles();
                
                tilesWithNoObstaclesTmp--;
            }
            else
            {
                spawnedTile.ActivateRandomObstacle();
            }
            
            spawnPosition = spawnedTile.endPoint.position;
            spawnedTile.transform.SetParent(transform);
            spawnedTiles.Add(spawnedTile);
        }
    }
    
    // Metodo per incrementare la velocità
    public void IncreaseSpeed()
    {
        rewardCount++;
        if (rewardCount % 10 == 0) // Aumenta la velocità ogni 10 ricompense
        {
            movingSpeed += 3f;  // Incrementa la velocità di movimento
            Debug.Log("Velocità attuale: " + movingSpeed);
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        // Move the object upward in world space x unit/second.
        //Increase speed the higher score we get
        if (!gameOver && gameStarted)
        {
            transform.Translate(-spawnedTiles[0].transform.forward * Time.deltaTime * (movingSpeed + (score/500)), Space.World);
            
        }

        if (mainCamera.WorldToViewportPoint(spawnedTiles[0].endPoint.position).z < 0)
        {
            //Move the tile to the front if it's behind the Camera
            SC_PlatformTile tileTmp = spawnedTiles[0];
            spawnedTiles.RemoveAt(0);
            tileTmp.transform.position = spawnedTiles[spawnedTiles.Count - 1].endPoint.position - tileTmp.startPoint.localPosition;
            tileTmp.ActivateRandomObstacle();
            spawnedTiles.Add(tileTmp);
        }

        if (gameOver || !gameStarted)
        {
            //Torna alla posizione iniziale
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            gameStarted = true;
        }
    }

    void OnGUI()
    {
        if (gameOver)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200), "Game Over");
        }
        else
        {
            if (!gameStarted)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200), "Game Start");
            }
        }


        //GUI.color = Color.black;
        //GUI.Label(new Rect(5, 5, 200, 25), "Score: " + ((int)score));
    }
}