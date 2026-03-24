using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel; // 'I'를 누르면 켜고 끌 UI 패널
    public bool isInventoryOpen = false;

    private InputAction toggleAction;

    private void Awake()
    {
        // 'I' 키 설정
        toggleAction = new InputAction("ToggleInventory", binding: "<Keyboard>/i");
    }

    private void OnEnable()
    {
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        toggleAction.Disable();
    }

    private void Update()
    {
        if (toggleAction.triggered)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // 인벤토리가 열리면 마우스 커서 해제
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // 시간 멈춤 (선택 사항)
            // Time.timeScale = 0f; 
        }
        else
        {
            // 인벤토리가 닫히면 마우스 커서 다시 고정
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Time.timeScale = 1f;
        }
    }
}
