using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    public void SetSlot(InventorySlot slot)
    {
        if (slot.item == null || slot.count <= 0)
        {
            ClearSlot();
            return;
        }

        iconImage.sprite = slot.item.icon;
        iconImage.enabled = true;
        
        // 1개 이상일 때만 숫자 표시 (마인크래프트 스타일)
        countText.text = slot.count > 1 ? slot.count.ToString() : "";
        countText.enabled = true;
    }

    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        countText.text = "";
        countText.enabled = false;
    }
}
