using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse Look (up/down only on camera, left/right on body)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // Rotate the player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down (only if camera is child of player)
        if (Camera.main.transform.IsChildOf(transform))
        {
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        // WASD Movement
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        Vector3 movement = moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    // Ground check
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
