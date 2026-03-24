using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Transform slotParent;  // 슬롯들이 배치될 부모 (Grid Layout Group이 있는 곳)
    public GameObject slotPrefab; // 슬롯 한 칸의 모양이 담긴 프리팹
    
    private List<SlotUI> slotUIs = new List<SlotUI>();

    private void Start()
    {
        // 1. 기존에 남아있을지 모를 자식들 청소 (깔끔하게 시작!)
        foreach (Transform child in slotParent) {
            Destroy(child.gameObject);
        }

        // 2. 인벤토리 매니저의 슬롯 개수만큼 UI 슬롯 생성
        if (InventoryManager.Instance != null)
        { // inventorySize(27) + hotbarSize(9) 총 36개 생성
            int totalSize = InventoryManager.Instance.inventorySize + InventoryManager.Instance.hotbarSize;
            for (int i = 0; i < totalSize; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotParent);
                SlotUI slotUI = newSlot.GetComponent<SlotUI>();
                if (slotUI != null) slotUIs.Add(slotUI);
            }

            // 3. 인벤토리 변경 이벤트 구독
            InventoryManager.Instance.onInventoryChanged += RefreshAllSlots;
            RefreshAllSlots();
        }
    }

    public void RefreshAllSlots()
    {
        var inventorySlots = InventoryManager.Instance.slots;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (i < inventorySlots.Count)
            {
                slotUIs[i].SetSlot(inventorySlots[i]);
            }
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= RefreshAllSlots;
        }
    }
}
