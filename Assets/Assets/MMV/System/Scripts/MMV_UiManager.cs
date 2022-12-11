using System;
using UnityEngine;
using UnityEngine.UI;

namespace MMV
{
    public class MMV_UiManager : MonoBehaviour
    {
        [SerializeField] private RectTransform crosshair;
        [SerializeField] private float crosshairMoveSpeed;
        [SerializeField] private Text reload;

        [SerializeField] private MMV_CameraController cameraController;
        [SerializeField] private MMV_MBT_Vehicle vehicle;

        [SerializeField] private Text velocity;
        [SerializeField] private Text gear;
        [SerializeField] private Text rightBrake;


        //------------------------------------------

        private MMV_Shooter shooter;
        private Vector3 finalCrosshairPos;  // used to lerp when assigning the crosshair mark in the center
        private Image crosshairImage;

        //------------------------------------------

        /// <summary>
        /// Gun crosshair sprite transform
        /// </summary>
        public RectTransform Crosshair { get => crosshair; set => crosshair = value; }

        /// <summary>
        /// Vehicle target
        /// </summary>
        public MMV_MBT_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        /// <summary>
        /// Current recharge time marker
        /// </summary>
        /// <value></value>
        public Text Reload { get => reload; set => reload = value; }

        /// <summary>
        /// Camera controller of vehicle
        /// </summary>
        /// <value></value>
        public MMV_CameraController CameraController { get => cameraController; set => cameraController = value; }

        /// <summary>
        /// The speed that the crosshair marker moves
        /// </summary>
        /// <value></value>
        public float CrosshairMoveSpeed { get => crosshairMoveSpeed; set => crosshairMoveSpeed = value; }

        /// <summary>
        /// UI gear text component
        /// </summary>
        /// <value></value>
        public Text Gear { get => gear; set => gear = value; }

        /// <summary>
        /// UI velocity text component
        /// </summary>
        /// <value></value>
        public Text Velocity { get => velocity; set => velocity = value; }

        /// <summary>
        /// Right track brake force text component
        /// </summary>
        /// <value></value>
        public Text RightBrake { get => rightBrake; set => rightBrake = value; }

        void Start()
        {
            shooter = vehicle.GetComponentInChildren<MMV_Shooter>();

            if (crosshair)
            {
                crosshairImage = crosshair.GetComponent<Image>();
            }
        }

        void Update()
        {
            if (!vehicle)
            {
                return;
            }

            if (vehicle.Turret.Gun && crosshair) CrosshairMovement();
            if (reload) UseReloadtext();


            //------------------------------------------

            if (gear) ShowText(gear, vehicle.CurrentGear.ToString(), "gear: ");
            if (velocity) ShowText(velocity, vehicle.LocalVelocity.z.ToString("0"), "velocity: ");
        }

        /// <summary>
        /// Move crosshair on screen
        /// </summary>
        private void CrosshairMovement()
        {
            if (!cameraController)
            {
                return;
            }

            //------------------------------------

            var _mainCamera = Camera.main;
            var _gun = vehicle.Turret.Gun;
            var _maxCrosshairDistance = cameraController.GunCrosshair.MaxDistance;
            var _forwardGun = _gun.transform.position + (_gun.forward * _maxCrosshairDistance);

            // check for obstacles in front of the cannon
            if (Physics.Raycast(_gun.transform.position, _gun.transform.forward, out RaycastHit hit, _maxCrosshairDistance))
            {
                _forwardGun = hit.point;
            }

            // placing the target's sprite in front of the cannon
            var _lookToHit = Quaternion.LookRotation(_forwardGun - _mainCamera.transform.position);
            var _crosshairPos = _mainCamera.transform.position + (_lookToHit * Vector3.forward);

            // disable the sight if the is not in front of the camera
            var _angleBetweenGunAndCamera = Mathf.Abs(Vector3.Angle(_gun.forward, _mainCamera.transform.forward));
            crosshairImage.enabled = _angleBetweenGunAndCamera > _mainCamera.fieldOfView ? false : true;

            // convert the world position of the crosshairs to the position of the screen (smoothed)
            finalCrosshairPos = Vector3.Lerp(finalCrosshairPos, _mainCamera.WorldToScreenPoint(_crosshairPos), Time.deltaTime * crosshairMoveSpeed);
            crosshair.position = finalCrosshairPos;
        }

        /// <summary>
        /// Show reload text on center of screen
        /// </summary>
        private void UseReloadtext()
        {
            if (!shooter)
            {
                return;
            }

            if (shooter.IsReloading)
            {
                reload.enabled = true;
                reload.text = shooter.ReloadProgress.ToString("0.00") + "s";
            }
            else
            {
                reload.enabled = false;
            }
        }

        private void ShowText(Text textComponent, string message, string aditional, bool aditionalOnFront = true)
        {
            string _finalMessage = aditionalOnFront ? aditional + message : message + aditional;
            textComponent.text = _finalMessage;
        }
    }
}
