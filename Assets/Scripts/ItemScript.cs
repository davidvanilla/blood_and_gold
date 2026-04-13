using System;
using UnityEngine;

public enum ItemType
{
    None,
    Health,
    Ammo,
    Riffle
}
public enum RiffleType
{
    None,
    ARLP,
    Ar15
}
public class ItemScript : MonoBehaviour
{
    public event Action EnteredItem;

   

    [SerializeField] private ItemType itemType;
    [SerializeField] private RiffleType riffleType;

    private void OnTriggerEnter(Collider other)
    {
        GameObject enteredObject = other.gameObject;

        if (enteredObject.CompareTag("Player"))
        {
            Debug.Log("An item entered: " + enteredObject.name);
            // Handle item logic
            GameEvents.TriggerOnEneteredItem();
            var player = enteredObject.GetComponent<Player>();
            if (player != null)
            {
                switch(itemType)
                {
                    case ItemType.Health:
                        player.SetNearItem(ItemType.Health);
                        player.SetItemPrefab(this.gameObject);
                        break;
                    case ItemType.Ammo:
                        player.SetNearItem(ItemType.Ammo);
                        player.SetItemPrefab(this.gameObject);
                        break;
                    case ItemType.Riffle:
                        player.SetRiffleType(this.riffleType);
                        player.SetNearItem(ItemType.Riffle);
                        player.SetItemPrefab(this.gameObject);
                        // Handle gun logic
                        break;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject enteredObject = other.gameObject;
        if (enteredObject.CompareTag("Player"))
        {
            Debug.Log("An item entered: " + enteredObject.name);
            // Handle item logic
            GameEvents.TriggerOnExitedItem();
            // get player and trigger new health
            var player = enteredObject.GetComponent<Player>();
            if (player != null)
            {
                
                player.SetNearItem(ItemType.None);
                player.SetItemPrefab(null);
                player.SetRiffleType(RiffleType.None);
            }
        }
    }
}
