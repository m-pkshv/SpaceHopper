using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(2f, 1f, -10f); // Уменьшенное смещение по X
    
    private Vector3 velocity = Vector3.zero;
    
    private void Start()
    {
        // Найти игрока, если не задан в инспекторе
        if (target == null)
            target = FindObjectOfType<PlayerController>().transform;
            
        if (target == null)
            Debug.LogError("Target not set for camera follow script!");
        
        // Установить начальное положение камеры с учетом смещения
        transform.position = new Vector3(
            target.position.x + offset.x, 
            target.position.y + offset.y, 
            offset.z
        );
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Целевая позиция - игрок плюс фиксированное смещение
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x, 
            target.position.y + offset.y, 
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