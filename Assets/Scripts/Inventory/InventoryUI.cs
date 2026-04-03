using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Transform slotParent;  // 슬롯들이 배치될 부모 (Grid Layout Group이 있는 곳)
    public GameObject slotPrefab; // 슬롯 한 칸의 모양이 담긴 프리팹
    
    private List<SlotUI> slotUIs = new List<SlotUI>();

    private void OnEnable()
    {
        // 1. 인벤토리 매니저가 있는지 확인. (실행 순서 버그로 아직 Awake가 안 된 경우 직접 찾음)
        if (InventoryManager.Instance == null)
        {
            InventoryManager.Instance = FindObjectOfType<InventoryManager>();
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[UI 에러] 씬에 'InventoryManager'가 존재하지 않습니다! Hierarchy에 Manager 오브젝트를 만들고 스크립트를 붙여주세요.");
                return;
            }
        }

        // 2. 슬롯 UI가 아직 생성되지 않았거나 개수가 맞지 않으면 다시 생성
        int totalSize = InventoryManager.Instance.inventorySize + InventoryManager.Instance.hotbarSize;
        if (slotUIs.Count != totalSize)
        {
            slotUIs.Clear();
            foreach (Transform child in slotParent)
            {
                if (child.gameObject != slotPrefab) Destroy(child.gameObject);
            }

            if (slotPrefab != null)
            {
                // 원본 노출 방지
                if (slotPrefab.scene.name != null) slotPrefab.SetActive(false);

                for (int i = 0; i < totalSize; i++)
                {
                    GameObject newSlot = Instantiate(slotPrefab, slotParent, false); // UI 스케일 박살나는 현상 방지
                    newSlot.transform.localScale = Vector3.one; // 로컬 스케일 1 강제
                    newSlot.SetActive(true);
                    
                    SlotUI slotUI = newSlot.GetComponent<SlotUI>();
                    if (slotUI != null) 
                    {
                        slotUIs.Add(slotUI);
                    }
                    else
                    {
                        Debug.LogError("[UI 에러] 할당된 slotPrefab에 'SlotUI' 스크립트가 붙어있지 않습니다! 프리팹을 확인하세요.");
                    }
                }
            }
            else
            {
                Debug.LogError("[UI 에러] InventoryUI 스크립트에 'slotPrefab'이 할당되지 않았습니다! 언리얼/유니티 인스펙터 창을 확인하세요.");
            }
        }

        // 3. 이벤트 구독 및 초기 갱신
        InventoryManager.Instance.onInventoryChanged -= RefreshAllSlots; // 중복 방지
        InventoryManager.Instance.onInventoryChanged += RefreshAllSlots;
        RefreshAllSlots();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= RefreshAllSlots;
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

}
