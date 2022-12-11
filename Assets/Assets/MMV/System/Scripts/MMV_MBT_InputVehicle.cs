using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Storage all inputs of vehicle controll
    /// </summary>
    [Serializable]
    public class MMV_MBT_InputVehicle : MMV_Input
    {
        /// <summary>
        ///Vehicle inputs
        /// </summary>
        [Serializable]
        public struct VehicleInput
        {
            public string vertical;     // forward / backward movement
            public string horizontal;   // rotate the vehicle

            public KeyCode brake;       // stop vehicle
        }

        /// <summary>
        /// Keyboad inputs
        /// </summary>
        [SerializeField] private VehicleInput keyboard;

        /// <summary>
        /// Gamepad inputs
        /// </summary>
        [SerializeField] private VehicleInput gamepad;

        /// <summary>
        /// Read the control input or change keys of keyboard
        /// </summary>
        /// <value></value>
        public VehicleInput Keyboard { get => keyboard; set => keyboard = value; }

        /// <summary>
        /// Read the control input or chance buttons of gamepad
        /// </summary>
        /// <value></value>
        public VehicleInput Gamepad { get => gamepad; set => gamepad = value; }

        /// <summary>
        /// Get vertical axis of vehicle controll
        /// </summary>
        /// <returns>
        /// vertical input axis
        /// </returns>
        public float VerticalAxis => AxisValue(keyboard.vertical, gamepad.vertical);

        /// <summary>
        /// Get horizontal axis of vehicle controll
        /// </summary>
        /// <returns>
        /// horizontal input axis
        /// </returns>
        public float HorizontalAxis => AxisValue(keyboard.horizontal, gamepad.horizontal);

        /// <summary>
        /// Check if vehicle is braking
        /// </summary>
        /// <returns>
        /// If keycode brake is pressed
        /// </returns>
        public bool Braking => Input.GetKey(Keyboard.brake) || Input.GetKey(gamepad.brake);
    }
}
