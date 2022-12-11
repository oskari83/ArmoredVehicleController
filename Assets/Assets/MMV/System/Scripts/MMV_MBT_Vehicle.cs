using System;
using UnityEngine;
using UnityEngine.AI;

namespace MMV
{
    public class MMV_MBT_Vehicle : MonoBehaviour
    {
        [SerializeField] private Transform centerOfMass;

        [SerializeField] private MMV_TurretController turret;

        [SerializeField] private MMV_MBT_Engine engine;
        [SerializeField] private MMV_MBT_WheelManager wheels;

        private Rigidbody rb;

        /// <summary>
        /// Get vehicle rigidBody component
        /// </summary>
        public Rigidbody Rb => rb;

        /// <summary>
        /// </summary>
        /// <returns>
        /// get vehicle velocity in local space
        /// </returns>
        public Vector3 LocalVelocity => transform.InverseTransformDirection(rb.velocity);

        /// <summary>
        /// Controll vehicle acceleration
        /// </summary>
        /// <value></value>
        public float Acceleration => moveDirection.z;

        /// <summary>
        /// Controll vehicle rotation
        /// </summary>
        /// <value></value>
        public float Steering => moveDirection.x;

        /// <summary>
        /// Manage all wheels, applie physics and simulate tracks
        /// </summary>
        /// <value></value>
        public MMV_MBT_WheelManager Wheels { get => wheels; set => wheels = value; }

        /// <summary>
        /// Gun turret of vehicle
        /// </summary>
        /// <value></value>
        public MMV_TurretController Turret { get => turret; set => turret = value; }

        /// <summary>
        /// </summary>
        /// <value> Get center of mass
        /// </value>
        public Transform CenterOfMass { get => centerOfMass; set => centerOfMass = value; }

        /// <summary>
        /// Engine power
        /// </summary>
        public MMV_MBT_Engine Engine { get => engine; set => engine = value; }

        /// <summary>
        /// Applie brake on vehicle
        /// </summary>
        /// <value></value>
        public bool Brake { get => isBraking; set => isBraking = value; }

        /// <summary>
        /// Current left wheels acceleration
        /// </summary>
        public float CurrentLeftAcceleration => leftAcc;

        /// <summary>
        /// Current right wheels acceleration
        /// </summary>
        public float CurrentRightAcceleration => rightAcc;

        /// <summary>
        /// Current left wheels brake force
        /// </summary>
        public float CurrentLeftBrake => leftBrake;

        /// <summary>
        /// Current right wheels brake force
        /// </summary>
        public float CurrentRightBrake => rightBrake;

        /// <summary>
        /// Current gear
        /// </summary>
        public int CurrentGear => currentGear;

        /// <summary>
        /// If the vehicle controls are enabled
        /// </summary>
        public bool VehicleControlsEnabled => vehicleControlEnabled;

        //----------------------------------------------------


        // control the vehicle
        private Vector3 moveDirection;
        private bool isBraking;
        private bool isRotating;
        private bool vehicleControlEnabled;
        private bool brakeOnDisableVehicle;

        private int currentGear;

        private float leftAcc;
        private float rightAcc;
        private float leftBrake;
        private float rightBrake;

        private float lastVelocity; // calculate vehicle deceleration when colliding

        private void Start()
        {
            vehicleControlEnabled = true;

            SetupVehicle();

            TurretTargetPosition = transform.position + (transform.forward * 2); // look to forward when start the game
        }

        private void Update()
        {
            if (!vehicleControlEnabled)
            {
                moveDirection.z = 0;
                moveDirection.x = 0;
                isBraking = brakeOnDisableVehicle;
            }

            var _vertical = moveDirection.z;
            var _horizontal = moveDirection.x;

            // --- update vehicle data

            // each track has its own acceleration and brake force
            // with different forces it is possible to change the direction that the vehicle moves

            // direction of the wheels should accelerate
            engine.WheelsAccelerationDirection(out float leftInput, out float rightInput, _vertical, _horizontal);

            float _currentSpeed = LocalVelocity.z;
            float maxSpeed = engine.CurrentMaxVelocity();
            currentGear = engine.CurrentGear();

            // find acceleration and brake force of the wheels
            leftAcc = engine.CurrentAcceleration(leftInput, isBraking, leftAcc, leftBrake);
            rightAcc = engine.CurrentAcceleration(rightInput, isBraking, rightAcc, rightBrake);
            leftBrake = engine.CurrentBrakeForce(isBraking, leftInput, _currentSpeed);
            rightBrake = engine.CurrentBrakeForce(isBraking, rightInput, _currentSpeed);

            // apply engine sound
            float _engineAcceleration = (leftAcc + rightAcc) / 2;
            engine.EngineSound.UseEngineSound(_currentSpeed, _engineAcceleration, maxSpeed, engine.MaxAcceleration, currentGear);


            // update movement state 
            if ((_horizontal != 0 && _vertical == 0) != isRotating)
            {
                isRotating = _horizontal != 0 && _vertical == 0;

                // when you stop turning and accelerate forward, the acceleration force 
                // rises significantly, giving the vehicle a lot of momentum.
                // to get around this the acceleration is reset
                if (!isRotating)
                {
                    leftAcc = 0;
                    rightAcc = 0;
                }
            }
        }

