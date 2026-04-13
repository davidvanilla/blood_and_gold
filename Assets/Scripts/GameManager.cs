using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputSystem_Actions _playerControlls;
    private void Awake()
    {
        _playerControlls = new InputSystem_Actions();
        _playerControlls.Enable();
    }

    private void OnEnable()
    {
        _playerControlls.UI.Escape.performed += OnEscape;
        _playerControlls.UI.LeftClick.performed += OnLeftClick;
    }

    private void OnDisable()
    {
        _playerControlls.UI.Escape.performed -= OnEscape;
        _playerControlls.UI.LeftClick.performed -= OnLeftClick;
        _playerControlls.Disable();
    }
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnEscape(InputAction.CallbackContext ctx)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
       
    }
}