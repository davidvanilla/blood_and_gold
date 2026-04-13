
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    public string itemsFolderPath = "Items";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnRandomItemAt(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void SpawnRandomItemAt(Vector3 position)
    {
        ItemData[] allItems = Resources.LoadAll<ItemData>(itemsFolderPath);
        if (allItems.Length == 0)
        {
            Debug.LogWarning("No ItemData found in Resources/" + itemsFolderPath);
            return;
        }
        ItemData selected = allItems[Random.Range(0, allItems.Length)];
        GameObject pickupObject = null;
        if (selected.prefab != null)
        {
            pickupObject = Instantiate(selected.prefab, position, Quaternion.identity);
        }
    }
}
