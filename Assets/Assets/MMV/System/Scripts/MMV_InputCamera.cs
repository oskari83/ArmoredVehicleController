using System;
using UnityEngine;

namespace MMV
{
    [Serializable]
    public class MMV_InputCamera : MMV_Input
    {
        /// <summary>
        /// Camera inputs
        /// </summary>
        [Serializable]
        public class CameraInput
        {
            [SerializeField] private string vertical;
            [SerializeField] private string horizontal;
            [SerializeField] private KeyCode cameraZoom;

            [SerializeField] private bool invertZoom;
            [SerializeField] private bool invertVertical;
            [SerializeField] private bool invertHorizontal;

            /// <summary>
            /// Vertical input axis
            /// </summary>
            /// <value></value>
            public string Vertical { get => vertical; set => vertical = value; }

            /// <summary>
            /// Horizontal input axis
            /// </summary>
            /// <value></value>
            public string Horizontal { get => horizontal; set => horizontal = value; }

            /// <summary>
            ///Chave para aplicar o zoom da câmera
            /// </summary>
            /// <value></value>
            public KeyCode CameraZoom { get => cameraZoom; set => cameraZoom = value; }

            /// <summary>
            /// Invert horizontal axis
            /// </summary>
            /// <value></value>
            public bool InvertHorizontal { get => invertHorizontal; set => invertHorizontal = value; }

            /// <summary>
            /// Invert vertical axis
            /// </summary>
            /// <value></value>
            public bool InvertVertical { get => invertVertical; set => invertVertical = value; }
        }

        /// <summary>
        /// Keyboad inputs
        /// </summary>
        [SerializeField] private CameraInput keyboard;

        /// <summary>
        /// Gamepad inputs
        /// </summary>
        [SerializeField] private CameraInput gamepad;

        /// <summary>
        /// Read the control input or change keys of keyboard
        /// </summary>
        /// <value></value>
        public CameraInput Keyboard { get => keyboard; set => keyboard = value; }

        /// <summary>
        /// Read the control input or chance buttons of gamepad
        /// </summary>
        /// <value></value>
        public CameraInput Gamepad { get => gamepad; set => gamepad = value; }

        /// <summary>
        /// Get vertical axis of vehicle controll
        /// </summary>
        /// <returns>
        /// vertical input axis
        /// </returns>
        public float VerticalAxis { get => AxisValue(keyboard.Vertical, gamepad.Vertical, keyboard.InvertVertical, gamepad.InvertVertical); }

        /// <summary>
        /// Get horizontal axis of vehicle controll
        /// </summary>
        /// <returns>
        /// horizontal input axis
        /// </returns>
        public float HorizontalAxis { get => AxisValue(keyboard.Horizontal, gamepad.Horizontal, keyboard.InvertHorizontal, gamepad.InvertHorizontal); }

        /// <summary>
        /// Check if zoom keycode is pressed
        /// </summary>
        /// <returns></returns>
        public bool ApplyZoom => Input.GetKey(keyboard.CameraZoom) || Input.GetKey(gamepad.CameraZoom);

    }
}