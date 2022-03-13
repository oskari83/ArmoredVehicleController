using System;
using UnityEngine;

namespace MMV
{
    [Serializable]
    public class MMV_TurretController
    {
        [SerializeField] private Transform turret;
        [SerializeField] private Transform gun;

        [SerializeField] private float rotSpeedHorizontal;
        [SerializeField] private float rotSpeedVertical;

        [SerializeField] private int maxGunAngle;
        [SerializeField] private int minGunAngle;

        private MMV_MBT_Vehicle vehicle;

        //-----------------------------------------------

        private Vector3 targetPosition;
        private Quaternion gunLocalRotation;
        private Quaternion turretLocalRotation;

        bool turretEnabled;


        //-----------------------------------------------

        /// <summary>
        /// Vehicle owner of the turret
        /// </summary>
        public MMV_MBT_Vehicle Vehicle => vehicle;

        /// <summary>
        /// Where the turret look
        /// </summary>
        /// <value></value>
        public Vector3 TargetPosition { set => targetPosition = value; get => targetPosition; }

        /// <summary>
        /// Max angle to up
        /// </summary>
        /// <value></value>
        public int MaxGunAngle { get => maxGunAngle; set => maxGunAngle = value; }

        /// <summary>
        /// Max angle to down
        /// </summary>
        /// <value></value>
        public int MinGunAngle
        {
            get => minGunAngle;
            set => minGunAngle = Mathf.Abs(value);
        }

        /// <summary>
        /// Horizontal rotation velocity
        /// </summary>
        /// <value></value>
        public float RotSpeedHorizontal { get => rotSpeedHorizontal; set => rotSpeedHorizontal = value; }

        /// <summary>
        /// Vertical rotation velocity
        /// </summary>
        /// <value></value>
        public float RotSpeedVertical { get => rotSpeedVertical; set => rotSpeedVertical = value; }

        /// <summary>
        /// Cannon of turret
        /// </summary>
        /// <value></value>
        public Transform Gun { get => gun; set => gun = value; }

        /// <summary>
        /// Turret body
        /// </summary>
        /// <value></value>
        public Transform Turret { get => turret; set => turret = value; }

        /// <summary>
        /// Deactivate or activate the turret and cannon
        /// </summary>
        /// <value></value>
        public bool TurretEnabled { get => turretEnabled; set => turretEnabled = value; }

        /// <summary>
        /// Configure turret 
        /// </summary>
        /// <param name="vehicle"></param>
        public void SetupTurret(MMV_MBT_Vehicle vehicle)
        {
            this.vehicle = vehicle;
            this.turretEnabled = true;
        }

        /// <summary>
        /// Apply turret
        /// </summary>
        public void UpateTurret()
        {
            if (!turretEnabled)
            {
                return;
            }

            if (!turret || !gun)
            {
                return;
            }

            // look to target
            Quaternion _lookAtTurret = Quaternion.LookRotation(targetPosition - turret.position, vehicle.transform.up);
            Quaternion _lookAtGun = Quaternion.LookRotation(targetPosition - gun.position, gun.up);

            // find rotation (local space of the vehicle)
            Quaternion _turretRelativeRotTarget = Quaternion.Euler(vehicle.transform.eulerAngles - _lookAtTurret.eulerAngles);
            Quaternion _gunRelativeRotTarget = Quaternion.Euler(turret.eulerAngles - _lookAtGun.eulerAngles);

            // angle between turret/gun rotation and look_rotation
            float _angleBetweenTurretAndTarget = Vector3.Angle(turretLocalRotation * Vector3.forward, _turretRelativeRotTarget * Vector3.forward);
            float _angleBetweenGunAndTarget = Vector3.Angle(gunLocalRotation * Vector3.forward, _gunRelativeRotTarget * Vector3.forward);


            // rotation velocity


            float _turretVelocity = 1 / _angleBetweenTurretAndTarget;
            float _gunVelocity = 1 / _angleBetweenGunAndTarget;

            // horizontal speed
            var _horizontalSpeed = rotSpeedHorizontal;
            _horizontalSpeed *= _turretVelocity;

            // vertical speed
            var _verticalSpeed = rotSpeedVertical;
            _verticalSpeed *= _gunVelocity;


            // --- rotating turret/gun

            // target rotation 
            Quaternion _turretFinalRotation = Quaternion.Euler(vehicle.transform.eulerAngles - _lookAtTurret.eulerAngles);
            Quaternion _gunFinalRotation = Quaternion.Euler(turret.eulerAngles - _lookAtGun.eulerAngles);

            // rotating to target (local space of the vehicle)
            turretLocalRotation = Quaternion.Lerp(turretLocalRotation, _turretFinalRotation, _horizontalSpeed);
            gunLocalRotation = Quaternion.Lerp(gunLocalRotation, _gunFinalRotation, _verticalSpeed);

            // apply rotation on world space
            Quaternion _turretRot = Quaternion.Euler(vehicle.transform.eulerAngles - turretLocalRotation.eulerAngles);
            Quaternion _gunRot = Quaternion.Euler(turret.eulerAngles - gunLocalRotation.eulerAngles);


            // --- apply rotation

            gun.rotation = _gunRot;
            turret.rotation = _turretRot;


            // --- clamp vertical rotation

            Vector3 _newGunRotation = gun.localEulerAngles;
            Vector3 _newTurretRotation = turret.localEulerAngles;

            {
                float _max = 360 - maxGunAngle;
                float _min = minGunAngle;
                float _currentAngle = gun.localEulerAngles.x;

                if (_currentAngle > 180)
                {
                    if (_currentAngle < _max) _newGunRotation.x = _max;
                }
                else
                {
                    if (_currentAngle > _min) _newGunRotation.x = _min;
                }
            }


            _newTurretRotation.x = 0;
            _newTurretRotation.z = 0;

            _newGunRotation.y = 0;
            _newGunRotation.z = 0;

            // --- apply local rotation

            turret.localRotation = Quaternion.Euler(_newTurretRotation);
            gun.localRotation = Quaternion.Euler(_newGunRotation);
        }
    }
}
