using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    public float rotationSpeed = 100f;
    
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}