using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Platform Generation")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private float minPlatformDistance = 2f;
    [SerializeField] private float maxPlatformDistance = 4f;
    [SerializeField] private float minPlatformHeight = -2f;
    [SerializeField] private float maxPlatformHeight = 2f;
    [SerializeField] private int initialPlatformCount = 5;
    
    [Header("Obstacle Generation")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float obstacleChance = 0.5f; // Chance of spawning obstacle on a platform
    
    [Header("Collectible Generation")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private float collectibleChance = 0.3f; // Chance of spawning collectible
    
    // Private variables
    private Transform playerTransform;
    private float lastPlatformX;
    private float viewportThreshold = 0.8f; // Generate new platforms when player is at this percentage of viewport
    
    private void Start()
    {
        playerTransform = FindObjectOfType<PlayerController>().transform;
        
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Obstacle generator needs player reference.");
            return;
        }
        
        // Generate initial platforms
        Vector3 startPos = new Vector3(0, -2, 0);
        lastPlatformX = startPos.x;
        
        // Create a starting platform under the player
        GameObject startPlatform = Instantiate(platformPrefab, startPos, Quaternion.identity);
        startPlatform.transform.localScale = new Vector3(3, 0.5f, 1); // Make first platform wider
        
        // Generate initial set of platforms
        for (int i = 0; i < initialPlatformCount; i++)
        {
            GenerateNextPlatform();
        }
    }
    
    private void Update()
    {
        // Check if player is close to the right edge of the screen
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(playerTransform.position);
        
        if (viewportPos.x > viewportThreshold)
        {
            GenerateNextPlatform();
        }
    }
    
    private void GenerateNextPlatform()
    {
        // Calculate the next platform position
        float nextX = lastPlatformX + Random.Range(minPlatformDistance, maxPlatformDistance);
        float nextY = Random.Range(minPlatformHeight, maxPlatformHeight);
        
        // Create the platform
        Vector3 platformPos = new Vector3(nextX, nextY, 0);
        GameObject platform = Instantiate(platformPrefab, platformPos, Quaternion.identity);
        
        // Randomly scale platform
        float platformWidth = Random.Range(1f, 2.5f);
        platform.transform.localScale = new Vector3(platformWidth, 0.5f, 1);
        
        // Add platform to the "Ground" layer for collision detection
        platform.layer = LayerMask.NameToLayer("Ground");
        
        // Update the last platform position
        lastPlatformX = nextX;
        
        // Randomly add obstacles
        if (Random.value < obstacleChance && obstaclePrefabs.Length > 0)
        {
            AddObstacle(platform.transform);
        }
        
        // Randomly add collectibles
        if (Random.value < collectibleChance && collectiblePrefab != null)
        {
            AddCollectible(platform.transform);
        }
    }
    
    private void AddObstacle(Transform platformTransform)
    {
        // Select a random obstacle prefab
        int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstaclePrefab = obstaclePrefabs[obstacleIndex];
        
        // Calculate position (on top of platform)
        Vector3 platformSize = platformTransform.GetComponent<Renderer>().bounds.size;
        Vector3 obstaclePos = platformTransform.position + new Vector3(0, platformSize.y / 2 + 0.5f, 0);
        
        // Create obstacle
        GameObject obstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
        
        // Make obstacle a child of platform so it moves with it
        obstacle.transform.SetParent(platformTransform);
        
        // Tag obstacle for collision detection
        obstacle.tag = "Obstacle";
    }
    
    private void AddCollectible(Transform platformTransform)
    {
        if (collectiblePrefab == null)
            return;
            
        // Calculate position (above platform)
        Vector3 platformSize = platformTransform.GetComponent<Renderer>().bounds.size;
        Vector3 collectiblePos = platformTransform.position + new Vector3(0, platformSize.y / 2 + 1.5f, 0);
        
        // Create collectible
        GameObject collectible = Instantiate(collectiblePrefab, collectiblePos, Quaternion.identity);
        
        // Tag for scoring
        collectible.tag = "ScorePoint";
        
        // Add a trigger collider if not already present
        if (collectible.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = collectible.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }
    }
}