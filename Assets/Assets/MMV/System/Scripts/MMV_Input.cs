using System;
using UnityEngine;

namespace MMV
{
    public class MMV_Input
    {
        /// <summary>
        /// Get axis of two different inputs
        /// </summary>
        /// <param name="keyboard">
        /// Keyboard axis input
        /// </param>
        /// <param name="gamepad">
        /// Gamepad axis input
        /// </param>
        /// <returns>
        /// Axis force
        /// </returns>
        protected float AxisValue(string keyboard, string gamepad, bool invertKeyboardAxis = false, bool invertGamepadAxis = false)
        {
            var _keyboardNull = String.IsNullOrEmpty(keyboard);
            var _gamepadNull = String.IsNullOrEmpty(gamepad);

            var _keyboardAxis = !_keyboardNull ? Input.GetAxisRaw(keyboard) : 0;
            var _gamepadAxis = !_gamepadNull ? Input.GetAxis(gamepad) : 0;

            if (invertKeyboardAxis) _keyboardAxis *= -1;
            if (invertGamepadAxis) _gamepadAxis *= -1;

            return Mathf.Clamp(_keyboardAxis + _gamepadAxis, -1, 1);
        }

        /// <summary>
        /// Check if is pressing key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool Pressing(KeyCode key)
        {
            return Input.GetKey(key);
        }

        /// <summary>
        /// Check that the key is pressed once
        /// </summary>
        /// <param name="key">
        /// The key
        /// </param>
        /// <returns></returns>
        protected bool Press(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }
    }
}
