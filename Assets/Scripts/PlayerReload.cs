using UnityEngine;

public class PlayerReload : MonoBehaviour
{

    [SerializeField] private Player player;
    private Gun _currentGun;
    void Start()
    {
        _currentGun = player.GetCurrentGun();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnReloadSound()
    {
        _currentGun = player.GetCurrentGun();
        if (_currentGun != null)
            _currentGun.PlayReloadSound();
    }
}
