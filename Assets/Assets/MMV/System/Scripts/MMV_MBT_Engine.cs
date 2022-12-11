using System;
using UnityEngine;

namespace MMV
{
    [Serializable]
    public class MMV_MBT_Engine
    {
        [Serializable]
        public class SoundSystem
        {
            [SerializeField] private AudioSource audioPlayer;
            [SerializeField] private AudioClip engineSound;

            [SerializeField] private float basePitch;
            [SerializeField] private float maxForwardPitch;
            [SerializeField] private float maxBackwardPitch;

            //------------------------------------------------------

            // lerp velocity of change gear (used on engine sound)
            private const float TRANSMISSION_CHANGE_SPEED = 5.0f;

            // current pitch of engine sound
            private float currentSoundPitch;

            //------------------------------------------------------

            /// <summary>
            /// The AudioSource of the engine
            /// </summary>
            /// <value></value>
            public AudioSource AudioPlayer { get => audioPlayer; set => audioPlayer = value; }

            /// <summary>
            /// Min pitch of the engine sound
            /// </summary>
            /// <value></value>
            public float BasePitch { get => basePitch; set => basePitch = value; }

            /// <summary>
            /// Max engine sound pitch moving to forward
            /// </summary>
            /// <value></value>
            public float MaxForwardPitch { get => maxForwardPitch; set => maxForwardPitch = value; }

            /// <summary>
            /// Max engine sound pitch moving to backward
            /// </summary>
            /// <value></value>
            public float MaxBackwardPitch { get => maxBackwardPitch; set => maxBackwardPitch = value; }

            /// <summary>
            /// Sound of the engine
            /// </summary>
            /// <value></value>
            public AudioClip Sound { get => engineSound; set => engineSound = value; }

            /// <summary>
            /// Apply engine acceleration sound
            /// </summary>
            /// <param name="currentSpeed">
            /// Current speed of vehilce (in local space / Z axis)
            /// </param>
            /// <param name="currentAcc">
            /// Current acceleration of the engine
            /// </param>
            /// <param name="maxSpeed">
            /// Max move speed
            /// </param>
            /// <param name="maxAcc">
            /// Max engine acceleration
            /// </param>
            /// <param name="currentGear">
            /// Current gear 
            /// </param>
            public void UseEngineSound(float currentSpeed, float currentAcc, float maxSpeed, float maxAcc, int currentGear)
            {
                if (!audioPlayer)
                {
                    return;
                }

                var _maxPitch = currentSpeed > 0 ? maxForwardPitch : maxBackwardPitch;
                var _accelerationPitch = (Mathf.Abs(currentSpeed) / maxSpeed) * (_maxPitch - basePitch);

                currentGear = Mathf.Abs(currentGear);
                if (currentGear == 0) currentGear = 1;

                _accelerationPitch /= currentGear;
                _accelerationPitch += basePitch;

                // smooth gear shift
                currentSoundPitch = Mathf.Lerp(currentSoundPitch, _accelerationPitch, Time.deltaTime * TRANSMISSION_CHANGE_SPEED);

                audioPlayer.pitch = currentSoundPitch;

                if (!audioPlayer.isPlaying)
                {
                    audioPlayer.Play();
                }
            }
        }

        [Header("Velocity controll")]

        [SerializeField] private float acceleration;
        [SerializeField] private float maxAcceleration;
        [SerializeField] private float slowdown;

        [SerializeField] private float maxRotationSpeed;

        [SerializeField] private float maxForwardVelocity;
        [SerializeField] private float maxReverseVelocity;

        [SerializeField] private float maxBrakeForce;

        [SerializeField] private float angleDesaceleration;
        [SerializeField] private float lossOfStrength;

        [SerializeField] private SoundSystem engineSound;

        [SerializeField] private float[] forwardGears;
        [SerializeField] private float[] backwardGears;

        private MMV_MBT_Vehicle vehicle;

        //------------------------------------------------------

        // considering that: _lossOfStrength: = lossOfStrength * _currentGear
        // And considering that the acceleration is: _acc = acceleration* _lossStrenght
        // lossOfStrenght must not be less than 1 so that the acceleration does not exceed the limit
        // 0.55 is a recomended limit for lossOfStrenght
        public const float MAX_OF_LOSS_STRENGHT = 1.00f;
        public const float MIN_OF_LOSS_STRENGTH = 0.55f;

