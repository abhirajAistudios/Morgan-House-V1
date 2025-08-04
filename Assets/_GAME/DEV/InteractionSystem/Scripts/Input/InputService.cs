using UnityEngine;

namespace HorrorGame.GameInput
{
    public class InputService : MonoBehaviour
    {
        [Header("Movement Inputs")]
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        [Header("Action Inputs")]
        public bool JumpPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool FirePressed { get; private set; }
        public bool SprintHeld { get; private set; }

        public void Update()
        {
            // Movement
            MoveInput = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));

            // Look (for mouse-based look)
            LookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Actions
            JumpPressed = Input.GetButtonDown("Jump");
            InteractPressed = Input.GetKeyDown(KeyCode.Z);
            FirePressed = Input.GetMouseButtonDown(0);
            SprintHeld = Input.GetKey(KeyCode.LeftShift);
        }
    }
}