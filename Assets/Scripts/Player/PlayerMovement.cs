using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpForce = 6f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Animation Tuning")]
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField] private float walkAnimBaseSpeed = 5f;
    [SerializeField] private float runAnimBaseSpeed = 9f;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip runFootstepClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField][Range(0f, 1f)] private float footstepVolume = 0.5f;

    private AudioSource audioSource;

    private Rigidbody rb;
    private Animator animator;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isRunning;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        transform.Rotate(Vector3.up * mouseX);

        if (Camera.main.transform.IsChildOf(transform))
        {
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            animator.SetTrigger("Jump");
            if (jumpClip != null) audioSource.PlayOneShot(jumpClip, 0.8f);
        }

        // Run
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Landing detection — wasGrounded lets us catch the exact frame we touch down
        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("Land");
            if (landClip != null) audioSource.PlayOneShot(landClip, 0.9f);
        }
        wasGrounded = isGrounded;

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = transform.right * horizontal + transform.forward * vertical;
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 movement = moveDirection.normalized * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    void UpdateAnimator()
    {
        // 0 = idle, 0.5 = walking, 1 = running — keeps them in separate ranges
        float speed = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).magnitude;

        float animSpeed_normalized = 0f;
        if (speed > 0.1f)
            animSpeed_normalized = isRunning ? 1f : 0.5f;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Scale animation playback speed to match actual movement speed
        float animSpeed = 1f;
        if (speed > 0.1f)
        {
            float baseSpeed = isRunning ? runAnimBaseSpeed : walkAnimBaseSpeed;
            float actualSpeed = isRunning ? runSpeed : moveSpeed;
            animSpeed = actualSpeed / baseSpeed;
        }

        animator.SetFloat("Speed", animSpeed_normalized, animationDampTime, Time.deltaTime);
        animator.SetFloat("Horizontal", horizontal, animationDampTime, Time.deltaTime);
        animator.SetFloat("Vertical", vertical, animationDampTime, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning && speed > 0.1f);
        animator.SetBool("IsFalling", !isGrounded && rb.linearVelocity.y < -0.1f);
        animator.SetFloat("AnimSpeed", animSpeed);
    }

    public void PlayFootstep()
    {
        if (footstepClip == null) return;
        if (isRunning) return; // walk event fired but we're running, skip it
        audioSource.PlayOneShot(footstepClip, footstepVolume);
    }

    public void PlayRunFootstep()
    {
        if (runFootstepClip == null) return;
        if (!isRunning) return; // run event fired but we're walking, skip it
        audioSource.PlayOneShot(runFootstepClip, footstepVolume);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}