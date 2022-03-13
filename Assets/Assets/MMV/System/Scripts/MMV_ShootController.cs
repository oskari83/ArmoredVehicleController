using System;
using UnityEngine;

namespace MMV
{
    public class MMV_ShootController : MonoBehaviour
    {
        [Serializable]
        public class InputShoot : MMV_Input
        {
            [SerializeField] private KeyCode mouseShoot;
            [SerializeField] private KeyCode gamepadShoot;

            /// <summary>
            /// Keyboard key or mouse button to shoot
            /// </summary>
            /// <value></value>
            public KeyCode Key { get => mouseShoot; set => mouseShoot = value; }

            /// <summary>
            /// Gamepad button to shoot
            /// </summary>
            /// <value></value>
            public KeyCode GamepadButton { get => gamepadShoot; set => gamepadShoot = value; }

            /// <summary>
            /// If player is shooting
            /// </summary>
            /// <returns></returns>
            public bool IsShooting => Press(mouseShoot) || Press(gamepadShoot);
        }

        [SerializeField] private InputShoot inputs;


        //-----------------------------------------

        private MMV_Shooter shootControl;

        //-----------------------------------------


        /// <summary>
        /// User input to shoot
        /// </summary>
        /// <value></value>
        public InputShoot Inputs => inputs;


        // Start is called before the first frame update
        void Start()
        {
            shootControl = GetComponentInChildren<MMV_Shooter>();

            if (!shootControl)
            {
                Debug.LogWarning("Shoot control not added on vehicle. Add MMV_Shooter on vehicle components");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!shootControl)
            {
                return;
            }

            if (inputs.IsShooting)
            {
                shootControl.Shoot();
            }
        }
    }
}