using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GamePlayUIData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amoCount;

    [SerializeField] private Player player;
    [SerializeField] private GameObject health;
    private Slider healthSlider;
    private Image healthFill;
    private TextMeshProUGUI healthCount;
    private TextMeshProUGUI healthCountExtra;
    [SerializeField]private TextMeshProUGUI pickupText;

    private Gun _currentGun;

    // Update is called once per frame
    void Update()
    {
        
    }


    private void Start()
    {
        UpdateAmoCount();
        pickupText.enabled = false;
        healthSlider = health.GetComponent<Slider>();
        var fillObject = health.transform.Find("Fill");
        healthFill = fillObject.GetComponent<Image>();
        var countObject = health.transform.Find("Count");
        healthCount = countObject.GetComponent<TextMeshProUGUI>();
        var countExtraObject = health.transform.Find("Extra");
        healthCountExtra = countExtraObject.GetComponent<TextMeshProUGUI>();

    }
    private void OnEnable()
    {
        _currentGun = player.GetCurrentGun(); 
        if (_currentGun != null)
            _currentGun.OnUpdateAmoCount += UpdateAmoCount;

        player.UpdateHealth += SetHealth;

        GameEvents.OnEnteredItem += OnEnteredItem;
        GameEvents.OnExitedItem += OnExitedPickupItem;
        GameEvents.OnHealthChanged += SetHealth;

        GameEvents.DestroyItem += OnGameItemDestroy;

        GameEvents.OnAmoCountChanged += UpdateAmoCount;

    }
    private void OnDisable()
    {
        if (_currentGun != null)
            _currentGun.OnUpdateAmoCount -= UpdateAmoCount;

        player.UpdateHealth -= SetHealth;

        GameEvents.OnEnteredItem -= OnEnteredItem;
        GameEvents.OnExitedItem -= OnExitedPickupItem;

        GameEvents.OnHealthChanged -= SetHealth;

        GameEvents.DestroyItem -= OnGameItemDestroy;

        GameEvents.OnAmoCountChanged -= UpdateAmoCount;
    }


    private void OnGameItemDestroy(GameObject obj)
    {
        player.SetNearItem(ItemType.None);
        player.SetItemPrefab(null);
        pickupText.enabled = false;
        player.SetRiffleType(RiffleType.None);
        if (obj != null) Destroy(obj);
    }

    public void OnExitedPickupItem()
    {
        player.SetNearItem(ItemType.None);
        player.SetItemPrefab(null);
        pickupText.enabled = false;
        player.SetRiffleType(RiffleType.None);
    }

    public void OnEnteredItem()
    {
        pickupText.enabled = true;
    }



    public void UpdateAmoCount()
    {
        _currentGun = player.GetCurrentGun();
        if (_currentGun != null)
            amoCount.text = $"{_currentGun.GetCurrentAmo()} / {_currentGun.GetCarriedAmo()}";
    }

    public void SetHealth()
    {
        healthSlider.value = player.GetHealth();
        healthCount.text = $"{player.GetHealth()}";
        healthCountExtra.text = $"{player.GetExtraHealth()}";
    }

}