        private void FixedUpdate()
        {
            // --- acceleration force based on current gear

            var _gear = Mathf.Abs(currentGear);

            if (_gear == 0)
            {
                _gear = 1;
            }

            var _maxVelocity = engine.CurrentMaxVelocity();
            var _accelerationLimit = 1.0f;

            // --- mediam velocity of all wheels
            var _leftVelocity = wheels.WheelsMovementVelocity(wheels.wheelsLeft, leftAcc).z;
            var _rightVelocity = wheels.WheelsMovementVelocity(wheels.wheelsRight, rightAcc).z;
            var _wheelsVelocity = (Mathf.Abs(_leftVelocity) + Mathf.Abs(_rightVelocity)) / 2;

            // if is turning stopped
            if (moveDirection.x != 0 && moveDirection.z == 0)
            {
                _wheelsVelocity = Mathf.Clamp(_wheelsVelocity, 0, engine.MaxRotationSpeed);
                _maxVelocity = engine.MaxRotationSpeed;

                _accelerationLimit = 1 - (_wheelsVelocity / _maxVelocity);
            }

            _accelerationLimit /= _gear * engine.LossOfStrength;

            // prevents you from exceeding maximum speed by limiting the acceleration
            if (Mathf.Abs(LocalVelocity.z) > engine.CurrentMaxVelocity())
            {
                _accelerationLimit = 0;
            }

            //--- decelerate the vehicle when it collides

            //cannot be done in Engine.CurrentAcceleration was calculated in the update.
            //To know the deceleration speed, it must be calculated in fixedUpdate

            var _maxAcceleration = engine.MaxAcceleration;
            var _desacelerationSpeed = lastVelocity - LocalVelocity.z;
            lastVelocity = LocalVelocity.z;
            var _desacelerationForce = (Mathf.Abs(_desacelerationSpeed) / engine.CurrentMaxVelocity()) * _maxAcceleration;

            leftAcc -= leftAcc >= 0 ? _desacelerationForce : -_desacelerationForce;
            rightAcc -= rightAcc >= 0 ? _desacelerationForce : -_desacelerationForce;

            //------------------------------------------


            // gravity effect on hills
            var _gravityForce = GravityInfluence();


            //------------------------------------------


            var _finalLeftAcc = leftAcc * _accelerationLimit;
            var _finalRightAcc = rightAcc * _accelerationLimit;

            _finalLeftAcc *= 1 - (leftBrake / engine.MaxBrakeForce);
            _finalRightAcc *= 1 - (rightBrake / engine.MaxBrakeForce);


            //------------------------------------------


            // on hills, the vehicle must brake if it does not have
            // enough acceleration to climp

            // --- get current angle
            var _currentAngle = transform.localEulerAngles.x;

            // convert 360 to -180 / 180
            if (_currentAngle > 180)
            {
                _currentAngle -= 360;
            }

            _currentAngle = Mathf.Clamp(_currentAngle, -90, 90); // angle limit 90°
            _currentAngle = _currentAngle / 90;       // convert (-90° / 90°) -> (-1 / 1) 

            // get acceleration force of all wheels together
            var _wheelsAccelerationForce = _finalLeftAcc * wheels.wheelsLeft.Length + _finalRightAcc * wheels.wheelsRight.Length;

            // convert gravity force to local space
            _gravityForce = transform.InverseTransformDirection(_gravityForce);

            var _finalLeftBrakeForce = leftBrake;
            var _finalRightBrakeForce = rightBrake;

            // braking 
            if (_wheelsAccelerationForce >= 0)
            {
                if (_gravityForce.magnitude > _wheelsAccelerationForce)
                {
                    _finalLeftBrakeForce = engine.MaxBrakeForce;
                    _finalRightBrakeForce = engine.MaxBrakeForce;
                }
            }
            else
            {
                if (_gravityForce.magnitude < _wheelsAccelerationForce)
                {
                    _finalLeftBrakeForce = engine.MaxBrakeForce;
                    _finalRightBrakeForce = engine.MaxBrakeForce;
                }
            }


            // --- Use vehicle physics / controll the vehicle 

            wheels.UseWheels(_finalLeftAcc, _finalRightAcc, _finalLeftBrakeForce, _finalRightBrakeForce);
            turret.UpateTurret();
        }

