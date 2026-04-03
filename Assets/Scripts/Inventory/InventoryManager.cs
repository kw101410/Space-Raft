using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public InventorySlot()
    {
        item = null;
        count = 0;
    }

    public void AddItem(ItemData newItem, int amount)
    {
        item = newItem;
        count += amount;
    }

    public bool IsEmpty => item == null;
}

[DefaultExecutionOrder(-50)] // UI 스크립트보다 무조건 먼저 Awake 되도록 설정
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    public int inventorySize = 27; // 마인크래프트 기본 보관함 크기
    public int hotbarSize = 9;    // 핫바 크기
    
    public List<InventorySlot> slots = new List<InventorySlot>();

    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 인스펙터에서 리스트 크기가 잘못 잡혀있을 수 있으므로 정확히 맞춥니다.
        int totalSize = inventorySize + hotbarSize;
        if (slots.Count < totalSize)
        {
            for (int i = slots.Count; i < totalSize; i++)
            {
                slots.Add(new InventorySlot());
            }
        }
        else if (slots.Count > totalSize)
        {
            slots.RemoveRange(totalSize, slots.Count - totalSize);
        }
    }

    // 아이템 획득 시 호출되는 함수
    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        
        // 안전 장치: maxStackSize가 0이면 1로 처리하여 무한 루프 방지
        int stackSize = Mathf.Max(1, item.maxStackSize);

        // 1. 이미 인벤토리에 같은 아이템이 있고 더 들어갈 자리가 있는지 확인 (Stacking)
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.count < stackSize)
            {
                int addAmount = Mathf.Min(amount, stackSize - slot.count);
                slot.count += addAmount;
                amount -= addAmount;
                if (amount <= 0)
                {
                    Debug.Log($"[인벤토리] {item.itemName} 중첩 완료. (현재 개수: {slot.count})");
                    onInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // 2. 남은 수량이 있다면 빈 슬롯 찾기
        while (amount > 0)
        {
            InventorySlot emptySlot = slots.Find(s => s.IsEmpty);
            if (emptySlot != null)
            {
                int addAmount = Mathf.Min(amount, stackSize);
                emptySlot.AddItem(item, addAmount);
                amount -= addAmount;
                Debug.Log($"[인벤토리] 새 슬롯에 {item.itemName} {addAmount}개 추가.");
            }
            else
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다!");
                onInventoryChanged?.Invoke();
                return false;
            }
        }

        onInventoryChanged?.Invoke();
        return true;
    }
}
