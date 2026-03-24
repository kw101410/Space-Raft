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
        else Destroy(gameObject);

        // 빈 슬롯들로 초기화
        for (int i = 0; i < inventorySize + hotbarSize; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    // 아이템 획득 시 호출되는 함수
    public bool AddItem(ItemData item, int amount = 1)
    {
        // 1. 이미 인벤토리에 같은 아이템이 있고 더 들어갈 자리가 있는지 확인 (Stacking)
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.count < item.maxStackSize)
            {
                int addAmount = Mathf.Min(amount, item.maxStackSize - slot.count);
                slot.count += addAmount;
                amount -= addAmount;
                if (amount <= 0)
                {
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
                int addAmount = Mathf.Min(amount, item.maxStackSize);
                emptySlot.AddItem(item, addAmount);
                amount -= addAmount;
            }
            else
            {
                // 인벤토리 가득 참!
                Debug.LogWarning("인벤토리가 가득 찼습니다!");
                onInventoryChanged?.Invoke();
                return false;
            }
        }

        onInventoryChanged?.Invoke();
        return true;
    }
}
