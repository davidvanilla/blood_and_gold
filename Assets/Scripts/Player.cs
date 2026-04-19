using System;
using System.Collections;

using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    private InputSystem_Actions _playerControlls;
    private Vector2 _moveInput;
    private float _moveSpeed = 2f;
    private Animator _animator;
    [SerializeField] private float _rotationSpeed = 10f;
    private Transform _camera;
    [SerializeField] public String Name;
    private bool _isSprinting;
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _sprintSpeed = 10f;

    private bool _isVaulting;       
    [SerializeField] private float _vaultDuration = 1.5f;
    private bool _isAiming = false;
    private CinemachineCamera _virtualCamera;
    [SerializeField] private float _normalFOV = 24f;
    [SerializeField] private float _aimFOV = 10f;
    [SerializeField] private float _zoomSpeed = 10f;

    [SerializeField] private float _normalRadius = 5f;
    [SerializeField] private float _aimRadius = 2f;

    private CinemachineOrbitalFollow _orbitalFollow;

    [Header("Weapon")]
    [SerializeField] private Gun _currentGun;
    [SerializeField] private GameObject playerRightHand;
    [SerializeField] private RawImage _crosshairImage;
    private bool hasRiffle = false;
    [SerializeField] private Image bloodMark;

    private Vector3 _aimPoint;
    private float _nextFireTime;
    [SerializeField] private float _jumpForwardForce = 5f;
    [SerializeField] private float _jumpUpwardForce = 8f;
    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;
    private float _groundCheckDistance = 0.1f;
    private float _groundCheckRadius = 0.40f;

    [Header("Sound")]
    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private AudioClip pickupClip;



    private CapsuleCollider capsuleCollider;
    private bool isGrounded;

    private int defaultHealth = 100;
    private int currentHealth;
    private int extraHealth = 0;

    [SerializeField] private Image bloodOverlay;
    private float bloodFadeDuration = 0.5f; 
    private float maxBloodAlpha = 0.6f;

    public float damageCooldown = 0.5f;
    private float lastDamageTime;
    private bool isDead = false;

    private Coroutine lowHealthFlashCoroutine;
    private Coroutine damageFlashCoroutine;
    public  event Action  UpdateHealth;

    public ItemType nearItem;
    private RiffleType riffleType;
    public GameObject nearItemPrefab;


    private HitReaction hitReaction;
    private int zombieHandLayer;
    private bool _isReloading;
    private Coroutine _reloadCoroutine;

    private Rigidbody _rigidbody;
    private Vector3 _desiredMoveDirection;
    private float _desiredMoveSpeed;
    private Quaternion _targetRotation;

    private void Awake()
    {
        Debug.Log("Awake called");
        _playerControlls = new InputSystem_Actions();
        //_animator = GetComponent<Animator>();
        _animator = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();


        if (_camera == null)
        {
            // Try to find the main camera (or any camera tagged "MainCamera")
            Camera mainCam = Camera.main;
            if (mainCam != null)
                _camera = mainCam.transform;
            else
                Debug.LogError("No camera found! Make sure your camera is tagged 'MainCamera'.");
        }
        if (_virtualCamera == null)
        {
            // Find the Cinemachine camera in the scene
            _virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            if (_virtualCamera == null)
                Debug.LogError("No CinemachineCamera found in scene!");
        }

        _crosshairImage.enabled = false; // Hide crosshair by default

        _rigidbody = GetComponent<Rigidbody>();
        // Optional: prevent physics from rotating the player
        _rigidbody.freezeRotation = true;
    }

    private void Start()
    {
        currentHealth = defaultHealth;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        UpdateHealth?.Invoke();
        bloodMark.enabled = false;
        Debug.Log($" blood mark {bloodMark.IsActive()}");
        hitReaction = GetComponent<HitReaction>();
        zombieHandLayer = LayerMask.NameToLayer("ZombieHand");
        _targetRotation = transform.rotation;
    }

    private void OnEnable()
    {
        _playerControlls.Enable();

        _playerControlls.Player.Move.performed += OnMovePerformed;
        _playerControlls.Player.Move.canceled += OnMoveCanceled;

        _playerControlls.Player.Sprint.performed += OnSprintPerformed;
        _playerControlls.Player.Sprint.canceled += OnSprintCanceled;

    

        _playerControlls.Player.Aim.performed += OnAimPerformed;
        _playerControlls.Player.Aim.canceled += OnAimCanceled;

        _playerControlls.Player.Fire.performed += OnFire;
        _playerControlls.Player.Fire.canceled += OnFireCancelled;

        _playerControlls.Player.Reload.performed += OnReload;

        _playerControlls.Player.Jump.performed += OnJump;

        _playerControlls.Player.Heal.performed += OnHeal;
       
    }


    private void OnDisable()
    {
        _playerControlls.Player.Move.performed -= OnMovePerformed;
        _playerControlls.Player.Move.canceled -= OnMoveCanceled;

        _playerControlls.Player.Sprint.performed -= OnSprintPerformed;
        _playerControlls.Player.Sprint.canceled -= OnSprintCanceled;

        
        _playerControlls.Player.Aim.performed -= OnAimPerformed;
        _playerControlls.Player.Aim.canceled -= OnAimCanceled;

        _playerControlls.Player.Fire.performed -= OnFire;
        _playerControlls.Player.Fire.canceled -= OnFireCancelled;

        _playerControlls.Player.Jump.canceled -= OnJump;

        _playerControlls.Player.Heal.canceled -= OnHeal;


        _playerControlls.Disable();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            _animator.SetTrigger("Jump");

            // Add a forward impulse
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Use the player's forward direction (which is the facing direction)
                Vector3 jumpForce = Vector3.up * _jumpUpwardForce + transform.forward * _jumpForwardForce;
                rb.AddForce(jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void OnAimPerformed(InputAction.CallbackContext context)
    {
        if (_isReloading) CancelReload();

        _isAiming = true;
        _currentGun.UpdateAiming(true);
        _crosshairImage.enabled = true;
        _animator.SetBool("Aim", _isAiming);
        Debug.Log($"Mouse clicked {_isAiming}");
        _isSprinting = false;
        _animator.SetBool("Sprint", false);
    }

    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Aim cancelled");
        _currentGun.UpdateAiming(false);
        _crosshairImage.enabled = false;
        _isAiming = false;
        _animator.SetBool("Aim", _isAiming);

    }

    private void OnFire(InputAction.CallbackContext context)
    {
        Debug.Log("Fire performed");
        if (!_isAiming) return;

        _currentGun.Fire();

    }

    private void OnFireCancelled(InputAction.CallbackContext context)
    {
        _currentGun.StopFire();
       
    }
  

    private void OnReload(InputAction.CallbackContext context)
    {
        // do not reload if in aiming mode
        if(_isAiming) return;
        if(_isSprinting) return;

        if (_currentGun.GetCarriedAmo() <= 0) return;
        if(_currentGun.GetCurrentAmo() == _currentGun.GetMaxAmo()) return;
        
        _animator.SetBool("Reload", true);
        _isReloading = true;
        _currentGun.StartReload();
        if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = StartCoroutine(ReloadRoutine());
    }

    public void OnReloadSound()
    {
        if (_currentGun != null)
            _currentGun.PlayReloadSound();
    }
    private IEnumerator ReloadRoutine()
    {
        float reloadTime = _currentGun.GetReloadTime();
        float elapsed = 0f;

        while (elapsed < reloadTime)
        {
            // Cancel reload if player starts sprinting or aiming mid-reload
            if (_isSprinting || _isAiming)
            {
                CancelReload();
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reload completed successfully
        
        _animator.SetBool("Reload", false);
        _isReloading = false;
        _reloadCoroutine = null;
    }

    private void CancelReload()
    {
        if (!_isReloading) return;

        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        _animator.SetBool("Reload", false);
        _isReloading = false;

    }





    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Only start vault if not already vaulting
        if (!_isVaulting)
        {
            StartCoroutine(PlayVault());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider belongs to the zombie's hand
        if (other.gameObject.layer == zombieHandLayer)
        {
            // Apply damage only after cooldown
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                TakeZombieDamage(); 
                lastDamageTime = Time.time;
            }
        }
    }

    public bool GetIsDead()
    {
        return isDead;
    }
    public void TakeZombieDamage()
    {
        // Existing code
        //_animator.SetTrigger("Hit");
        currentHealth -= 10;
        Debug.Log($"current player health {currentHealth}");

        // Show blood effect on screen
        if(currentHealth < 90)
        {
            if (bloodOverlay != null)
            {
                if (damageFlashCoroutine != null)
                    StopCoroutine(damageFlashCoroutine);
                damageFlashCoroutine = StartCoroutine(ShowBloodEffect());
            }
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            if(hasRiffle)
                _animator.SetTrigger("Die1");
            else
                _animator.SetTrigger("DieNW");
        }

        GameEvents.TriggerHealthChanged();
    }


    


    public void TakeGunDamage()
    {
        // Existing code
        //_animator.SetTrigger("Hit");

        hitReaction.TriggerHitReaction(0.5f);
        currentHealth -= 10;
        Debug.Log($"current player health {currentHealth}");
        StartCoroutine(ShowBloodMark());
        // Show blood effect on screen
        if (currentHealth < 20)
        {
            if (bloodOverlay != null)
            {
                if (damageFlashCoroutine != null)
                    StopCoroutine(damageFlashCoroutine);
                damageFlashCoroutine = StartCoroutine(ShowBloodEffect());
            }
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            if (hasRiffle)
                _animator.SetTrigger("Die1");
            else
                _animator.SetTrigger("DieNW");
        }

        GameEvents.TriggerHealthChanged();
    }


    private IEnumerator ShowBloodMark()
    {
        float elapsed = 0f;
        while (elapsed < bloodFadeDuration)
        {
            elapsed += Time.deltaTime;
            bloodMark.enabled = true;
            yield return null;
        }

        bloodMark.enabled = false;
    }
    private IEnumerator ShowBloodEffect()
    {
        // Set blood to max alpha instantly
        Color color = bloodOverlay.color;
        color.a = maxBloodAlpha;
        bloodOverlay.color = color;

        // Fade out over time
        float elapsed = 0f;
        while (elapsed < bloodFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(maxBloodAlpha, 0f, elapsed / bloodFadeDuration);
            color.a = alpha;
            bloodOverlay.color = color;
            yield return null;
        }

        // Ensure fully transparent at the end
        color.a = 0f;
        bloodOverlay.color = color;
    }



    private IEnumerator PlayVault()
    {
        _isVaulting = true;

        _animator.SetBool("Vault", true);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.forward * 4f; // forward vault distance

        float time = 0f;

        while (time < _vaultDuration)
        {
            time += Time.deltaTime;

            float t = time / _vaultDuration;

            // Forward movement
            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);

            // Arc movement (up then down)
            float height = Mathf.Sin(t * Mathf.PI) * 0.1f;

            transform.position = horizontal + Vector3.up * height;

            yield return null;
        }

        transform.position = endPos;

        _animator.SetBool("Vault", false);

        _isVaulting = false;
    }

  

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        _isSprinting = true;
        if (_isReloading)
        {
            CancelReload();
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        _isSprinting = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero; // Stop moving
    }

    private IEnumerator FlashBloodEffect()
    {
        while (true)  // infinite loop until stopped
        {
            // Fade in to max alpha
            float elapsed = 0f;
            float fadeDuration = 0.2f;  // quick fade in

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, maxBloodAlpha, elapsed / fadeDuration);
                Color c = bloodOverlay.color;
                c.a = alpha;
                bloodOverlay.color = c;
                yield return null;
            }

            // Hold at max alpha briefly
            yield return new WaitForSeconds(0.5f);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(maxBloodAlpha, 0f, elapsed / fadeDuration);
                Color c = bloodOverlay.color;
                c.a = alpha;
                bloodOverlay.color = c;
                yield return null;
            }

            // Pause before next flash
            yield return new WaitForSeconds(0.8f);
        }
    }

    private void Update()
    {
        // ---- Low health blood effect ----
        if (currentHealth < 20 && lowHealthFlashCoroutine == null && isDead == true)
        {
            lowHealthFlashCoroutine = StartCoroutine(FlashBloodEffect());
        }
        else if (currentHealth >= 20 && lowHealthFlashCoroutine != null)
        {
            StopCoroutine(lowHealthFlashCoroutine);
            lowHealthFlashCoroutine = null;
            Color c = bloodOverlay.color;
            c.a = 0f;
            bloodOverlay.color = c;
        }

        // ---- Ground check ----
        Vector3 bottom = transform.position + Vector3.down * (capsuleCollider.height / 2f - capsuleCollider.radius);
        isGrounded = Physics.CheckSphere(bottom, _groundCheckRadius, _groundLayer);

        // ---- Camera zoom when aiming ----
        if (_orbitalFollow != null)
        {
            float targetRadius = _isAiming ? _aimRadius : _normalRadius;
            _orbitalFollow.Radius = Mathf.Lerp(
                _orbitalFollow.Radius,
                targetRadius,
                Time.deltaTime * _zoomSpeed
            );
        }

        // ---- Movement direction calculation (camera-relative) ----
        Vector3 camForward = _camera.forward;
        Vector3 camRight = _camera.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        _moveSpeed = _isSprinting ? _sprintSpeed : _walkSpeed;
        _desiredMoveDirection = camForward * _moveInput.y + camRight * _moveInput.x;
        _desiredMoveSpeed = _moveSpeed;

        // ---- Animation parameters ----
        float speed = _moveInput.magnitude;
        _animator.SetFloat("Walk", speed);
        if (_isSprinting && !_isAiming)
        {
            _animator.SetBool("Sprint", true);
        }
        else
        {
            _animator.SetBool("Sprint", false);
        }

        // ---- Calculate target rotation (but DO NOT apply it here) ----
        if (_isAiming)
        {
            // While aiming, face the horizontal direction of the camera
            Vector3 cameraForward = _camera.forward;
            cameraForward.y = 0;
            if (cameraForward.sqrMagnitude > 0.01f)
            {
                _targetRotation = Quaternion.LookRotation(cameraForward);
            }
        }
        else
        {
            // Not aiming: face movement direction if moving
            if (_desiredMoveDirection.sqrMagnitude > 0.01f)
            {
                _targetRotation = Quaternion.LookRotation(_desiredMoveDirection);
            }
        }
    }

    private void FixedUpdate()
    {
        // ---- Apply rotation using physics (synchronized with movement) ----
        if (_targetRotation != null) // Quaternion is a struct, but we can check if it's not default
        {
            // Smoothly rotate toward the target rotation using physics interpolation
            _rigidbody.MoveRotation(Quaternion.Slerp(
                _rigidbody.rotation,
                _targetRotation,
                _rotationSpeed * Time.fixedDeltaTime
            ));
        }

        // ---- Apply movement using physics ----
        if (_desiredMoveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 newPosition = _rigidbody.position + _desiredMoveDirection * _desiredMoveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);
        }
    }


    void OnAnimatorIK(int layerIndex)
    {

        if (!_isAiming) return; // enable only when aiming

        Vector3 lookDirection = _camera.forward; // <-- negative
        Vector3 lookAtPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position + lookDirection * 10f;

        // Optional: clamp the vertical angle to avoid extreme twists
        // (you can add a pitch limit if needed)

        _animator.SetLookAtWeight(1f, 0.3f, 0.8f, 1f, 0.5f);
        _animator.SetLookAtPosition(lookAtPosition);


        //if (!_isAiming) return;

        //_animator.SetLookAtWeight(1f);
        //_animator.SetLookAtPosition(_aimPoint);
    }


    public Gun GetCurrentGun()
    {
        return _currentGun;
    }

    public GameObject GetCurrentGunObject()
    {
        return _currentGun.gameObject;
    }

    public void SetCurrentGun(Gun newGun)
    {
        _currentGun = newGun;
        GameEvents.TriggerAmoCountChanged();
    }

    public void SetRiffleStateAnimation()
    {
        hasRiffle = true;
        _animator.SetBool("HasRiffle", true);
    }

    public bool GetHasRiffle()
    {
        return hasRiffle;
    }

    public int GetHealth()
    {
        return currentHealth;
    }
    public int GetExtraHealth()
    {
        return extraHealth;
    }

    public void SetNearItem(ItemType itemType)
    {
        nearItem = itemType;
    }

    public ItemType GetNearItem()
    {
        return nearItem;
    }

    public void SetRiffleType(RiffleType type)
    {
        riffleType = type;
    }

    public RiffleType GetRiffleType()
    {
        return riffleType;
    }

    public void SetItemPrefab(GameObject prefab)
    {
        nearItemPrefab = prefab;
    }
    public GameObject GetItemPrefab()
    {
        return nearItemPrefab;
    }
    public void AddExtraHealth(int amount)
    {
        if(extraHealth >= 100) return;
        extraHealth += amount;
    }
     public void Heal()
    {
        currentHealth += extraHealth;
        // extra health is max 100
        extraHealth = 0;

        UpdateHealth?.Invoke();
    }

    private void OnHeal(InputAction.CallbackContext context)
    {
        if(extraHealth <= 0) return;
        Heal();
        playerAudio.PlayOneShot(healClip);
    }

    public void PlayPickup()
    {
        playerAudio.PlayOneShot(pickupClip);
    }

    public GameObject GetPlayerRightHand()
    {
        return playerRightHand;
    }

    public void AddCarriedAmo(int amount)
    {
               if (_currentGun == null) return;
        _currentGun.AddCarriedAmmo(amount);
    }
}
