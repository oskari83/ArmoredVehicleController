using UnityEngine;

namespace MMV
{
    public class MMV_MBT_ControllVehicle : MonoBehaviour
    {
        [SerializeField] private MMV_MBT_InputVehicle input;

        private MMV_MBT_Vehicle vehicle;

        /// <summary>
        /// vehicle target for controll
        /// </summary>
        /// <value></value>
        public MMV_MBT_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        /// <summary>
        /// Inputs to crontroll MBT vehicle
        /// </summary>
        /// <value></value>
        public MMV_MBT_InputVehicle Input { get => input; set => input = value; }

        private void Start()
        {
            vehicle = GetComponentInChildren<MMV_MBT_Vehicle>();
        }

        void Update()
        {
            if (!vehicle)
            {
                return;
            }

            // Applie control    
            vehicle.PlayerInputs(input.VerticalAxis, input.HorizontalAxis, input.Braking);
        }
    }
}

