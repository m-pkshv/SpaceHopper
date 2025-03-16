using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(4f, 1f, -10f);
    [SerializeField] private float lookAheadAmount = 3f; // How far ahead the camera should look
    
    private Vector3 desiredPosition;
    private Vector3 smoothedPosition;
    private float defaultZPosition;
    
    private void Start()
    {
        // Find player if not set in inspector
        if (target == null)
            target = FindObjectOfType<PlayerController>().transform;
            
        if (target == null)
            Debug.LogError("Target not set for camera follow script!");
            
        defaultZPosition = transform.position.z;
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
            
        // Calculate the desired position with lookahead
        desiredPosition = target.position + offset;
        desiredPosition.x += lookAheadAmount; // Look ahead in the direction of movement
        desiredPosition.z = defaultZPosition; // Keep the original z position
        
        // Smoothly move the camera towards the desired position
        smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}