using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float offsetBelowCamera = 5f; // Расстояние ниже нижней границы камеры
    [SerializeField] private float width = 100f; // Ширина коллайдера смерти
    
    private BoxCollider2D deathCollider;
    private Camera mainCamera;
    
    private void Awake()
    {
        // Создаем коллайдер или получаем существующий
        deathCollider = GetComponent<BoxCollider2D>();
        if (deathCollider == null)
        {
            deathCollider = gameObject.AddComponent<BoxCollider2D>();
            deathCollider.isTrigger = true; // Делаем его триггером
        }
        
        // Находим игрока, если не задан в инспекторе
        if (playerTransform == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                playerTransform = player.transform;
        }
        
        mainCamera = Camera.main;
        
        // Установим тег для определения коллизии
        gameObject.tag = "DeathZone";
    }
    
    private void Start()
    {
        // Настраиваем начальный размер и позицию коллайдера
        UpdateColliderSizeAndPosition();
    }
    
    private void Update()
    {
        // Обновляем позицию коллайдера, чтобы он всегда следовал за камерой
        UpdateColliderSizeAndPosition();
    }
    
    private void UpdateColliderSizeAndPosition()
    {
        if (mainCamera == null || deathCollider == null)
            return;
            
        // Получаем нижнюю границу экрана в мировых координатах
        Vector3 bottomScreenPoint = new Vector3(Screen.width / 2, 0, 0);
        Vector3 bottomWorldPoint = mainCamera.ScreenToWorldPoint(bottomScreenPoint);
        
        // Устанавливаем позицию и размеры коллайдера
        transform.position = new Vector3(
            mainCamera.transform.position.x, 
            bottomWorldPoint.y - offsetBelowCamera, 
            0
        );
        
        // Настраиваем размеры коллайдера
        deathCollider.size = new Vector2(width, 1f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что в зону смерти попал игрок
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}