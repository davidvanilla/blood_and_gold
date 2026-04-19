using System.Collections;
using Unity.Cinemachine;

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Gun : MonoBehaviour
{
    private Vector3 _aimPoint;
    private bool _isFiring;
    private CinemachineCamera _virtualCamera;
    

    [Header("Audio")]

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _shotSound;
    [SerializeField] private AudioClip _reloadSound;
    [SerializeField] private AudioClip _emptySound;

    private Vector3 _targetOffset = Vector3.zero;
    [Header("Recoil")]
    [SerializeField] private float _recoilAmount = 2f;        // How much the camera rotates up per shot
    [SerializeField] private float _recoilRecoverySpeed = 5f; // How fast it returns
    [SerializeField] private float _maxRecoilX = 10f;         // Clamp to avoid spinning
    private float _currentRecoilX;
    private CinemachineCameraOffset _cameraOffset;
   
    [SerializeField] private float _aimDistance = 100f;
    [SerializeField] private LayerMask _aimLayers;

    [SerializeField] private Transform _gunMuzzle;
    //[SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 50f;
    [SerializeField] private float _fireRate = 0.2f;

    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private Light _muzzleLight;

    private float _nextFireTime;
    private bool _isAiming;

    public event System.Action OnUpdateAmoCount;
  
    private int currentAmmo = 30;
    private int maxAmmo = 30;
    private int carriedAmmo = 90;
    private int maxCarriedAmmo = 120;
    private int headLayer;
    private int bodyLayer;

    private Coroutine _reloadCoroutine;
    private bool _isReloading = false;
    [SerializeField]private int _reloadTime = 3; // seconds

    private void Start()
    {
        _muzzleLight.enabled = false;
        currentAmmo = maxAmmo;
        carriedAmmo = maxCarriedAmmo;

        headLayer = LayerMask.NameToLayer("enemy_head");
        bodyLayer = LayerMask.NameToLayer("enemy_body");
    }
    private void Awake()
    {
        _virtualCamera = FindFirstObjectByType<CinemachineCamera>();

        if (_virtualCamera != null)
            Debug.Log("Found Vcamera");
            _cameraOffset = _virtualCamera.GetComponent<CinemachineCameraOffset>();
    }
    void Update()
    {
        if (_isFiring && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + _fireRate;
        }

        if (_cameraOffset != null)
        {
            // Smoothly move the camera's current offset toward the target offset
            _cameraOffset.Offset = Vector3.Lerp(
                _cameraOffset.Offset,
                _targetOffset,
                _recoilRecoverySpeed * Time.deltaTime
            );

            // Gradually reduce the target offset back to zero (recovery)
            _targetOffset = Vector3.Lerp(
                _targetOffset,
                Vector3.zero,
                _recoilRecoverySpeed * 0.5f * Time.deltaTime   // adjust multiplier for recovery speed
            );
        }


        if (_isAiming)
        {
            Ray ray = new Ray(_virtualCamera.transform.position, _virtualCamera.transform.forward);

            int playerLayer = LayerMask.NameToLayer("Player");
            int playerGunLayer = LayerMask.NameToLayer("PlayerGun");
            int aimLayerMask = ~((1 << playerLayer) | (1 << playerGunLayer));

            if (Physics.Raycast(ray, out RaycastHit hit, _aimDistance, aimLayerMask))
            {
                _aimPoint = hit.point;
            }
            else
            {
                _aimPoint = ray.origin + ray.direction * _aimDistance;
            }

            Debug.DrawLine(_virtualCamera.transform.position, _aimPoint, Color.red);
        }
    }

    public void UpdateFire(bool data)
    {
        _isFiring = data;
    }
    public void UpdateAiming(bool data)
    {
        _isAiming = data;
    }

    public void Fire()
    {
        Debug.Log("Fire method called");
        if (!_isAiming) return;


        if (Time.time < _nextFireTime) return;

        _nextFireTime = Time.time + _fireRate;
        _isFiring = true;

        if (_isFiring && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + _fireRate;
        }
        
    }


    void Shoot()
    {
        //Debug.Log("Shooting!");
        if (!_isAiming)
        {
            StopFire();
            return;
            
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            //_emptyGunAudio.Play();
            //_audioSource.Stop();
            _muzzleLight.enabled = false;
            _muzzleFlash.Stop();
            _audioSource.Stop();
            _audioSource.PlayOneShot(_emptySound);
            return;
        }

       

        _currentRecoilX += _recoilAmount;
        _currentRecoilX = Mathf.Clamp(_currentRecoilX, 0f, _maxRecoilX);

        _muzzleLight.enabled = true;
        _muzzleFlash.Play();

        if (_audioSource != null && !_audioSource.isPlaying)
        {
            _audioSource.clip = _shotSound;
            _audioSource.Play();

        }


        Ray camRay = new Ray(_virtualCamera.transform.position, _virtualCamera.transform.forward);

        int playerLayer = LayerMask.NameToLayer("Player");
        int playerGunLayer = LayerMask.NameToLayer("PlayerGun");
        int shootLayerMask = ~((1 << playerLayer) | (1 << playerGunLayer));

        if (Physics.Raycast(camRay, out RaycastHit hit, 200f, shootLayerMask))
        {
            Debug.DrawLine(_gunMuzzle.position, hit.point, Color.black, 2f);
            IDamageable enemy = hit.collider.GetComponentInParent<IDamageable>();
            Debug.Log("Hit: " + hit.collider.gameObject.layer + hit.collider.gameObject.name);
            if (enemy != null)
            {
                // Determine damage multiplier based on hit part
                int multiplier = 1;
                if (hit.collider.gameObject.layer == headLayer)
                {
                    multiplier = 5;
                    //enemy.TakeDamage(10 * multiplier, hit.collider, hit.point, hit.normal);
                    enemy.TakeDamage(50, true);
                }

                else if (hit.collider.gameObject.layer == bodyLayer)
                {
                    multiplier = 1;
                    enemy.TakeDamage(5, false);
                    // enemy.TakeDamage(10 * multiplier, hit.collider, hit.point, hit.normal);
                }



            }
        }
        else
        {
            Debug.DrawRay(_gunMuzzle.position, camRay.direction * 200f, Color.blue, 2f);
        }
            _targetOffset += new Vector3(
                Random.Range(-0.1f, 0.1f), // horizontal randomness
                _recoilAmount,             // upward kick (use your existing _recoilAmount)
                0f
            );

        // Clamp the vertical offset so it doesn't go too high
        _targetOffset.y = Mathf.Clamp(_targetOffset.y, 0f, _maxRecoilX);

        currentAmmo--;
        OnUpdateAmoCount?.Invoke();
        GameEvents.TriggerAmoCountChanged();
    }


    public void StopFire()
    {
        if (_gunMuzzle != null)
        {
            _muzzleLight.enabled = false;
             _muzzleFlash.Stop();
        _audioSource.Stop();
        _isFiring = false;
        }
        
    }

    public void Reload()
    {
        if (carriedAmmo <= 0)
        {
            // Play empty click sound
            _audioSource.PlayOneShot(_emptySound);
            return;
        }

        int needed = maxAmmo - currentAmmo;
        if (needed <= 0) return;
        int transfer = Mathf.Min(needed, carriedAmmo);
        currentAmmo += transfer;
        carriedAmmo -= transfer;

        _audioSource.PlayOneShot(_reloadSound);
        GameEvents.TriggerAmoCountChanged();
    }

    public void AddCarriedAmmo(int amount)
    {
        carriedAmmo += amount;
        if (carriedAmmo > maxCarriedAmmo) carriedAmmo = maxCarriedAmmo;
        GameEvents.TriggerAmoCountChanged();
    }

    public int GetCurrentAmo()
    {
        return currentAmmo;
    }

    public int GetCarriedAmo()
    {
        return carriedAmmo;
    }
     public int GetMaxAmo()
    {
        return maxAmmo;
    }
    public int GetMaxCarriedAmo()
    {
        return maxCarriedAmmo;
    }

    public void StartReload()
    {
        if (_isReloading) return;
        if (carriedAmmo <= 0) return;
        if (currentAmmo == maxAmmo) return;

        _isReloading = true;
        if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    public int GetReloadTime()
    {
        return _reloadTime;
    }

    private IEnumerator ReloadCoroutine()
    {
        //_audioSource.PlayOneShot(_reloadSound);
        float reloadDuration = _reloadTime;
        yield return new WaitForSeconds(reloadDuration);

        // Apply ammo only if not cancelled
        int needed = maxAmmo - currentAmmo;
        int transfer = Mathf.Min(needed, carriedAmmo);
        currentAmmo += transfer;
        carriedAmmo -= transfer;

       
        GameEvents.TriggerAmoCountChanged();
        _isReloading = false;
        _reloadCoroutine = null;
    }
    public void PlayReloadSound()
    {
        _audioSource.PlayOneShot(_reloadSound);
    }

    public void CancelReload()
    {
        if (!_isReloading) return;

        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }
        _isReloading = false;
        // Optionally play a "reload cancelled" sound or stop any reload audio
    }
}