        //------------------------------------------------------

        /// <summary>
        /// Owner of the engine
        /// </summary>
        /// <value></value>
        public MMV_MBT_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        /// <summary>
        /// Acceleration velocity
        /// </summary>
        /// <value></value>
        public float Acceleration { get => acceleration; set => acceleration = value; }

        /// <summary>
        /// Power max of engine
        /// </summary>
        /// <value></value>
        public float MaxAcceleration { get => maxAcceleration; set => maxAcceleration = value; }

        /// <summary>
        /// Desaleration velocity
        /// </summary>
        /// <value></value>
        public float Slowdown { get => slowdown; set => slowdown = value; }

        /// <summary>
        /// Max move speed to forward
        /// </summary>
        /// <value></value>
        public float MaxForwardVelocity { get => maxForwardVelocity; set => maxForwardVelocity = value; }

        /// <summary>
        /// Max move speed to backward
        /// </summary>
        /// <value></value>
        public float MaxReverseVelocity { get => maxReverseVelocity; set => maxReverseVelocity = value; }

        /// <summary>
        /// Force of brake
        /// </summary>
        /// <value></value>
        public float MaxBrakeForce { get => maxBrakeForce; set => maxBrakeForce = value; }

        /// <summary>
        /// Contrary force applied to the vehicle in inclined places to simulate deceleration
        /// </summary>
        /// <value></value>
        public float AngleDesaceleration { get => angleDesaceleration; set => angleDesaceleration = value; }

        /// <summary>
        /// How much power the vehicle loses when changing gears
        /// </summary>
        /// <value></value>
        public float LossOfStrength
        {
            get => lossOfStrength;
            set => lossOfStrength = Mathf.Clamp(value, MIN_OF_LOSS_STRENGTH, MAX_OF_LOSS_STRENGHT);
        }

        /// <summary>
        /// Applie engine sound 
        /// </summary>
        /// <value></value>
        public SoundSystem EngineSound { get => engineSound; set => engineSound = value; }

        /// <summary>
        /// Limit speeds for gear shifting to forward
        /// </summary>
        /// <value></value>
        public float[] ForwardGears { get => forwardGears; set => forwardGears = value; }

        /// <summary>
        /// Limit speeds for gear shifting to backward
        /// </summary>
        /// <value></value>
        public float[] BackwardGears { get => backwardGears; set => backwardGears = value; }
        public float MaxRotationSpeed { get => maxRotationSpeed; set => maxRotationSpeed = value; }

        /// <summary>
        /// Setup engine on vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        public void SetupEngine(MMV_MBT_Vehicle vehicle)
        {
            this.vehicle = vehicle;

            if (engineSound.AudioPlayer)
            {
                if (!engineSound.AudioPlayer.loop) engineSound.AudioPlayer.loop = true;
                if (!engineSound.AudioPlayer.isPlaying) engineSound.AudioPlayer.Play();

                engineSound.AudioPlayer.clip = engineSound.Sound;
            }
        }

        /// <summary>
        /// Get current transmission
        /// </summary>
        /// <returns>
        /// current gear of the engine
        /// </returns>
        public int CurrentGear()
        {
            float _currentSpeed = vehicle.LocalVelocity.z;
            float _currentAcceleration = vehicle.Acceleration;

            // if is moving to forward (1) else (-1)
            int _gear = (_currentAcceleration >= 0 && _currentSpeed >= 0) ? 1 : -1;

            if (_currentSpeed >= 0)   // change transmission moving forward
            {
                for (int i = 1; i <= forwardGears.Length; i++)
                {
                    if (_currentSpeed > forwardGears[i - 1]) _gear = i + 1;

                }
            }
            else                    // change transmission moving backward
            {
                for (int i = 1; i <= backwardGears.Length; i++)
                {
                    if (_currentSpeed < -backwardGears[i - 1]) _gear = -i - 1;
                }
            }



            // if vehicle is stopped and not accelerating the gear is neutral
            if (Mathf.Abs(_currentSpeed) < 1 && _currentAcceleration == 0)
            {
                _gear = 0;
            }

            return _gear;
        }

