using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float runMultiplier = 1.5f;

    [Header("Squash and Stretch Settings")]
    public float maxStretch = 0.3f;     // How much the ball deforms at max speed
    public float scaleLerpSpeed = 15f;  // How smoothly it transitions between shapes

    [Header("Growth Settings (E Key)")]
    public float growMultiplier = 1.8f; // How much bigger the ball gets when E is held
    public float growLerpSpeed = 12f;   // How smoothly it grows and shrinks

    [Header("Color Palette (Keys 1, 2, 3, 4)")]
    public Color color1 = Color.white;  // Press '1'
    public Color color2 = Color.red;    // Press '2'
    public Color color3 = Color.green;  // Press '3'
    public Color color4 = Color.blue;   // Press '4'
    public float colorLerpSpeed = 10f;  // Smooth color transition speed

    [Header("Dash Settings (L Key)")]
    public float dashForce = 25f;        // Speed burst during dash
    public float dashDuration = 0.2f;    // How long the dash burst lasts (in seconds)
    public float dashCooldown = 1f;      // Cooldown in seconds between dashes
    
    private float nextDashTime = 0f;
    private float dashTimer = 0f;
    private Vector2 dashDirection;
    private Vector2 lastMoveDir = Vector2.right; // Fallback direction when standing still

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private float currentGrowth = 1f;
    private Color targetColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetColor = color1;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(x, y).normalized;

        // Save last movement direction so you can dash while standing still
        if (movement.sqrMagnitude > 0.01f)
        {
            lastMoveDir = movement;
        }

        // --- DASH LOGIC ---
        if (Input.GetKeyDown(KeyCode.L) && Time.time >= nextDashTime)
        {
            dashDirection = (movement.sqrMagnitude > 0.01f) ? movement : lastMoveDir;
            dashTimer = dashDuration;
            nextDashTime = Time.time + dashCooldown;
        }

        // Apply movement velocity (Handling active dash vs normal movement)
        if (dashTimer > 0)
        {
            rb.linearVelocity = dashDirection * dashForce;
            dashTimer -= Time.deltaTime;
        }
        else
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float currentSpeed = isRunning ? (speed * runMultiplier) : speed;
            rb.linearVelocity = movement * currentSpeed;
        }

        // Growth (E key)
        float targetGrowth = Input.GetKey(KeyCode.E) ? growMultiplier : 1f;
        currentGrowth = Mathf.Lerp(currentGrowth, targetGrowth, Time.deltaTime * growLerpSpeed);

        // Color Selection (Keys 1, 2, 3, 4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) targetColor = color1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) targetColor = color2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) targetColor = color3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) targetColor = color4;

        // Invisibility (P key) and Color Lerping
        if (spriteRenderer != null)
        {
            bool isInvisible = Input.GetKey(KeyCode.P);
            spriteRenderer.enabled = !isInvisible;

            if (!isInvisible)
            {
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * colorLerpSpeed);
            }
        }

        // Apply Squash, Stretch, and Scale
        HandleSquashAndStretch(movement);
    }

    void HandleSquashAndStretch(Vector2 movement)
    {
        Vector3 currentBaseScale = originalScale * currentGrowth;

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float maxPossibleSpeed = Mathf.Max(speed * runMultiplier, dashForce);
            float speedRatio = Mathf.Clamp01(rb.linearVelocity.magnitude / maxPossibleSpeed);
            float stretch = speedRatio * maxStretch;

            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Vector3 targetScale = new Vector3(
                currentBaseScale.x * (1f + stretch),
                currentBaseScale.y * (1f - stretch),
                1f
            );

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleLerpSpeed);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, currentBaseScale, Time.deltaTime * scaleLerpSpeed);
        }
    }
}