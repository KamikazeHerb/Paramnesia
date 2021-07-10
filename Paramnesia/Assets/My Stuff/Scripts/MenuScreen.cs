using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuScreen : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject winScreen;
    public InputAction enter;
    public InputAction escape;

    // Start is called before the first frame update
    void Start()
    {
        enter = new InputAction(type: InputActionType.Button, binding: "<keyboard>/enter", interactions: "");
        escape = new InputAction(type: InputActionType.Button, binding: "<keyboard>/escape", interactions: "");
        enter.Enable();
        escape.Enable();
        enter.performed += ctx =>
        {
            if (pauseMenu.activeSelf)
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
    }
}
