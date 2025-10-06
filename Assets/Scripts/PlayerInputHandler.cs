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

    private InputAction movementAction;
    private InputAction fire1Action;

    public Vector2 MovementInput { get; private set; }
    public bool Fire1Pressed { get; private set; }

    private void Awake()
    {
        InputActionMap mapRefrence = playerControls.FindActionMap(actionMapName);

        movementAction = mapRefrence.FindAction(movement);
        fire1Action = mapRefrence.FindAction(fire1);

        SubscribeActionValuesToInputEvents();
    }

    private void SubscribeActionValuesToInputEvents()
    {
        movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
        movementAction.canceled += inputInfo => MovementInput = Vector2.zero;

        fire1Action.performed += inputInfo => Fire1Pressed = true;
        fire1Action.canceled += inputInfo => Fire1Pressed = false;
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }




}
