using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameSpeedIncrease = 0.1f;
    [SerializeField] private float maxGameSpeed = 15f;
    [SerializeField] private float initialSpawnDelay = 2f;
    [SerializeField] private float minSpawnDelay = 0.8f;
    
    [Header("UI References")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Header("Prefabs and Spawning")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float destroyDistance = 10f;
    
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;
    
    // Private variables
    private int score = 0;
    private int highScore = 0;
    private float currentGameSpeed;
    private float currentSpawnDelay;
    private bool isGameActive = false;
    private Transform cameraTransform;
    private ArrayList spawnedObstacles = new ArrayList();
    
    private void Awake()
    {
        // Get high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Find player if not set in inspector
        if (player == null)
            player = FindObjectOfType<PlayerController>();
            
        cameraTransform = Camera.main.transform;
        
        // Subscribe to player events
        if (player != null)
        {
            player.onDeath.AddListener(GameOver);
            player.onScore.AddListener(AddScore);
        }
    }
    
    private void Start()
    {
        // Hide game over UI initially
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
            
        // Start the game
        StartGame();
    }
    
    private void Update()
    {
        if (!isGameActive)
            return;
            
        // Update UI
        if (scoreText != null)
            scoreText.text = score.ToString();
            
        // Clean up obstacles that are too far behind
        CleanupObstacles();
    }
    
    public void StartGame()
    {
        score = 0;
        currentGameSpeed = player.GetComponent<PlayerController>().MovementSpeed;
        currentSpawnDelay = initialSpawnDelay;
        isGameActive = true;
        
        // Show gameplay UI
        if (gameplayUI != null)
            gameplayUI.SetActive(true);
            
        // Hide game over UI
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
            
        // Start spawning obstacles
        StartCoroutine(SpawnObstacles());
    }
    
    private IEnumerator SpawnObstacles()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(currentSpawnDelay);
            
            if (obstaclePrefabs.Length > 0 && spawnPoint != null)
            {
                // Select random obstacle prefab
                int randomIndex = Random.Range(0, obstaclePrefabs.Length);
                
                // Spawn obstacle
                GameObject obstacle = Instantiate(obstaclePrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
                spawnedObstacles.Add(obstacle);
                
                // Decrease spawn delay, but not below minimum
                currentSpawnDelay = Mathf.Max(currentSpawnDelay - 0.01f, minSpawnDelay);
                
                // Increase game speed
                IncreaseGameSpeed();
            }
        }
    }
    
    private void CleanupObstacles()
    {
        // Create a temporary list to avoid modifying while iterating
        ArrayList obstaclesForRemoval = new ArrayList();
        
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle != null && obstacle.transform.position.x < cameraTransform.position.x - destroyDistance)
            {
                obstaclesForRemoval.Add(obstacle);
            }
        }
        
        // Remove and destroy marked obstacles
        foreach (GameObject obstacle in obstaclesForRemoval)
        {
            spawnedObstacles.Remove(obstacle);
            Destroy(obstacle);
        }
    }
    
    private void IncreaseGameSpeed()
    {
        // Increase game speed
        if (currentGameSpeed < maxGameSpeed)
        {
            currentGameSpeed += gameSpeedIncrease;
            
            // Update player movement speed
            if (player != null)
                player.GetComponent<PlayerController>().MovementSpeed = currentGameSpeed;
        }
    }
    
    public void AddScore()
    {
        score++;
        
        // Update high score if necessary
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }
    
    public void GameOver()
    {
        isGameActive = false;
        
        // Stop spawning
        StopAllCoroutines();
        
        // Update UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            
            if (finalScoreText != null)
                finalScoreText.text = "Score: " + score.ToString();
                
            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore.ToString();
        }
        
        // Hide gameplay UI
        if (gameplayUI != null)
            gameplayUI.SetActive(false);
    }
    
    public void RestartGame()
    {
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame()
    {
        // In editor, stop play mode; in build, quit application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}