        /// <summary>
        /// Setup vehicle
        /// </summary>
        private void SetupVehicle()
        {
            rb = GetComponent<Rigidbody>();

            if (centerOfMass) rb.centerOfMass = centerOfMass.localPosition;
            else rb.centerOfMass = Vector3.zero;

            // --- create vehicle system

            wheels.SetupWheels(this);
            engine.SetupEngine(this);
            turret.SetupTurret(this);
        }

        /// <summary>
        /// Applies counterforce on climbs to simulate deceleration
        /// </summary>
        /// <return>
        /// the applied force
        /// <return>
        private Vector3 GravityInfluence()
        {
            var _force = Vector3.zero;
            float _currentAngle = transform.localEulerAngles.x;

            if (_currentAngle > 180) _currentAngle -= 360;          // convert 360 to -180 / 180
            _currentAngle = Mathf.Clamp(_currentAngle, -90, 90);    // angle limit 

            // force down
            float _gravity = (_currentAngle / 90) * engine.AngleDesaceleration;

            // avoids accelerating if the vehicle is heading towards a downhill
            if (Mathf.Abs(LocalVelocity.z) > engine.CurrentMaxVelocity())
            {
                _gravity = 0;
            }

            _force = transform.forward * _gravity;

            rb.AddForce(_force);
            return _force;
        }

        /// <summary>
        ///  Disable vehicle controls
        /// </summary>
        /// <param name="enabled">
        /// Whether to disable or enable controls
        /// </param>
        /// <param name="brake">
        /// Stop the vehicle when disabling the controls?
        /// </param>
        /// <returns>
        /// If vehicle controls is enabled
        /// </returns>
        public bool DisableVehicleControl(bool enabled, bool brake = false)
        {
            vehicleControlEnabled = enabled;
            brakeOnDisableVehicle = brake;
            return vehicleControlEnabled;
        }

        /// <summary>
        /// The direction in which the vehicle is moving
        /// </summary>
        /// <returns></returns>
        public Vector3 MoveDirection => transform.TransformDirection(new Vector3(moveDirection.x, 0, moveDirection.z));

        /// <summary>
        /// Tower target position to aim
        /// </summary>
        /// <value></value>
        public Vector3 TurretTargetPosition { get => turret.TargetPosition; set => turret.TargetPosition = value; }

        /// <summary>
        /// Pass player controls to vehicle
        /// </summary>
        /// <param name="vertical">
        /// how much the vehicle should accelerate forward (from -1 to 1)
        /// </param>
        /// <param name="horizontal">
        /// how much the vehicle should turn left or right (from -1 to 1)
        /// </param>
        /// <param name="isBraking">
        /// If the vehicle is braking
        /// </param>
        public void PlayerInputs(float vertical, float horizontal, bool isBraking)
        {
            vertical = Mathf.Clamp(vertical, -1, 1);
            horizontal = Mathf.Clamp(horizontal, -1, 1);

            moveDirection.z = vertical;
            moveDirection.x = horizontal;
            this.isBraking = isBraking;
        }

        /// <summary>
        /// Say the position in which the vehicle should go
        /// </summary>
        /// <param name="target">
        /// The position of your target you want to go
        /// </param>
        /// <param name="stopWithAngle">
        /// When the angle between the vehicle and the direction it should go 
        /// is greater than 'X' the vehicle stops, rotates and goes back to walking, 
        /// use this for very extreme turns
        /// </param>
        public void MoveTo(Vector3 target, float stopWithAngle = 45.0f)
        {
            var _moveDirection = target - transform.position;

            stopWithAngle = Mathf.Abs(stopWithAngle);
            stopWithAngle = Mathf.Clamp(stopWithAngle, 0, 360);

            var _angleToDirection = Vector3.Angle(transform.forward, _moveDirection.normalized);
            _angleToDirection = Mathf.Clamp(_angleToDirection, -stopWithAngle, stopWithAngle);

            //converts direction to target to local space, after converting direction looks like player control inputs
            _moveDirection = transform.InverseTransformDirection(_moveDirection);
            _moveDirection.y = 0;

            // Steering is poor when you approach the target(distance<1), as steering is the main input 
            // to accelerate the vehicle, if steering is poor, the vehicle will not accelerate in times 
            // of need.Divide the acceleration by itself and it will always have the maximum acceleration "dir.z = 1"
            if (_moveDirection.z != 0) _moveDirection.z = _moveDirection.z / Mathf.Abs(_moveDirection.z);

            // smoothes the steering so that the vehicle does not roll to the sides, the sudden movements 
            // will still be noticeable if the acceleration and acceleration speed is too high
            {
                // smooth driving over long distances
                _moveDirection.x /= _moveDirection.magnitude;

                // it increases the precision of the curves more
                _moveDirection.x /= Mathf.Clamp(stopWithAngle - Mathf.Abs(_angleToDirection), 1, stopWithAngle);

                // ignore very small curves (they hamper stability)
                if (Mathf.Abs(_moveDirection.x) < 0.01f) _moveDirection.x = 0;
            }

            _moveDirection.x = Mathf.Clamp(_moveDirection.x, -1, 1);
            _moveDirection.z = Mathf.Clamp(_moveDirection.z, -1, 1);

            if (stopWithAngle != 0.0f && stopWithAngle != 360.0f)
            {
                // if the target is not ahead (within an 'X' degree radius) the vehicle must stop and turn towards the target
                if (Mathf.Abs(_angleToDirection) >= stopWithAngle)
                {
                    _moveDirection.z = 0;
                    _moveDirection.x = _moveDirection.x > 0 ? 1 : -1; // rotate in max velocity
                }
            }

            moveDirection.z = _moveDirection.z;
            moveDirection.x = _moveDirection.x;
            moveDirection.y = 0;
        }

