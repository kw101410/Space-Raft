using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 15f;
    public float fieldOfView = 60f;
    public Transform cameraTransform;
    private Camera playerCamera;
    private float xRotation = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Input Actions (코드 내부에서 바로 정의하여 에디터 설정 없이도 작동하도록 구성)
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform != null)
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
        }

        // 이동 키 (W, A, S, D)
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // 마우스 회전
        lookAction = new InputAction("Look", binding: "<Gamepad>/rightStick");
        lookAction.AddBinding("<Mouse>/delta");

        // 점프 (Space)
        jumpAction = new InputAction("Jump", binding: "<Gamepad>/buttonSouth");
        jumpAction.AddBinding("<Keyboard>/space");

        // 달리기 (Left Shift)
        sprintAction = new InputAction("Sprint", binding: "<Gamepad>/leftTrigger");
        sprintAction.AddBinding("<Keyboard>/leftShift");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        
        // 마우스 커서 숨기기 및 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();

        // 마우스 커서 보이게 해제 (UI 창 등을 열 때 필요)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = fieldOfView;
        }
        
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("PlayerController에 Main Camera가 할당되지 않았습니다!");
            return;
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity * Time.deltaTime;

        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 카메라는 위/아래로 회전
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // 플레이어 몸체는 좌/우로 회전
        transform.Rotate(Vector3.up * lookInput.x);
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 땅에 붙어있도록 약간의 아래쪽 힘 유지
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // 달리기 적용
        float currentSpeed = walkSpeed;
        if (sprintAction.IsPressed())
        {
            currentSpeed *= sprintMultiplier;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // 점프
        if (jumpAction.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
