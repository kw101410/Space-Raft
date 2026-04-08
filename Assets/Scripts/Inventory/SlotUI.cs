using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    
    [HideInInspector] public int slotIndex = -1;
    
    // 드래그 아이콘 처리를 위한 정적 변수들
    private static GameObject dragIconObject;
    private static RectTransform dragIconRect;
    private static Image dragIconImage;
    private Canvas canvas;

    private Canvas GetRootCanvas()
    {
        if (canvas != null) return canvas;
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null) canvas = canvas.rootCanvas;
        return canvas;
    }

    public void SetSlot(InventorySlot slot, int index)
    {
        slotIndex = index;
        if (slot == null || slot.item == null || slot.count <= 0)
        {
            ClearSlot();
            return;
        }

        // 디버깅: UI가 데이터를 받았는지 확인
        // Debug.Log($"[UI 슬롯] '{slot.item.itemName}' 아이템을 슬롯에 그립니다. (개수: {slot.count})");

        if (iconImage != null)
        {
            iconImage.sprite = slot.item.icon;
            iconImage.enabled = true;
            iconImage.gameObject.SetActive(true);
            
            // 만약 아이콘이 없으면 반투명한 빨간색으로 표시해서 존재 확인
            if (slot.item.icon == null)
            {
                iconImage.color = new Color(1, 0, 0, 0.5f); 
                Debug.LogWarning($"[UI 주의] '{slot.item.itemName}'의 아이콘이 없습니다!");
            }
            else
            {
                iconImage.color = Color.white; 
            }
        }
        else
        {
            Debug.LogError("[UI 에러] SlotUI에 Icon Image가 할당되지 않았습니다!");
        }
        
        if (countText != null)
        {
            countText.text = slot.count.ToString(); // 1개일 때도 무조건 표시하여 확인
            countText.enabled = true;
            countText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("[UI 에러] SlotUI에 Count Text가 할당되지 않았습니다!");
        }
    }

    public void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // 투명하게
            iconImage.enabled = false;
            iconImage.gameObject.SetActive(false);
        }
        if (countText != null)
        {
            countText.text = "";
            countText.enabled = false;
            countText.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) // 드래그 시작
    {
        if (iconImage.sprite == null || !iconImage.gameObject.activeSelf) return;

        Canvas rootCanvas = GetRootCanvas();
        if (rootCanvas == null) return;

        // 임시 드래그 아이콘 생성
        if (dragIconObject == null)
        {
            dragIconObject = new GameObject("DragIcon");
            dragIconObject.transform.SetParent(rootCanvas.transform, false);
            dragIconObject.transform.SetAsLastSibling(); // 맨 앞으로 (UI 렌더링 최상단)
            
            dragIconImage = dragIconObject.AddComponent<Image>();
            dragIconImage.raycastTarget = false; // 드랍 처리를 위해 이건 클릭 안받게
            
            dragIconRect = dragIconObject.GetComponent<RectTransform>();
            dragIconRect.sizeDelta = iconImage.rectTransform.sizeDelta; // 아이콘 크기 복사
        }

        dragIconObject.SetActive(true);
        dragIconObject.transform.SetAsLastSibling();
        dragIconImage.sprite = iconImage.sprite;
        dragIconImage.color = iconImage.color;
        
        // 투명도 조절로 드래그 중임을 표시
        iconImage.color = new Color(1, 1, 1, 0.5f);
        
        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData) // 드래그 중
    {
        if (dragIconObject != null && dragIconObject.activeSelf)
        {
            UpdateDragPosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData) // 드래그 종료
    {
        if (dragIconObject != null)
        {
            dragIconObject.SetActive(false);
        }

        // 아이콘 원래대로
        if (iconImage.sprite != null)
        {
            iconImage.color = Color.white;
        }
    }

    public void OnDrop(PointerEventData eventData) // 아이템을 이 슬롯에 놓았을 때
    {
        if (eventData.pointerDrag != null)
        {
            SlotUI sourceSlot = eventData.pointerDrag.GetComponentInParent<SlotUI>();
            if (sourceSlot != null && sourceSlot != this && sourceSlot.slotIndex >= 0 && this.slotIndex >= 0)
            {
                // 인벤토리 매니저를 통해 두 슬롯 교환
                InventoryManager.Instance.SwapSlots(sourceSlot.slotIndex, this.slotIndex);
            }
        }
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetRootCanvas().transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 localPointerPosition);
            
        dragIconRect.localPosition = localPointerPosition;
    }
}