        /// <summary>
        /// Use a NavMeshAgent to guide your vehicle to a position, thus avoiding obstacles in a simple way
        /// </summary>
        /// <param name="agent">
        /// Your NavMeshAgent component to control the vehicle
        /// </param>
        /// <param name="target">
        /// The position in which to go
        /// </param>
        /// <param name="curveSensitive">
        /// The sensitivity of the brake in corners, the higher it is, the faster and harder the vehicle will brake
        /// </param>
        /// <param name="detectCurveInDistance">
        /// from this distance from the curve the vehicle starts to brake
        /// </param>
        /// <param name="stopWithAngle">
        /// When the angle between the vehicle and the direction in which it should go is greater than
        /// </param>
        /// <param name="minVelocity">
        /// The speed the vehicle walks when making turns at low speed
        /// </param>
        public void MoveTo(NavMeshAgent agent, Vector3 target, float curveSensitive = 20.0f, float detectCurveInDistance = 40.0f, float stopWithAngle = 45.0f, float minVelocity = 12.0f)
        {
            agent.destination = target;
            agent.speed = 0;

            var _path = agent.path.corners;

            // follows the main path to a target using the path provided by the agent
            MoveTo(agent.steeringTarget, stopWithAngle);

            // the new direction generated by following the path
            var _moveDirection = moveDirection;

            // --- Control the vehicle's speed on curves so you don't miss the path

            // within this distance to the curve, the deceleration calculations are made
            var distanceToDetectCurve = engine.CurrentMaxVelocity() + agent.radius;

            // 1 - itself; 2 - path_point_0; 3 - path_point_1; ...
            // if the path has more than 2 points it means that there may be a curve ahead, 
            // if there is a curve, speed control must be done
            if (_path.Length > 2)
            {
                if (Mathf.Abs(LocalVelocity.z) > minVelocity)
                {
                    var _curveStart = _path[1]; // steering target
                    var _curveEnd = _path[2];

                    _curveStart.y = 0;
                    _curveEnd.y = 0;

                    var _curveAngle = Vector3.Angle((_curveStart - transform.position).normalized, _path[2] - agent.steeringTarget); // 180 to 180

                    // convert curve angle to 0 - 1
                    _curveAngle /= 180;

                    _curveAngle *= curveSensitive;
                    _curveAngle = Mathf.Clamp(_curveAngle, 0, 1);

                    // we can consider a curve every point of the way
                    var _distanceOfCurve = Vector3.Distance(transform.position, _curveStart) - agent.radius;
                    // convert to 1 - 0
                    _distanceOfCurve = 1 - (Mathf.Clamp(_distanceOfCurve, 0, distanceToDetectCurve) / distanceToDetectCurve);

                    // the longer the curve and the closer the greater the brake force
                    _distanceOfCurve *= _curveAngle;

                    _moveDirection *= 1 - _distanceOfCurve;
                }
            }
            else
            {
                // limit speed when reaching final destination
                if (Mathf.Abs(LocalVelocity.z) > minVelocity)
                {
                    var _distanceOfTarget = Mathf.Clamp(Vector3.Distance(transform.position, agent.destination) - agent.radius, 0, distanceToDetectCurve);

                    // converting to scale from 0 to 1
                    _distanceOfTarget /= distanceToDetectCurve;

                    _moveDirection.z *= _distanceOfTarget;
                }
            }

            _moveDirection.x = Mathf.Clamp(_moveDirection.x, -1, 1);
            _moveDirection.z = Mathf.Clamp(_moveDirection.z, -1, 1);

            moveDirection.z = _moveDirection.z;
            moveDirection.x = _moveDirection.x;
            moveDirection.y = 0;

            agent.transform.position = transform.position;
        }
    }
}
