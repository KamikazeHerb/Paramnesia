using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        public Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
        private InputAction moveLeft;
        private InputAction moveRight;
        private InputAction moveForward;
        private InputAction moveBack;
        private InputAction crouch;
        private InputAction sprint;
        private InputAction jump;
        

        private void Start()
        {

            //Configure input action
            moveLeft = new InputAction( type: InputActionType.Button, binding: "<keyboard>/a", interactions: "");
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

            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }


        private void Update()
        {
            if (!m_Jump)
            {
                m_Jump = jump.ReadValue<float>() > 0;
            }
            if (sprint.ReadValue<float>() == 0)
            {
                Vector3.ClampMagnitude(m_Move, 0.49f);
            }
            else
            {
               m_Move = Vector3.ClampMagnitude(m_Move, 1.2f);
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float v = moveForward.ReadValue<float>() - moveBack.ReadValue<float>();
            float h = moveRight.ReadValue<float>() - moveLeft.ReadValue<float>();

            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v*m_CamForward + h*m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v*Vector3.forward + h*Vector3.right;
            }
            m_Move.Normalize();
            // walk speed multiplier
            if (sprint.ReadValue<float>() == 0 && !m_Character.m_Crouching)
            {
                m_Move *= 0.5f;
                Vector3.ClampMagnitude(m_Move, 0.49f);
            }

            if (sprint.ReadValue<float>() == 0)
            {
                Vector3.ClampMagnitude(m_Move, 0.49f);
            }
            else
            {
                Vector3.ClampMagnitude(m_Move, 1.2f);
            }
            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch.ReadValue<float>() == 1, m_Jump);
            m_Jump = false;
        }
    }
}
