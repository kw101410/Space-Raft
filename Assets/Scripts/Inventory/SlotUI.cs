using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    public void SetSlot(InventorySlot slot)
    {
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
}