        /// <summary>
        /// The velocity when the vehicle is moving to forward is different 
        /// of the velocity moving to backward
        /// </summary>
        /// <returns>
        /// When moving to forward return the max velocity to forward, 
        /// else, return max velocity of move to backward
        /// </returns>
        public float CurrentMaxVelocity()
        {
            return vehicle.LocalVelocity.z >= 0 ? maxForwardVelocity : maxReverseVelocity;
        }

        /// <summary>
        /// Get current acceleration of the engine
        /// </summary>
        /// <param name="accInput">
        /// How much should accelerate (-1 to 1)
        /// </param>
        /// <param name="isBraking">
        /// If vehicle is braking
        /// </param>
        /// <param name="currentAcc">
        /// Current engine acceleration
        /// </param>
        /// <param name="currentBrakeForce">
        /// Current brake force
        /// </param>
        /// <returns>
        /// Acceleration of the engine 
        /// </returns>
        public float CurrentAcceleration(float accInput, bool isBraking, float currentAcc, float currentBrakeForce)
        {
            accInput = Mathf.Clamp(accInput, -1, 1);

            // ---------------------------------------------

            float _delta = Time.deltaTime;

            // how much acceleration should be
            float _target = accInput * maxAcceleration;

            //if the current acceleration is less than the desired one must accelerate, if not decelerate
            float _accVelocity = Mathf.Abs(accInput) * acceleration * _delta;
            currentAcc += currentAcc < _target ? _accVelocity : -_accVelocity;

            // if the acceleration input is 0 or the vehicle is reversing 
            // the acceleration direction the vehicle must decelerate
            if (accInput == 0 ||
                currentAcc < 0 && _target > 0 ||
                currentAcc > 0 && _target < 0)
            {
                currentAcc -= currentAcc * _delta;
            }

            return currentAcc;
        }

        /// <summary>
        /// Get current brake force
        /// </summary>
        /// <param name="isBraking">
        /// If the vehicle is braking
        /// </param>
        /// <param name="accelerationInput">
        /// Input value in range -1 / 1
        /// </param>
        /// <param name="currentVelocity">
        /// Current vehicle velocity in local space (z axis)
        /// </param>
        /// <returns>
        /// Brake force
        /// </returns>
        public float CurrentBrakeForce(bool isBraking, float accelerationInput, float currentVelocity)
        {
            // when less accelerates, more applies braking force
            float _brakeForce = (1 - Mathf.Abs(accelerationInput)) * vehicle.Engine.maxBrakeForce;

            // when reverting the acceleration the vehicle must brake so that it decelerates faster
            if (accelerationInput > 0 && currentVelocity < -0.5f ||
                accelerationInput < 0 && currentVelocity > 0.5f)
            {
                _brakeForce = slowdown;
            }

            // if not accelerate, vehicle must stop
            if (accelerationInput == 0)
            {
                _brakeForce = slowdown;
            }

            // brake
            if (isBraking)
            {
                _brakeForce = maxBrakeForce;
            }

            return _brakeForce;
        }

        /// <summary>
        /// the direction in which the wheels should accelerate
        /// </summary>
        /// <param name="left">
        /// Direction in which the left wheels will accelerate
        /// </param>
        /// <param name="right">
        /// Direction in which the right wheels will accelerate
        /// </param>
        /// <param name="vertical">
        /// the current acceleration
        /// </param>
        /// <param name="horizontal">
        /// direction it will turn
        /// </param>
        public void WheelsAccelerationDirection(out float left, out float right, float vertical, float horizontal)
        {
            left = vertical;
            right = vertical;

            if (vertical == 0)
            {
                if (horizontal > 0)
                {
                    left = Mathf.Abs(horizontal);
                    right = -Mathf.Abs(horizontal);
                }
                if (horizontal < 0)
                {
                    left = -Mathf.Abs(horizontal);
                    right = Mathf.Abs(horizontal);
                }
            }
            else
            {
                if (horizontal > 0)
                {
                    left *= 1.0f;
                    right *= 0.5f;
                }

                else if (horizontal < 0)
                {
                    left *= 0.5f;
                    right *= 1.0f;
                }
            }
        }
    }
}