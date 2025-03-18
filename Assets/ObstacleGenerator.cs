using UnityEngine;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Platform Generation")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private float platformWidth = 10f; // Ширина отдельной платформы
    [SerializeField] private float platformHeight = -2f; // Высота платформ
    [SerializeField] private int initialPlatformCount = 5; // Количество начальных платформ
    [SerializeField] private float minGapWidth = 2f; // Минимальная ширина разрыва
    [SerializeField] private float maxGapWidth = 4f; // Максимальная ширина разрыва
    
    [Header("Obstacle Generation")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float minObstacleDistance = 5f; // Минимальное расстояние между препятствиями
    [SerializeField] private float maxObstacleDistance = 8f; // Максимальное расстояние между препятствиями
    [SerializeField] private float obstacleHeightOffset = 1f; // Высота препятствий над платформой
    
    [Header("Collectible Generation")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private float collectibleChance = 0.3f; // Шанс создания коллекционного предмета
    
    // Для отслеживания созданных объектов
    private List<GameObject> activePlatforms = new List<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeCollectibles = new List<GameObject>();
    
    private Transform playerTransform;
    private float lastPlatformEndX = 0f;
    private float screenRightEdge = 0f;
    private float cleanupDistance = -20f; // Расстояние для удаления объектов за экраном
    
    private void Start()
    {
        playerTransform = FindObjectOfType<PlayerController>().transform;
        
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Obstacle generator needs player reference.");
            return;
        }
        
        // Создаем начальные платформы
        CreateInitialPlatforms();
    }
    
    private void Update()
    {
        if (playerTransform == null)
            return;
            
        // Обновляем положение правого края экрана
        Camera mainCamera = Camera.main;
        Vector3 screenRightPoint = new Vector3(Screen.width, Screen.height / 2, 0);
        Vector3 worldRightPoint = mainCamera.ScreenToWorldPoint(screenRightPoint);
        screenRightEdge = worldRightPoint.x;
        
        // Создаем новые платформы, если нужно
        if (lastPlatformEndX < screenRightEdge + 20f)
        {
            CreateNewPlatform();
        }
        
        // Удаляем объекты, которые игрок уже прошел
        CleanupObjects();
    }
    
    private void CreateInitialPlatforms()
    {
        // Создаем начальную платформу под игроком (более широкую)
        Vector3 startPosition = new Vector3(-5f, platformHeight, 0);
        GameObject startPlatform = CreatePlatform(startPosition, platformWidth + 5f);
        
        // Устанавливаем начальное значение lastPlatformEndX как правый край первой платформы
        lastPlatformEndX = startPosition.x + (platformWidth + 5f) / 2;
        
        // Создаем дополнительные начальные платформы без препятствий
        for (int i = 0; i < initialPlatformCount; i++)
        {
            CreateNewPlatform(false); // Без препятствий на начальных платформах
        }
    }
    
    private GameObject CreatePlatform(Vector3 position, float width)
    {
        GameObject platform = Instantiate(platformPrefab, position, Quaternion.identity);
        
        // Устанавливаем размер платформы
        platform.transform.localScale = new Vector3(width, 0.5f, 1);
        
        // Устанавливаем слой Ground
        platform.layer = LayerMask.NameToLayer("Ground");
        
        // Добавляем в список активных платформ
        activePlatforms.Add(platform);
        
        return platform;
    }
    
    private void CreateNewPlatform(bool addObstacles = true)
    {
        // Случайная ширина платформы
        float width = Random.Range(platformWidth * 0.8f, platformWidth * 1.2f);
        
        // Сначала добавляем разрыв к концу предыдущей платформы
        float gapSize = 2.5f; // Фиксированный разрыв для тестирования float gapSize = Random.Range(minGapWidth, maxGapWidth); !!!!!
        float startPositionAfterGap = lastPlatformEndX + gapSize;
        
        // Центр новой платформы должен быть на половину её ширины правее точки после разрыва
        Vector3 platformPosition = new Vector3(startPositionAfterGap + width/2, platformHeight, 0);
        
        Debug.Log("Последняя платформа заканчивается на x=" + lastPlatformEndX);
        Debug.Log("Добавлен разрыв " + gapSize + ". Позиция после разрыва = " + startPositionAfterGap);
        Debug.Log("Создаем платформу: ширина=" + width + 
                ", центр на x=" + platformPosition.x +
                ", левый край на x=" + (platformPosition.x - width/2) +
                ", правый край на x=" + (platformPosition.x + width/2));
        
        // Создаем новую платформу
        GameObject platform = CreatePlatform(platformPosition, width);
        
        // Обновляем позицию конца последней платформы (правый край)
        lastPlatformEndX = platformPosition.x + width / 2;
        
        Debug.Log("Новый lastPlatformEndX = " + lastPlatformEndX);
    }
    
    private void AddObstaclesToPlatform(GameObject platform)
    {
        if (obstaclePrefabs.Length == 0)
            return;
            
        // Получаем размеры платформы
        float platformWidth = platform.transform.localScale.x;
        Vector3 platformLeft = platform.transform.position - new Vector3(platformWidth / 2, 0, 0);
        
        // Добавляем препятствие на платформу (если она достаточно широкая)
        if (platformWidth >= 6f) 
        {
            // Выбираем случайное препятствие
            int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
            
            // Случайная позиция на платформе (не у краев)
            float obstacleX = platformLeft.x + Random.Range(platformWidth * 0.3f, platformWidth * 0.7f);
            
            // Создаем препятствие
            Vector3 obstaclePosition = new Vector3(
                obstacleX, 
                platform.transform.position.y + obstacleHeightOffset,
                0
            );
            
            GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIndex], obstaclePosition, Quaternion.identity);
            obstacle.tag = "Obstacle";
            activeObstacles.Add(obstacle);
            
            // Создаем коллекционный предмет рядом с препятствием с некоторым шансом
            if (Random.value < collectibleChance * 0.5f && collectiblePrefab != null)
            {
                AddCollectible(new Vector3(obstacleX + 1.5f, obstaclePosition.y + 1.5f, 0));
            }
        }
    }
    
    private void AddCollectiblesOverGap(float gapStartX, float gapEndX)
    {
        if (collectiblePrefab == null || Random.value > collectibleChance)
            return;
            
        // Расстояние разрыва
        float gapDistance = gapEndX - gapStartX;
        
        // Если разрыв достаточно большой, добавляем коллекционные предметы
        if (gapDistance >= 2f)
        {
            int collectibleCount = Mathf.FloorToInt(gapDistance / 1.5f);
            
            for (int i = 0; i < collectibleCount; i++)
            {
                float posX = gapStartX + (i + 1) * gapDistance / (collectibleCount + 1);
                float posY = platformHeight + Random.Range(2f, 3f); // Над разрывом
                
                AddCollectible(new Vector3(posX, posY, 0));
            }
        }
    }
    
    private void AddCollectible(Vector3 position)
    {
        GameObject collectible = Instantiate(collectiblePrefab, position, Quaternion.identity);
        collectible.tag = "ScorePoint";
        
        // Добавляем триггер-коллайдер, если его нет
        if (collectible.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = collectible.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }
        
        activeCollectibles.Add(collectible);
    }
    
    private void CleanupObjects()
    {
        float cleanupX = playerTransform.position.x + cleanupDistance;
        
        // Очистка платформ
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            if (activePlatforms[i] != null && activePlatforms[i].transform.position.x + 
                (activePlatforms[i].transform.localScale.x / 2) < cleanupX)
            {
                Destroy(activePlatforms[i]);
                activePlatforms.RemoveAt(i);
            }
        }
        
        // Очистка препятствий
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] != null && activeObstacles[i].transform.position.x < cleanupX)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
        
        // Очистка коллекционных предметов
        for (int i = activeCollectibles.Count - 1; i >= 0; i--)
        {
            if (activeCollectibles[i] != null && activeCollectibles[i].transform.position.x < cleanupX)
            {
                Destroy(activeCollectibles[i]);
                activeCollectibles.RemoveAt(i);
            }
        }
    }
}