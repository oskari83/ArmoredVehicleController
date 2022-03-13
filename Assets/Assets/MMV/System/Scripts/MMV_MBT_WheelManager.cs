using System;
using UnityEngine;

namespace MMV
{
    [Serializable]
    public class MMV_MBT_WheelManager
    {
        /// <summary>
        /// Emits particles when the vehicle is in motion
        /// </summary>
        [Serializable]
        public class TrackParticleEmission
        {
            [SerializeField] private float maxEmission;
            [SerializeField] private float stopDelay;

            [SerializeField] private ParticleSystem leftParticle;
            [SerializeField] private ParticleSystem rightParticle;

            private ParticleSystem.EmissionModule leftEmissionModule;
            private ParticleSystem.EmissionModule rightEmissionModule;

            public const float MAX_STOP_DELAY = 10.0f;
            public const float MIN_STOP_DELAY = 0.0f;

            private float currentEmissionLeft;
            private float currentEmissionRight;

            /// <summary>
            /// Set max emission of the particles.
            /// emission = velocity * maxEmission.
            /// </summary>
            /// <value></value>
            public float MaxEmission { get => maxEmission; set => maxEmission = value; }

            /// <summary>
            /// time it takes for the treadmill to stop emitting particular when leaving the floor.
            /// </summary>
            /// <value></value>
            public float StopDelay
            {
                get => stopDelay;
                set => stopDelay = Mathf.Clamp(value, MIN_STOP_DELAY, MAX_STOP_DELAY);
            }
            /// <summary>
            /// Left track particle
            /// </summary>
            /// <value></value>
            public ParticleSystem LeftParticle { get => leftParticle; set => leftParticle = value; }

            /// <summary>
            /// RIght track particle
            /// </summary>
            /// <value></value>
            public ParticleSystem RightParticle { get => rightParticle; set => rightParticle = value; }

            /// <summary>
            /// Set up particle system
            /// </summary>
            public void SetupParticle()
            {
                if (!leftParticle || !rightParticle)
                {
                    return;
                }

                ParticleSystem.MainModule leftMainModule = leftParticle.main;
                ParticleSystem.MainModule rightMainModule = rightParticle.main;

                leftMainModule.loop = true;
                rightMainModule.loop = true;

                leftEmissionModule = leftParticle.emission;
                rightEmissionModule = rightParticle.emission;
            }

            /// <summary>
            /// Emits particles when the vehicle is in motion
            /// </summary>
            /// <param name="leftVelocity">
            /// Velocity of the left track
            /// </param>
            /// <param name="rightVelocity">
            /// Velocity of the right track
            /// <param name="leftOnGround">
            /// If the left track is on the floor
            /// </param>
            /// <param name="rightOnGround">
            /// If the right track is on the floor
            /// </param>
            public void UseParticles(float leftVelocity, float rightVelocity, bool leftOnGround, bool rightOnGround)
            {
                if (!leftParticle || !rightParticle)
                {
                    return;
                }

                currentEmissionLeft = Emission(leftOnGround, leftVelocity, currentEmissionLeft);
                currentEmissionRight = Emission(rightOnGround, rightVelocity, currentEmissionRight);

                leftEmissionModule.rateOverTime = currentEmissionLeft;
                rightEmissionModule.rateOverTime = currentEmissionRight;

                float Emission(bool onGrounded, float velocity, float current)
                {
                    float _emission = Mathf.Abs(velocity) * maxEmission;

                    if (onGrounded)
                    {
                        current = _emission;
                    }
                    else // if the mat is not on the floor it does not emit particles
                    {
                        current = Mathf.Lerp(current, 0, (MAX_STOP_DELAY - stopDelay) * Time.deltaTime);
                    }

                    return current;
                }
            }
        }

        [SerializeField] private float wheelRadius;
        [SerializeField] private float springLenght;

        [SerializeField] private int springStiffness;
        [SerializeField] private int springDamper;
        [SerializeField] private float springHeight;

        [SerializeField] private float forwardFriction;
        [SerializeField] private float sideFriction;
        [SerializeField] private float multiplyFriction;

        [SerializeField] private float trackMoveSpeed;

