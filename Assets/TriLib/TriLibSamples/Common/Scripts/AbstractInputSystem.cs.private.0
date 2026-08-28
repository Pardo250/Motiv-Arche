using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace TriLibCore.Samples
{
    /// <summary>
    /// Represents a class to abstract input system actions.
    /// </summary>
    public class AbstractInputSystem : MonoBehaviour
    {
        /// <summary>
        /// Helper method to get a mouse button using the legacy and new input systems.
        /// </summary>
        protected bool GetMouseButton(int index)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                switch (index)
                {
                    case 0:
                        return Mouse.current.leftButton.isPressed;
                    case 1:
                        return Mouse.current.rightButton.isPressed;
                    case 2:
                        return Mouse.current.middleButton.isPressed;
                }
            }
            return false;
#else
            return Input.GetMouseButton(index);
#endif
        }

        /// <summary>
        /// Helper method to get a mouse button down using the legacy and new input systems.
        /// </summary>
        protected bool GetMouseButtonDown(int index)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                switch (index)
                {
                    case 0:
                        return Mouse.current.leftButton.wasPressedThisFrame;
                    case 1:
                        return Mouse.current.middleButton.wasPressedThisFrame;
                    case 2:
                        return Mouse.current.rightButton.wasPressedThisFrame;
                }
            }
            return false;
#else
            return Input.GetMouseButtonDown(index);
#endif
        }

        /// <summary>
        /// Helper method to get an axis value using the legacy and new input systems.
        /// </summary>
        protected float GetAxis(string axisName)
        {
#if ENABLE_INPUT_SYSTEM
            switch (axisName)
            {
                case "Mouse X":
                    return Mouse.current != null ? Mouse.current.delta.x.ReadValue() : 0f;
                case "Mouse Y":
                    return Mouse.current != null ? Mouse.current.delta.y.ReadValue() : 0f;
                case "Horizontal":
                    if (Keyboard.current == null)
                    {
                        return 0f;
                    }
                    return Keyboard.current.leftArrowKey.isPressed ? -1f :
                           Keyboard.current.rightArrowKey.isPressed ? 1f : 0f;
                case "Vertical":
                    if (Keyboard.current == null)
                    {
                        return 0f;
                    }
                    return Keyboard.current.downArrowKey.isPressed ? -1f :
                           Keyboard.current.upArrowKey.isPressed ? 1f : 0f;
                default:
                    return 0f;
            }
#else
            return Input.GetAxis(axisName);
#endif
        }

        /// <summary>
        /// Helper method to return a keyboard key using the legacy and new input systems.
        /// </summary>
        protected bool GetKey(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (keyCode)
                {
                    case KeyCode.LeftAlt:
                        return Keyboard.current.leftAltKey.isPressed;
                    case KeyCode.RightAlt:
                        return Keyboard.current.rightAltKey.isPressed;
                }
            }
            return false;
#else
            return Input.GetKey(keyCode);
#endif
        }

        /// <summary>
        /// Helper method to return the mouse scroll delta using the legacy and new input systems.
        /// </summary>
        protected Vector2 GetMouseScrollDelta()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.scroll.ReadValue() * 0.01f: default;
#else
            return Input.mouseScrollDelta;
#endif
        }
    }
}