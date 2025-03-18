using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float coyoteTime = 0.1f; // Время "койота" - можно прыгать чуть-чуть после схода с платформы
    [SerializeField] private float jumpBufferTime = 0.1f; // Буфер прыжка - можно нажать прыжок чуть раньше приземления
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onJump;
    public UnityEvent onScore;
    
    // Публичное свойство для скорости движения
    public float MovementSpeed
    {
        get { return movementSpeed; }
        set { movementSpeed = value; }
    }
    
    // Приватные переменные
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private bool isGrounded;
    private bool isDead;
    private float coyoteTimeCounter; // Счетчик времени "койота"
    private float jumpBufferCounter; // Счетчик буфера прыжка
    private Vector3 groundCheckPosition;
    private bool jumpInput;

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
            
        // Обновляем позицию проверки земли
        groundCheckPosition = transform.position - new Vector3(0, 0.5f, 0);
            
        // Проверяем, стоит ли игрок на земле
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheckPosition, groundCheckRadius, groundLayers);
        
        // Обновляем счетчик времени "койота"
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        // Обновляем анимацию, если есть аниматор
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }
        
        // Явно проверяем все возможные виды ввода
        CheckInput();
        
        // Выполняем прыжок, если есть ввод и игрок на земле или в пределах времени "койота"
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
        
        // Ограничиваем скорость падения
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
        
        // Перемещаем игрока вперед автоматически
        rb.velocity = new Vector2(movementSpeed, rb.velocity.y);
    }

    private void CheckInput()
    {
        // ВАЖНО: Проверяем много разных вариантов ввода, чтобы что-то точно сработало
        
        // Способ 1: Стандартная проверка нажатия клавиши
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log("Space key detected");
            return;
        }
        
        // Способ 2: Проверка тачей
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log("Touch detected");
            return;
        }
        
        // Способ 3: Использование Button input (для мобильных устройств)
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1"))
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log("Jump/Fire1 button detected");
            return;
        }
        
        // Способ 4: Проверка кликов мыши (можно использовать в редакторе)
        if (Input.GetMouseButtonDown(0))
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log("Mouse click detected");
            return;
        }
        
        // Декрементируем счетчик буфера прыжка
        jumpBufferCounter -= Time.deltaTime;
    }
    
    private void Jump()
    {
        Debug.Log("Player jumped!");
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        
        // Воспроизводим звук прыжка
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
            
            // Уничтожаем коллекционный предмет
            Destroy(collider.gameObject);
        }
    }
    
    public void Die()
    {
        isDead = true;
        
        // Воспроизводим анимацию смерти, если есть аниматор
        if (animator != null)
            animator.SetTrigger("Die");
            
        // Воспроизводим звук смерти
        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);
            
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        
        // Отключаем коллайдер
        GetComponent<Collider2D>().enabled = false;
        
        onDeath?.Invoke();
    }
    
    // Для отладки - визуализация проверки земли
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - new Vector3(0, 0.5f, 0), groundCheckRadius);
    }
}