        [SerializeField] private SkinnedMeshRenderer leftTrack;
        [SerializeField] private SkinnedMeshRenderer rightTrack;

        [SerializeField] private TrackParticleEmission tracksParticles;

        [SerializeField] private Transform[] leftAdditionalWheelsRenderers;
        [SerializeField] private Transform[] rightAdditionalWheelsRenderers;

        [SerializeField] public MMV_MBT_Wheel[] wheelsLeft;
        [SerializeField] public MMV_MBT_Wheel[] wheelsRight;

        // material of tracks (used for make moviment effect)
        [NonSerialized] private Material leftTrackMaterial;
        [NonSerialized] private Material rightTrackMaterial;

        // owner of wheels
        private MMV_MBT_Vehicle vehicle;

        //-------------------------------------------------------

        /// <summary>
        /// Radius of all wheels
        /// </summary>
        /// <value></value>
        public float WheelRadius { get => wheelRadius; set => wheelRadius = value; }

        /// <summary>
        /// Suspension height
        /// </summary>
        /// <value></value>
        public float SpringLenght { get => springLenght; set => springLenght = value; }

        /// <summary>
        /// Resistense of spring
        /// </summary>
        public int SpringStiffness { get => springStiffness; set => springStiffness = value; }

        /// <summary>
        /// Suspension smooth force
        /// </summary>
        public int SpringDamper { get => springDamper; set => springDamper = value; }

        /// <summary>
        /// Start spring height
        /// </summary>
        public float SpringHeight { get => springHeight; set => springHeight = value; }

        /// <summary>
        /// Forward friction of wheels
        /// </summary>
        public float ForwardFriction { get => forwardFriction; set => forwardFriction = value; }

        /// <summary>
        /// Side friction of wheels
        /// </summary>
        public float SideFriction { get => sideFriction; set => sideFriction = value; }

        /// <summary>
        /// Mat movement velocity
        /// </summary>
        public float TrackMoveSpeed { get => trackMoveSpeed; set => trackMoveSpeed = value; }

        /// <summary>
        /// Left mat renderer
        /// </summary>
        public SkinnedMeshRenderer LeftTrack { get => leftTrack; set => leftTrack = value; }

        /// <summary>
        /// Right mat renderer
        /// </summary>
        public SkinnedMeshRenderer RightTrack { get => rightTrack; set => rightTrack = value; }

        /// <summary>
        /// Use for implement diferent types of friction
        /// </summary>
        /// <value></value>
        public float MultiplyFriction { get => multiplyFriction; set => multiplyFriction = value; }

        /// <summary>
        /// GGet additional wheel meshes transformations on the left side
        /// </summary>
        public Transform[] AdditionalWheelMeshLeft => leftAdditionalWheelsRenderers;

        /// <summary>
        /// Get additional wheel meshes transformations on the right side
        /// </summary>
        public Transform[] AdditionalWheelMeshRight => rightAdditionalWheelsRenderers;

        /// <summary>
        /// Material of the left track 
        /// </summary>
        /// <value></value>
        public Material LeftTrackMaterial => leftTrackMaterial;

        /// <summary>
        /// Material of the right track 
        /// </summary>
        /// <value></value>
        public Material RightTrackMaterial => rightTrackMaterial;

        /// <summary>
        /// emission of particles during the movement of the vehicle
        /// </summary>
        /// <value></value>
        public TrackParticleEmission TracksParticles { get => tracksParticles; set => tracksParticles = value; }

        /// <summary>
        /// Generate wheels configurations
        /// </summary>
        /// <param name="vehicle">
        /// Owner of wheels
        /// </param>
        public void SetupWheels(MMV_MBT_Vehicle vehicle)
        {
            this.vehicle = vehicle;

            // getting material of tracks
            if (leftTrack) leftTrackMaterial = leftTrack.GetComponent<SkinnedMeshRenderer>().material;
            if (rightTrack) rightTrackMaterial = rightTrack.GetComponent<SkinnedMeshRenderer>().material;

            foreach (var w in wheelsLeft) Apply(w);
            foreach (var w in wheelsRight) Apply(w);

            tracksParticles.SetupParticle();


            //---------------------------------------------

            void Apply(MMV_MBT_Wheel w)
            {
                w.SetupWheel(vehicle);
                w.vehicle = vehicle;
            }
        }

