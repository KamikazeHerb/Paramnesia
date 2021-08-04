using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonUserControl : MonoBehaviour
{
    private InputAction moveLeft;
    private InputAction moveRight;
    private InputAction moveForward;
    private InputAction moveBack;
    private InputAction crouch;
    private InputAction sprint;
    private InputAction jump;
    // Start is called before the first frame update
    void Start()
    {
        //Configure input action
        moveLeft = new InputAction(type: InputActionType.Button, binding: "<keyboard>/a", interactions: "");
        moveRight = new InputAction(type: InputActionType.Button, binding: "<keyboard>/d", interactions: "");
        moveForward = new InputAction(type: InputActionType.Button, binding: "<keyboard>/w", interactions: "");
        moveBack = new InputAction(type: InputActionType.Button, binding: "<keyboard>/s", interactions: "");
        crouch = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton", interactions: "");
        sprint = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton", interactions: "");
        jump = new InputAction(type: InputActionType.Button, binding: "<keyboard>/space", interactions: "");

        moveLeft.Enable();
        moveRight.Enable();
        moveForward.Enable();
        moveBack.Enable();
        crouch.Enable();
        sprint.Enable();
        jump.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
