using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }

    void Update()
    {
        //Use raycasting to create a distance offset that can be used to stop the camera clipping through walls (doesn't work perfectly)
        relativePosition = thirdPersonPosition - (target.position);
        RaycastHit hit;
        if (Physics.Raycast(target.position, relativePosition, out hit, dstFromTarget + 0.5f))
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

        // Allow camera zooming with scroll wheel
        dstFromTarget -= Input.GetAxis("Mouse ScrollWheel") * 2f;
        if (dstFromTarget > 2)
            dstFromTarget = 2;
        if (dstFromTarget < 0)
            dstFromTarget = 0;

        //Handles camera angle rotation independently of position
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
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
