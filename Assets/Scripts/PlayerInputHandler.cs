using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Refrences")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action Map Name Refrences")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string fire1 = "Fire1";
    [SerializeField] private string pauseMenu = "PauseMenu";

    private InputAction movementAction;
    private InputAction fire1Action;
    private InputAction pauseMenuAction;

    public Vector2 MovementInput { get; private set; }
    public bool Fire1Pressed { get; private set; }
    public bool PauseMenuPressed { get; private set; }

    private void Awake()
    {
        InputActionMap mapRefrence = playerControls.FindActionMap(actionMapName);

        movementAction = mapRefrence.FindAction(movement);
        fire1Action = mapRefrence.FindAction(fire1);
        pauseMenuAction = mapRefrence.FindAction(pauseMenu);

        SubscribeActionValuesToInputEvents();
    }

    private void SubscribeActionValuesToInputEvents()
    {
        movementAction.performed += inputInfo =>
        {
            MovementInput = inputInfo.ReadValue<Vector2>();
            Debug.Log($"Movement input received: {MovementInput}");
        };
        movementAction.canceled += inputInfo =>
        {
            MovementInput = Vector2.zero;
            Debug.Log("Movement input canceled");
        };

        fire1Action.performed += inputInfo =>
        {
            Fire1Pressed = true;
            Debug.Log("Fire1 pressed!");
        };
        fire1Action.canceled += inputInfo =>
        {
            Fire1Pressed = false;
            Debug.Log("Fire1 released!");
        };

        pauseMenuAction.performed += inputInfo =>
        {
            PauseMenuPressed = true;
            Debug.Log("PauseMenu pressed!");
        };
        pauseMenuAction.canceled += inputInfo =>
        {
            PauseMenuPressed = false;
            Debug.Log("PauseMenu released!");
        };
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }

    // Disable player actions (movement, fire) but keep pause menu active
    public void DisablePlayerActions()
    {
        if (movementAction != null)
        {
            movementAction.Disable();
            MovementInput = Vector2.zero; // Reset movement input
        }

        if (fire1Action != null)
        {
            fire1Action.Disable();
            Fire1Pressed = false; // Reset fire1 input
        }

        Debug.Log("PlayerInputHandler: Player actions (movement, fire1) disabled");
    }

    // Re-enable player actions when unpausing
    public void EnablePlayerActions()
    {
        if (movementAction != null)
        {
            movementAction.Enable();
        }

        if (fire1Action != null)
        {
            fire1Action.Enable();
        }

        Debug.Log("PlayerInputHandler: Player actions (movement, fire1) enabled");
    }
}
