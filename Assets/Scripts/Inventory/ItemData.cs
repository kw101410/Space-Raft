using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "SpaceRaft/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize = 64;
    public GameObject prefab; // 나중에 손에 들거나 건설할 때 쓸 모델
    
    [TextArea]
    public string description;
}
