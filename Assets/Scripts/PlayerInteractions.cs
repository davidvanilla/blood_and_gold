using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    private InputSystem_Actions _playerControlls;
    private Player player;
    

    public void Awake()
    {
        _playerControlls = new InputSystem_Actions();
        
    }

    public void Start()
    {
        player = GetComponent<Player>();
        _playerControlls.Player.Enable();
    }

    public void OnEnable()
    {
        _playerControlls.Enable();
        _playerControlls.Player.Action.performed += Interact_performed;
    }

    public void OnDisable()
    {
       
        _playerControlls.Player.Action.performed -= Interact_performed;
        _playerControlls.Disable();
    }


    private void Interact_performed(InputAction.CallbackContext obj)
    {
        Debug.Log($"Interact performed. Near item {player.GetNearItem().ToString()}");
        //if (player.GetNearItem() == ItemType.Health)
        //{
        //    player.AddExtraHealth(10);
        //    GameEvents.TriggerHealthChanged();
        //    GameEvents.TriggerDestroyItem(player.GetHealthPrefab());
        //    player.PlayPickup();
            
        //}

        switch (player.GetNearItem())
        {
            case ItemType.Health:
                player.AddExtraHealth(10);
                GameEvents.TriggerHealthChanged();
                GameEvents.TriggerDestroyItem(player.GetItemPrefab());
                player.PlayPickup();
                break;
            case ItemType.Riffle:
                GameObject rifleToPickup = player.GetItemPrefab();
                RiffleType type = player.GetRiffleType();
                EquipRifle(rifleToPickup, type);
                player.PlayPickup();
                break;

            case ItemType.Ammo:
                if(!player.GetHasRiffle())
                {
                    Debug.Log("No gun equipped, cannot pick up ammo");
                    return;
                }
                if(player.GetCurrentGun().GetCarriedAmo() == player.GetCurrentGun().GetMaxCarriedAmo())
                {
                    Debug.Log("Player has enough amo");
                    return;
                }
                player.AddCarriedAmo(30);
                GameEvents.TriggerAmoCountChanged();
                GameEvents.TriggerDestroyItem(player.GetItemPrefab());
                player.PlayPickup();
                break;
        }
    }


    private void EquipRifle(GameObject newRiffle, RiffleType riffleType)
    {
        Debug.Log("Spawning new gun");

        // --- Drop the old gun on the floor ---
        GameObject oldGun = player.GetCurrentGunObject();
        if (oldGun != null)
        {
            // Detach from player's hand
            oldGun.transform.SetParent(null);

            // Add Rigidbody if not present
            Rigidbody rb = oldGun.GetComponent<Rigidbody>();
            if (rb == null)
                rb = oldGun.AddComponent<Rigidbody>();

            // Add Collider if missing (so it doesn't fall through floor)
            Collider col = oldGun.GetComponent<Collider>();
            if (col == null)
                col = oldGun.AddComponent<BoxCollider>();

            // Re-enable ItemScript so it becomes pickable again
            ItemScript oldItemScript = oldGun.GetComponent<ItemScript>();
            if (oldItemScript != null)
                oldItemScript.enabled = true;

            // reenable non trigger collider for gravity
            // remove the non trigger collider for new gun
            Transform oldGunColChild = oldGun.transform.Find("Col");
            if (oldGunColChild != null)
            {
                Collider physicsCollider = oldGunColChild.GetComponent<Collider>();
                if (physicsCollider != null) physicsCollider.enabled = true;
            }

            // Optional: give a small random velocity for a "drop" effect
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;  // helps with fast objects
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Spawn the new gun 
        //GameObject newGunObj = Instantiate(player.GetItemPrefab(), player.GetPlayerRightHand().transform);

        newRiffle.transform.SetParent(player.GetPlayerRightHand().transform);
        var newGunObj = newRiffle;
        // Apply position/rotation offsets based on rifle type
        if (riffleType == RiffleType.Ar15)
        {
            //newGunObj.transform.localPosition = new Vector3(-0.1f, 0.214f, 0.054f);
            //newGunObj.transform.localRotation = Quaternion.Euler(177.881f, 22.9259f, 80.247f);
            newGunObj.transform.localPosition = new Vector3(-0.074f, 0.219f, 0.05f);
            newGunObj.transform.localRotation = Quaternion.Euler(-1.652f, -169.048f, -101.102f);
        }
        else if (riffleType == RiffleType.ARLP)
        {
            //newGunObj.transform.localPosition = new Vector3(-0.09900006f, 0.183f, 0.05399996f);
            //newGunObj.transform.localRotation = Quaternion.Euler(93.496f, -38.13202f, 30.90199f);

            newGunObj.transform.localPosition = new Vector3(-0.08642835f, 0.1261911f, 0.01415792f);
            newGunObj.transform.localRotation = Quaternion.Euler(101.045f, -54.15698f, 10.56f);
        }


        // remove rigid body for new gun
        Rigidbody newGunRb = newGunObj.GetComponent<Rigidbody>();
        if (newGunRb != null)
        {

            Destroy(newGunRb);
        }
        // remove the non trigger collider for new gun
        Transform colChild = newGunObj.transform.Find("Col");
        if (colChild != null)
        {
            Collider physicsCollider = colChild.GetComponent<Collider>();
            if (physicsCollider != null) physicsCollider.enabled = false;
        }

        // Disable ItemScript on the new gun (so it doesn't detect itself while held)
        ItemScript newItemScript = newGunObj.GetComponent<ItemScript>();
        if (newItemScript != null) newItemScript.enabled = false;

        // Update player reference
        Gun newGunComponent = newGunObj.GetComponent<Gun>();
        player.SetCurrentGun(newGunComponent);

        //set to riffle animation mode
        player.SetRiffleStateAnimation();

        // on exit item event
        GameEvents.TriggerOnExitedItem();
    }

    private void SpawnRiffle(RiffleType riffleType)
    {
        Debug.Log("Spawning new gun");

        // --- Drop the old gun on the floor ---
        GameObject oldGun = player.GetCurrentGunObject();
        if (oldGun != null)
        {
            // Detach from player's hand
            oldGun.transform.SetParent(null);

            // Add Rigidbody if not present
            Rigidbody rb = oldGun.GetComponent<Rigidbody>();
            if (rb == null)
                rb = oldGun.AddComponent<Rigidbody>();

            // Add Collider if missing (so it doesn't fall through floor)
            Collider col = oldGun.GetComponent<Collider>();
            if (col == null)
                col = oldGun.AddComponent<BoxCollider>();
          
            // Re-enable ItemScript so it becomes pickable again
            ItemScript oldItemScript = oldGun.GetComponent<ItemScript>();
            if (oldItemScript != null)
                oldItemScript.enabled = true;

            // reenable non trigger collider for gravity
            // remove the non trigger collider for new gun
            Transform oldGunColChild = oldGun.transform.Find("Col");
            if (oldGunColChild != null)
            {
                Collider physicsCollider = oldGunColChild.GetComponent<Collider>();
                if (physicsCollider != null) physicsCollider.enabled = true;
            }

            // Optional: give a small random velocity for a "drop" effect
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;  // helps with fast objects
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Spawn the new gun 
        GameObject newGunObj = Instantiate(player.GetItemPrefab(), player.GetPlayerRightHand().transform);

        // Apply position/rotation offsets based on rifle type
        if (riffleType == RiffleType.Ar15)
        {
            newGunObj.transform.localPosition = new Vector3(-0.1f, 0.214f, 0.054f);
            newGunObj.transform.localRotation = Quaternion.Euler(177.881f, 22.9259f, 80.247f);
        }
        else if (riffleType == RiffleType.ARLP)
        {
            newGunObj.transform.localPosition = new Vector3(-0.09900006f, 0.183f, 0.05399996f);
            newGunObj.transform.localRotation = Quaternion.Euler(93.496f, -38.13202f, 30.90199f);
        }


        // remove rigid body for new gun
        Rigidbody newGunRb = newGunObj.GetComponent<Rigidbody>();
        if (newGunRb != null)
        {
            
            Destroy(newGunRb);
        }
        // remove the non trigger collider for new gun
        Transform colChild = newGunObj.transform.Find("Col");
        if (colChild != null)
        {
            Collider physicsCollider = colChild.GetComponent<Collider>();
            if (physicsCollider != null) physicsCollider.enabled = false;
        }

        // Disable ItemScript on the new gun (so it doesn't detect itself while held)
        ItemScript newItemScript = newGunObj.GetComponent<ItemScript>();
        if (newItemScript != null) newItemScript.enabled = false;

        // Update player reference
        Gun newGunComponent = newGunObj.GetComponent<Gun>();
        player.SetCurrentGun(newGunComponent);

        //set to riffle animation mode
        player.SetRiffleStateAnimation();
    }

    private void SpawnGun(RiffleType riffleType)
    {
        Debug.Log("Spawning new gun");
        var gunTransform = player.GetCurrentGun().transform;
        //Vector3(3.75051975, 192.625763, 258.790619)
        //Vector3(-0.105999999, 0.250999987, 0.0260000005)
         


            //178.24 12.628 78.791
        //GameObject newGunObj =Instantiate(player.GetItemPrefab(), gunTransform.position, gunTransform.rotation,);
        Destroy(player.GetCurrentGunObject());

        GameObject newGunObj = Instantiate(player.GetItemPrefab(), player.GetPlayerRightHand().transform);
        if(riffleType == RiffleType.Ar15)
        {
            newGunObj.transform.localPosition = new Vector3(-0.1f, 0.214f, 0.054f);
            newGunObj.transform.localRotation = Quaternion.Euler(177.881f, 22.9259f, 80.247f);
        }

        if(riffleType == RiffleType.ARLP)
        {
            newGunObj.transform.localPosition = new Vector3(-0.09900006f, 0.183f, 0.05399996f);
            newGunObj.transform.localRotation = Quaternion.Euler(93.496f, -38.13202f, 30.90199f);
        }



        Gun newGunComponent = newGunObj.GetComponent<Gun>();
        ItemScript itemScript = newGunObj.GetComponent<ItemScript>();
        itemScript.enabled = false;
        player.SetCurrentGun(newGunComponent);

    }
}