        /// <summary>
        /// Apply wheel physics and controll the vehicle.
        /// Each side has its own acceleration and brake force, 
        /// changing these parameters it is possible to control 
        /// the direction in which the vehicle goes
        /// </summary>
        /// <param name="leftAcceleration">
        /// Left track acceleration
        /// </param>
        /// <param name="rightAcceleration">
        /// Right track acceleration
        /// </param>
        /// <param name="leftBrakeForce">
        /// left track brake force
        /// </param>
        /// <param name="rightBrakeForce">
        /// Right track brake force
        /// </param>
        public void UseWheels(float leftAcceleration, float rightAcceleration, float leftBrakeForce, float rightBrakeForce)
        {
            var _currentVelocity = vehicle.LocalVelocity.z;

            var _leftVelocity = WheelsMovementVelocity(wheelsLeft, leftAcceleration).z;
            var _rightVelocity = WheelsMovementVelocity(wheelsRight, rightAcceleration).z;

            var _leftWheelRot = (Vector3.right * _leftVelocity);
            var _rightWheelRot = (Vector3.right * _rightVelocity);

            // Apply wheel
            foreach (var w in wheelsLeft)
            {
                Apply(w, _leftWheelRot / wheelRadius, leftAcceleration, leftBrakeForce);
            }

            foreach (var w in wheelsRight)
            {
                Apply(w, _rightWheelRot / wheelRadius, rightAcceleration, rightBrakeForce);
            }

            // apply rotation on additional wheel meshs
            foreach (var w in leftAdditionalWheelsRenderers) w.Rotate(_leftWheelRot);
            foreach (var w in rightAdditionalWheelsRenderers) w.Rotate(_rightWheelRot);

            // applie rotation on track
            if (leftTrackMaterial) leftTrackMaterial.mainTextureOffset += new Vector2(_leftWheelRot.z, _leftWheelRot.x) * trackMoveSpeed * Time.fixedDeltaTime;
            if (rightTrackMaterial) rightTrackMaterial.mainTextureOffset += new Vector2(_rightWheelRot.z, _rightWheelRot.x) * trackMoveSpeed * Time.fixedDeltaTime;

            // --- Applie particle system

            bool _leftWheelsOnGround = OnGrounded(wheelsLeft);
            bool _rightWheelsOnGround = OnGrounded(wheelsRight);

            tracksParticles.UseParticles(Mathf.Abs(_leftVelocity), Mathf.Abs(_rightVelocity), _leftWheelsOnGround, _rightWheelsOnGround);


            //----------------------------------------------------------------------

            // Applie wheel acceleration and set mesh transfromation
            void Apply(MMV_MBT_Wheel w, Vector3 rot, float acc, float brakeForce)
            {
                w.UseWheel(acc, brakeForce);

                if (w.Mesh)
                {
                    w.Mesh.Rotate(rot);
                }
            }
        }

        /// <summary>
        /// Get median velocity of wheels
        /// </summary>
        /// <param name="wheels">
        /// Wheels of some side
        /// </param>
        /// <param name="currentAcceleration">
        /// The acceleration of these wheels
        /// </param>
        /// <returns>
        /// Median velocity in local space
        /// </returns>
        public Vector3 WheelsMovementVelocity(MMV_MBT_Wheel[] wheels, float currentAcceleration)
        {
            if (wheels.Length == 0) return Vector3.zero;

            var _higherVelocity = wheels[0].WheelMoveSpeed;
            
            foreach (var w in wheels)
            {
                if (Mathf.Abs(w.WheelMoveSpeed.z) > Mathf.Abs(_higherVelocity.z))
                {
                    _higherVelocity = w.WheelMoveSpeed;
                }
            }

            return _higherVelocity;
        }

        /// <summary>
        /// Check if the all wheels is on ground
        /// </summary>
        /// <param name="wheels">
        /// Wheels to check
        /// </param>
        /// <returns></returns>
        private bool OnGrounded(MMV_MBT_Wheel[] wheels)
        {
            foreach (var w in wheels)
            {
                if (!w.OnGronded) return false;
            }

            return true;
        }
    }
}
