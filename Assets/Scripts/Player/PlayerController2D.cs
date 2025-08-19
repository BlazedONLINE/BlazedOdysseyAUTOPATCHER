using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeedMultiplier = 1.5f;
    
    [Header("Animation")]
    public Animator animator; // Assign in Inspector
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateAnimation();
    }
    
    void HandleInput()
    {
        // Input using new Input System
        if (Keyboard.current != null)
        {
            // WASD and Arrow Keys
            Vector2 moveInput = Vector2.zero;
            
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
            
            movement = moveInput;
            
            // Shift for running
            isRunning = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

            // Normalize movement vector to prevent faster diagonal movement
            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }
        }
    }
    
    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude); // Use sqrMagnitude for performance
            animator.SetBool("IsRunning", isRunning);
        }
    }

    void FixedUpdate()
    {
        // Movement
        float currentSpeed = moveSpeed * (isRunning ? runSpeedMultiplier : 1f);
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
    
    // Called when player spawns
    public void Initialize()
    {
        Debug.Log($"🎮 Player {gameObject.name} initialized!");
    }
}
