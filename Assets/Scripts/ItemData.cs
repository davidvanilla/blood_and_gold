using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "BattleRoyale/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int quantity;
    public ItemType type;
    public GameObject prefab; 
           
}
