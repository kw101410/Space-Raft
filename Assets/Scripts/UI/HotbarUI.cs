using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HotbarUI : MonoBehaviour
{
    public Transform hotbarParent; // 핫바 슬롯들이 들어갈 부모
    public GameObject slotPrefab;  // 슬롯 프리팹 (InventoryUI와 동일한 것 사용)
    
    // (선택 사항) 현재 선택된 슬롯을 강조해 주기 위한 테두리 투명도 값
    private Color normalColor = new Color(1, 1, 1, 0.3f); 
    private Color selectedColor = new Color(1, 1, 1, 1.0f); 

    private List<SlotUI> hotbarSlots = new List<SlotUI>();
    private List<Image> bgImages = new List<Image>();

    private PlayerEquip equipSys;

    private void Start()
    {
        if (InventoryManager.Instance == null) return;

        // PlayerEquip 스크립트 연결 캐싱
        equipSys = FindObjectOfType<PlayerEquip>();

        // 핫바 UI 생성 (0~8번 = 9개)
        int hotbarSize = InventoryManager.Instance.hotbarSize;
        
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, hotbarParent, false);
            newSlot.transform.localScale = Vector3.one;
            newSlot.SetActive(true);
            
            SlotUI slotUI = newSlot.GetComponent<SlotUI>();
            if (slotUI != null)
            {
                hotbarSlots.Add(slotUI);
                bgImages.Add(newSlot.GetComponent<Image>()); // 슬롯 배경 이미지 (테두리 하이라이트용)
            }
        }

        // 인벤토리 상태가 변할 때 이벤트 구독
        InventoryManager.Instance.onInventoryChanged += RefreshHotbar;
        RefreshHotbar();
    }

    private void Update()
    {
        // 시각적으로 하이라이트 갱신 (선택된 1~9번 칸 색깔 바꾸기)
        if (equipSys == null) equipSys = FindObjectOfType<PlayerEquip>();
        
        if (equipSys != null && bgImages.Count == InventoryManager.Instance.hotbarSize)
        {
            for (int i = 0; i < bgImages.Count; i++)
            {
                if (bgImages[i] != null)
                {
                    // 선택된 칸이면 완전 불투명 하얀색, 안 선택된 칸은 약간 투명하게
                    bgImages[i].color = (equipSys.currentHotbarIndex == i) ? selectedColor : normalColor;
                }
            }
        }
    }

    public void RefreshHotbar()
    {
        if (InventoryManager.Instance == null) return;
        var slots = InventoryManager.Instance.slots;

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (i < slots.Count)
            {
                hotbarSlots[i].SetSlot(slots[i]);
            }
        }
    }
}
