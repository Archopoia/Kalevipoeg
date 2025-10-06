using System.Runtime.CompilerServices;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.0f;

    [Header("Refrences")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private bool isFacingRight;
    float horizontalInput;

    public Animator anim;

    private Vector3 currentMovement;
    private float CurrentSpeed => walkSpeed;

    // Walking sound variables
    private float lastWalkingSoundTime;
    private float walkingSoundInterval = 0.5f;
    void Start()
    {
        isFacingRight = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        HandleMovement();

        if (currentMovement.magnitude >= 0.1f)
        {
            anim.SetBool("isRunning", true);

            // Play walking sound at intervals
            if (Time.time - lastWalkingSoundTime >= walkingSoundInterval)
            {
                PlayWalkingSound();
                lastWalkingSoundTime = Time.time;
            }
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        if (!isFacingRight && currentMovement.x > 0)
        {
            FlipSprite();
        }
        else if (isFacingRight && currentMovement.x < 0)
        {
            FlipSprite();

        }
    }

    void FlipSprite()
    {
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f , playerInputHandler.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }
    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        currentMovement = Vector3.zero;

        currentMovement.x = worldDirection.x * CurrentSpeed;
        currentMovement.z = worldDirection.z * CurrentSpeed;

        characterController.Move(currentMovement * Time.deltaTime);
    }

    /// <summary>
    /// Play walking sound through AudioManager
    /// </summary>
    private void PlayWalkingSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerWalkingSound();
        }
    }
}
