using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPCamera : MonoBehaviour
{
    public bool lockCursor;
    public float mouseSensitivity = 10;
    public float dstFromTarget = 1;
    public float rotationSmoothTime = .12f;
    public float distanceOffset;
    public Transform target;
    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;
    private Vector3 relativePosition;
    private Vector3 thirdPersonPosition;
    private Vector2 pitchMinMax = new Vector2(-40, 85);
    private float yaw;
    private float pitch;
    private InputAction lookLeft;
    private InputAction lookRight;
    private InputAction lookUp;
    private InputAction lookDown;

    public void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        lookLeft = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta/x", interactions: "");
        lookRight = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta/x", interactions: "");
        lookUp = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta/y", interactions: "");
        lookDown = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta/y", interactions: "");

        lookLeft.Enable();
        lookRight.Enable();
        lookUp.Enable();
        lookDown.Enable();
    }

    void Update()
    {
        //Use raycasting to create a distance offset that can be used to stop the camera clipping through walls (doesn't work perfectly)
        relativePosition = thirdPersonPosition - (target.position);
        if (Physics.Raycast(target.position, relativePosition, out var hit, dstFromTarget + 0.5f))
        {
            Debug.DrawLine(target.position, hit.point);
            distanceOffset = dstFromTarget - hit.distance + 0.8f;
            distanceOffset = Mathf.Clamp(distanceOffset, 0, dstFromTarget);
        }
        else
        {
            distanceOffset = 0;
        }
    }

    void LateUpdate()
    {
        //Handles camera angle rotation independently of position
        yaw += lookRight.ReadValue<float>() * mouseSensitivity;
        pitch -= lookUp.ReadValue<float>() * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        thirdPersonPosition = target.position - transform.forward * (dstFromTarget - distanceOffset);
        //Sets camera position to first person
        transform.position = new Vector3(target.parent.GetComponent<Collider>().bounds.center.x,
        target.parent.GetComponent<Collider>().bounds.center.y + (target.parent.GetComponent<Collider>().bounds.size.y/3),
        target.parent.GetComponent<Collider>().bounds.center.z);
    }
}
