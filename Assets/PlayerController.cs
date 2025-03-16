using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float movementSpeed = 5f;
    public float MovementSpeed
{
    get { return movementSpeed; }
    set { movementSpeed = value; }
}
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private LayerMask groundLayers;
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onJump;
    public UnityEvent onScore;
    
    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private bool isGrounded;
    private bool isDead;
    private float groundCheckRadius = 0.2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (rb == null)
            Debug.LogError("Rigidbody2D component missing on player!");
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void Update()
    {
        if (isDead)
            return;
            
        // Check if player is on the ground
        isGrounded = Physics2D.OverlapCircle(transform.position - new Vector3(0, 0.5f, 0), groundCheckRadius, groundLayers);
        
        // Set animation parameters if animator exists
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }
        
        // Handle jump input
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            Jump();
        }
        
        // Limit fall speed
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
        
        // Move player forward automatically
        rb.velocity = new Vector2(movementSpeed, rb.velocity.y);
    }
    
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        
        // Play jump sound
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
            
        onJump?.Invoke();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isDead)
        {
            Die();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("ScorePoint"))
        {
            onScore?.Invoke();
        }
    }
    
    public void Die()
    {
        isDead = true;
        
        // Play death animation if animator exists
        if (animator != null)
            animator.SetTrigger("Die");
            
        // Play death sound
        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);
            
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        
        // Disable collider
        GetComponent<Collider2D>().enabled = false;
        
        onDeath?.Invoke();
    }
}