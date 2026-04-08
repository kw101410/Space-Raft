using UnityEngine;

public enum ItemType
{
    Resource,   // 나무, 철, 얼음 등 재료
    Tool,       // 훅, 망치 등 도구
    Food        // 식량, 물
}

[CreateAssetMenu(fileName = "New Item", menuName = "SpaceRaft/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize = 64;
    public ItemType itemType; // 아이템 종류 추가
    public GameObject prefab; // 나중에 손에 들거나 건설할 때 쓸 모델
    
    [Header("Equip Transforms")]
    public Vector3 equipPosition = Vector3.zero;
    public Vector3 equipRotation = Vector3.zero;
    public Vector3 equipScale = Vector3.one;

    [TextArea]
    public string description;
}
