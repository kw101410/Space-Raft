using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel; // 'I'를 누르면 켜고 끌 UI 패널
    public bool isInventoryOpen = false;

    private void Start()
    {
        // 시작할 때 인벤토리는 무조건 닫음
        isInventoryOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        // 처음엔 커서 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 새로운 입력 시스템(Input System) 전용 코드: 'I' 키 체크
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // 인벤토리가 열리면 마우스 커서 해제
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 인벤토리가 닫히면 마우스 커서 다시 고정
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
