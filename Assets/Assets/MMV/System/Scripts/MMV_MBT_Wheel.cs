using System;
using UnityEngine;

namespace MMV
{
    [Serializable]
    public class MMV_MBT_Wheel
    {
        [SerializeField] private Transform collider;
        [SerializeField] private Transform bone;
        [SerializeField] private Transform mesh;

        [NonSerialized] public MMV_MBT_Vehicle vehicle;
        [NonSerialized] public MMV_MBT_WheelManager wheelManager;

        //-------------------------------------------------------------------

        //-------------------------------------------------------------------

        private Rigidbody rb;
        private RaycastHit wheelHit;

        //-------------------------------------------------------------------

        private float lastSpringLenght;
        private float currentSpringLenght;

        private Vector3 offsetBone; // distance of bone relative to wheel

        private Vector3 wheelMoveSpeed;

        //-------------------------------------------------------------------

        /// <summary>
        /// Force applied on spring
        /// </summary>
        /// <returns></returns>
        public float CurrentSpringForce => wheelManager.SpringStiffness * (vehicle.Wheels.SpringLenght - currentSpringLenght);

        /// <summary>
        /// Speed spring is compressed by weight
        /// </summary>
        /// <returns></returns>
        public float SpringCompressVelocity => (lastSpringLenght - currentSpringLenght) / Time.fixedDeltaTime;

        /// <summary>
        /// Get wheel moviment velocity in local space
        /// </summary>
        /// <returns></returns>
        public Vector3 WheelMoveSpeed => wheelMoveSpeed;

        /// <summary>
        /// Add up with spring force for smoother suspension
        /// </summary>
        /// <value>
        /// smooth force
        /// </value>
        public float CurrentDamperForce => wheelManager.SpringDamper * SpringCompressVelocity;

        /// <summary>
        /// Suspend vehicle on air, simulate suspension physics
        /// </summary>
        /// <value>
        /// Suspension force
        /// </value>
        public float CurrentSuspensionForce => CurrentSpringForce + CurrentDamperForce;

        /// <summary>
        /// Check if vehicle is on ground
        /// </summary>
        /// <value></value>
        public bool OnGronded => wheelHit.transform != null;

        /// <summary>
        /// Get ground info
        /// </summary>
        /// <value></value>
        public RaycastHit WheelHit => wheelHit;

        /// <summary>
        /// Wheel position (as Empty game object) 
        /// </summary>
        /// <value></value>
        public Transform Collider => collider;

        /// <summary>
        /// Bone of wheel for simulate track suspension
        /// </summary>
        /// <value></value>
        public Transform Bone => bone;

        /// <summary>
        /// MeshRenderer of wheel
        /// </summary>
        /// <value></value>
        public Transform Mesh => mesh;

        /// <summary>
        /// Create wheel
        /// </summary>
        /// <param name="owner">
        /// Wheel owner vehicle
        /// </param>
        public void SetupWheel(MMV_MBT_Vehicle owner)
        {
            vehicle = owner;
            rb = vehicle.Rb;
            wheelManager = vehicle.Wheels;

            if (bone)
            {
                offsetBone = bone.localPosition - collider.localPosition;
            }
        }

        /// <summary>
        /// Use wheel and controll vehicle
        /// </summary>
        /// <param name="accelerationInput">
        /// Vertical input clamped -1 to 1
        /// <param name="brakeInput">
        /// Brake input force clamped -1 to 1
        /// /<param>
        public void UseWheel(float accelerationInput, float brakeInput)
        {
            if (!collider)
            {
                return;
            }

            Vector3 _localVelocity = vehicle.LocalVelocity;
            Vector3 _springPosition = Collider.position + (vehicle.transform.up * wheelManager.SpringHeight);   // when start raycast of suspension

            // springHeight is added so that the initial position of the suspension 
            // raycast does not get out of the vehicle collider, just set the value.

            var _springLenght = vehicle.Wheels.SpringLenght;
            var _wheelRadius = vehicle.Wheels.WheelRadius;

            if (Physics.Raycast(_springPosition, -vehicle.transform.up, out wheelHit, _springLenght + _wheelRadius + wheelManager.SpringHeight))
            {
                //---get current spring lenght

                lastSpringLenght = currentSpringLenght;
                currentSpringLenght = wheelHit.distance - _wheelRadius - wheelManager.SpringHeight;

                // prevents the distance from the ground from being negative

                if (currentSpringLenght < 0)
                {
                    currentSpringLenght = 0;
                }

                //---get suspension force

                float _springForce = CurrentSpringForce;
                float _damperForce = wheelManager.SpringDamper * SpringCompressVelocity;
                float _suspensionForce = _springForce + _damperForce;

                //---get wheel friction

                var _wheelForwardFriction = wheelManager.ForwardFriction;
                var _wheelSideFriction = wheelManager.SideFriction;

                float _sideFriction = Mathf.Clamp(WheelMoveSpeed.x, -_wheelSideFriction, _wheelSideFriction);
                float _forwardFriction = Mathf.Clamp(WheelMoveSpeed.z, -_wheelForwardFriction, _wheelForwardFriction);

                float _sideStiffness = _springForce * _sideFriction;
                float _longitudinalStiffness = _springForce * _forwardFriction;

                Vector3 _upForce = Vector3.up * CurrentSuspensionForce;
                Vector3 _sideForce = -vehicle.transform.right * _sideStiffness;
                Vector3 _forwardForce = -vehicle.transform.forward * _longitudinalStiffness;

                Vector3 _forwardAcc = vehicle.transform.forward * accelerationInput;
                Vector3 _forwardBrake = vehicle.transform.forward * brakeInput * Mathf.Clamp(-WheelMoveSpeed.z, -1, 1);
                Vector3 _directionForce = _forwardAcc + _forwardBrake;

                Vector3 _totalForce = _upForce + _sideForce + _forwardForce + _directionForce;

                rb.AddForceAtPosition(_totalForce, wheelHit.point);


                // --- apply position in bone and mesh

                Vector3 _wheelPos = Collider.position + (-vehicle.transform.up * (wheelHit.distance - wheelManager.SpringHeight - _wheelRadius));

                if (bone)
                {
                    bone.position = wheelHit.point;
                }

                if (mesh)
                {
                    mesh.position = _wheelPos;
                }
            }

            else
            {
                if (bone && mesh)
                {
                    Vector3 _wheelPos = Collider.position + (-vehicle.transform.up * vehicle.Wheels.SpringLenght);
                    bone.position = _wheelPos;
                    bone.localPosition += offsetBone;
                    Mesh.position = _wheelPos;
                }
            }

            wheelMoveSpeed = CurrentWheelMoveSpeed(wheelMoveSpeed);
        }

        private Vector3 CurrentWheelMoveSpeed(Vector3 current)
        {
            Vector3 _velocity = current;

            if (OnGronded)
            {
                _velocity = vehicle.transform.InverseTransformDirection(rb.GetPointVelocity(wheelHit.point));
            }
            else
            {
                _velocity -= _velocity * Time.deltaTime;
            }
            return new Vector3(_velocity.x, 0, _velocity.z);
        }
    }
}
