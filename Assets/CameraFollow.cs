using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(2f, 1f, -10f);
    [SerializeField] private float fixedYPosition = 0f; // Фиксированная Y позиция камеры
    
    private Vector3 velocity = Vector3.zero;
    private float initialY; // Исходная Y позиция камеры
    
    private void Start()
    {
        // Найти игрока, если не задан в инспекторе
        if (target == null)
            target = FindObjectOfType<PlayerController>().transform;
            
        if (target == null)
            Debug.LogError("Target not set for camera follow script!");
        
        // Сохраняем начальную Y позицию из инспектора или используем текущую
        initialY = fixedYPosition != 0f ? fixedYPosition : transform.position.y;
        
        // Установить начальное положение камеры с учетом смещения
        transform.position = new Vector3(
            target.position.x + offset.x, 
            initialY, // Используем фиксированную Y позицию
            offset.z
        );
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Целевая позиция - следуем за игроком только по X, используем фиксированную Y
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x, 
            initialY, // Используем фиксированную Y позицию
            offset.z
        );
        
        // Плавное следование за игроком
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            smoothSpeed
        );
    }
}