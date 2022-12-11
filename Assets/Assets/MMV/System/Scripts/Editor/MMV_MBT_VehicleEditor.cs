using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_MBT_Vehicle))]
    public class MMV_MBT_VehicleEditor : UnityEditor.Editor
    {
        //--- properties

        private const string PROPERTY_NAME_WHEELS_LEFT = "wheels.wheelsLeft";
        private const string PROPERTY_NAME_WHEELS_RIGHT = "wheels.wheelsRight";
        private const string PROPERTY_NAME_ADDITIONAL_WHEELS_LEFT = "wheels.leftAdditionalWheelsRenderers";
        private const string PROPERTY_NAME_ADDITIONAL_WHEELS_RIGHT = "wheels.rightAdditionalWheelsRenderers";

        //--- vehicle editor constants

        private const float ENGINE_MIN_ACCELERATION_VELOCITY = 10.0f;
        private const float ENGINE_MAX_ACCELERATION_FORCE = 10000.0f;
        private const float ENGINE_MIN_ACCELERATION_FORCE = 100.0f;
        private const float ENGINE_MIN_SPEED = 10.0f;
        private const float ENGINE_MAX_SPEED = 180.0f;
        private const float ENGINE_MIN_ROTATION_VELOCITY = 1.0f;
        private const float ENGINE_MAX_BRAKE_FORCE = 10000.0f;
        private const float ENGINE_MIN_BRAKE_FORCE = 1.0f;
        private const float ENGINE_MIN_SOUND_PITCH = 0.1f;
        private const float ENGINE_MAX_SOUND_PITCH = 10.0f;

        private const int GEARS_MIN_AMOUNT = 1;
        private const int GEARS_MAX_AMOUNT_FORWARD = 10;

        private const float TURRET_MIN_ROTATION_VELOCITY = 0.0f;
        private const float TURRET_MAX_ROTATION_VELOCITY = 5.0f;

        private const int GUN_MIN_ANGLE = -45;
        private const int GUN_MAX_ANGLE = 45;

        private const float WHEELS_MIN_RADIUS = 0.1f;
        private const float WHEELS_MAX_RADIUS = 5.0f;
        private const float WHEELS_MIN_SPRING_LENGHT = 0.05f;
        private const float WHEELS_MAX_SPRING_LENGHT = 5.0f;
        private const float WHEELS_MIN_SPRING_HEIGHT = 0.0f;
        private const float WHEELS_MAX_SPRING_HEIGHT = 1.0f;
        private const int WHEELS_MIN_SPRING_STIFFNESS = 100;
        private const int WHEELS_MAX_SPRING_STIFFNESS = 100000;
        private const int WHEELS_MIN_SPRING_DAMPER = 100;
        private const int WHEELS_MAX_SPRING_DAMPER = 100000;
        private const float WHEELS_MIN_FRICTION = 0.2f;
        private const float WHEELS_MAX_FRICTION = 5.0f;
        private const int WHEELS_MIN_PARTICLES_EMISSION = 1;
        private const int WHEELS_MAX_PARTICLES_EMISSION = 10;
        private const float WHEELS_MIN_PARTICLES_STOP_DELAY = 0;
        private const float WHEELS_MAX_PARTICLES_STOP_DELAY = 10;

        private const float STABILITY_MIN_SLOPE_DECELERATION = 1.0f;
        private const float STABILITY_MAX_SLOPE_DECELERATION = 100000.0f;

        private const float RIGID_BODY_DEFAULT_MASS = 1;
        private const float VEHICLE_DEFAULT_MASS = 1000;


        // --- handles colors

        private Color suspensionColor = Color.green;
        private Color springPosColor = Color.yellow;
        private Color wheelCircleColor = Color.green;

        // --- wheels lists

        private ReorderableList leftWheelsList;
        private ReorderableList rightWheelsList;

        //--- editor variables

        private int currentEditorTab;

        private bool engineSoundExpanded;
        private bool engineAccelerationExpanded;
        private bool engineBrakeExpanded;
        private bool engineGearsExpanded;

        private bool wheelSettingsExpanded;
        private bool wheelsLeftWheelsExpanded;
        private bool wheelsRightWheelsExpanded;
        private bool wheelsTracksExpanded;
        private bool wheelsParticlesExpanded;

        //--- properties

        private SerializedProperty leftWheels;
        private SerializedProperty rightWheels;
        private SerializedProperty leftAdditionalWheels;
        private SerializedProperty rightAdditionalWheels;

        //-------------------------------------

        private MMV_MBT_Vehicle vehicle;

        private void OnEnable()
        {
            vehicle = (MMV_MBT_Vehicle)target;

            // configure vehicle on add this component 
            {
                {
                    var _rb = vehicle.GetComponentInChildren<Rigidbody>();

                    if (!_rb)
                    {
                        _rb = vehicle.gameObject.AddComponent<Rigidbody>();
                    }

                    // --- configuring dependencies

                    if (_rb.mass == RIGID_BODY_DEFAULT_MASS)
                    {
                        _rb.mass = VEHICLE_DEFAULT_MASS;
                    }
                }

                // configure new engine
                if (vehicle.Engine == null)
                {
                    var _engine = new MMV_MBT_Engine();

                    _engine.Acceleration = 600;
                    _engine.MaxAcceleration = 1000;
                    _engine.MaxForwardVelocity = 20;
                    _engine.MaxReverseVelocity = 10;
                    _engine.MaxBrakeForce = 1500;
                    _engine.Slowdown = 500;

                    _engine.ForwardGears = new float[] { 5, 10, 15 };
                    _engine.BackwardGears = new float[] { 5 };
                    _engine.LossOfStrength = MMV_MBT_Engine.MAX_OF_LOSS_STRENGHT;

                    _engine.MaxRotationSpeed = 3.0f;

                    _engine.AngleDesaceleration = 400;

                    _engine.EngineSound = new MMV_MBT_Engine.SoundSystem();

                    _engine.EngineSound.BasePitch = 0.8f;
                    _engine.EngineSound.MaxForwardPitch = 6.0f;
                    _engine.EngineSound.MaxBackwardPitch = 2.5f;

                    vehicle.Engine = _engine;
                }

                // configure wheels
                if (vehicle.Wheels == null)
                {
                    var _wheels = new MMV_MBT_WheelManager();

                    _wheels.SpringHeight = 0.2f;
                    _wheels.SpringLenght = 0.5f;
                    _wheels.WheelRadius = 0.5f;
                    _wheels.SpringStiffness = 30000;
                    _wheels.SpringDamper = 1300;

                    _wheels.ForwardFriction = 0.2f;
                    _wheels.SideFriction = 2.0f;
                    _wheels.MultiplyFriction = 1.0f;

                    _wheels.TrackMoveSpeed = 1.0f;
                    _wheels.wheelsLeft = new MMV_MBT_Wheel[] { };
                    _wheels.wheelsRight = new MMV_MBT_Wheel[] { };

                    vehicle.Wheels = _wheels;
                }

                // configure turret 
                if (vehicle.Turret == null)
                {
                    var _turret = new MMV_TurretController();

                    _turret.MaxGunAngle = 20;
                    _turret.MinGunAngle = -5;

                    _turret.RotSpeedHorizontal = 0.5f;
                    _turret.RotSpeedVertical = 0.5f;

                    vehicle.Turret = _turret;
                }
            }


            // --- load data
            LoadEditorState();

            // --- find properties

            leftWheels = serializedObject.FindProperty(PROPERTY_NAME_WHEELS_LEFT);
            rightWheels = serializedObject.FindProperty(PROPERTY_NAME_WHEELS_RIGHT);
            leftAdditionalWheels = serializedObject.FindProperty(PROPERTY_NAME_ADDITIONAL_WHEELS_LEFT);
            rightAdditionalWheels = serializedObject.FindProperty(PROPERTY_NAME_ADDITIONAL_WHEELS_RIGHT);

            // --- create wheels lists editor
            leftWheelsList = new ReorderableList(serializedObject, leftWheels, true, true, true, true);
            rightWheelsList = new ReorderableList(serializedObject, rightWheels, true, true, true, true);

            leftWheelsList = DrawWheelsList(leftWheelsList, "left wheels");
            rightWheelsList = DrawWheelsList(rightWheelsList, "right wheels");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // tabs selector 
            EditorGUI.BeginChangeCheck();
            {
                currentEditorTab = GUILayout.Toolbar(currentEditorTab, new string[] { "engine", "turret", "wheels", "stability" });
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
            }


            //--- drawind editors

            if (currentEditorTab == 0)
            {
                Acceleration();
                ShowGearSystem();
                EngineSound();
            }
            else if (currentEditorTab == 1)
            {
                ShowTurretTab();
            }
            else if (currentEditorTab == 2)
            {
                ShowWheelsTab();
            }
            else if (currentEditorTab == 3)
            {
                ShowStabilityTab();
            }

            // apply all modifications
            SaveEditorState();
            EditorUtility.SetDirty(vehicle);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            DrawWheelsHandles(vehicle.Wheels.wheelsLeft);
            DrawWheelsHandles(vehicle.Wheels.wheelsRight);
        }

        /// <summary>
        /// Show engine acceleration editor
        /// </summary>
        private void Acceleration()
        {
            EditorGUILayout.Separator();

            MMV_MBT_Engine _engine = vehicle.Engine;

            engineAccelerationExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(engineAccelerationExpanded, "Acceleration");


            if (engineAccelerationExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Acceleration", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                var _acceleration = EditorGUILayout.Slider("velocity", _engine.Acceleration, ENGINE_MIN_ACCELERATION_VELOCITY, _engine.MaxAcceleration);
                var _maxAcceleration = EditorGUILayout.Slider("max", _engine.MaxAcceleration, ENGINE_MIN_ACCELERATION_FORCE, ENGINE_MAX_ACCELERATION_FORCE);
                var _slowdown = EditorGUILayout.FloatField("slowdown", _engine.Slowdown);

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Velocity", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                var _maxSpeedForward = EditorGUILayout.FloatField("forward", _engine.MaxForwardVelocity);
                var _maxSpeedBackward = EditorGUILayout.FloatField("reverse", _engine.MaxReverseVelocity);

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Steering", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                var _maxRotationSpeed = EditorGUILayout.Slider("max rotation speed", _engine.MaxRotationSpeed, ENGINE_MIN_ROTATION_VELOCITY, _engine.MaxForwardVelocity);

                EditorGUI.indentLevel--;


                //--------------------------------------------------

                _maxSpeedForward = Mathf.Clamp(_maxSpeedForward, ENGINE_MIN_SPEED, ENGINE_MAX_SPEED);
                _maxSpeedBackward = Mathf.Clamp(_maxSpeedBackward, ENGINE_MIN_SPEED, ENGINE_MAX_SPEED);
                _slowdown = Mathf.Clamp(_slowdown, ENGINE_MIN_BRAKE_FORCE, ENGINE_MAX_BRAKE_FORCE - 1);


                //--------------------------------------------------
                _engine.Acceleration = _acceleration;
                _engine.MaxAcceleration = _maxAcceleration;
                _engine.Slowdown = _slowdown;

                _engine.MaxForwardVelocity = _maxSpeedForward;
                _engine.MaxReverseVelocity = _maxSpeedBackward;

                _engine.MaxRotationSpeed = _maxRotationSpeed;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            engineBrakeExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(engineBrakeExpanded, "Brake");

            if (engineBrakeExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Separator();

                var _maxBrakeForce = EditorGUILayout.FloatField("brake force", _engine.MaxBrakeForce);

                EditorGUI.indentLevel--;

                _maxBrakeForce = Mathf.Clamp(_maxBrakeForce, _engine.Slowdown, ENGINE_MAX_BRAKE_FORCE);
                _engine.MaxBrakeForce = _maxBrakeForce;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            vehicle.Engine = _engine;
        }

        /// <summary>
        /// Show gears editor 
        /// </summary>
        private void ShowGearSystem()
        {
            // add or subtract from maximum or minimum to avoid being equal in extreme circumstances

            MMV_MBT_Engine _engine = vehicle.Engine;

            engineGearsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(engineGearsExpanded, "Gears");

            if (engineGearsExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Gear settings", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                // gears settings
                {
                    var _lossOfStrength = MMV_MBT_Engine.MIN_OF_LOSS_STRENGTH;

                    if (_engine.ForwardGears.Length > 1 || _engine.BackwardGears.Length > 1)
                    {
                        float _maxStrenght = MMV_MBT_Engine.MAX_OF_LOSS_STRENGHT;
                        float _minStrenght = MMV_MBT_Engine.MIN_OF_LOSS_STRENGTH;

                        _lossOfStrength = EditorGUILayout.Slider("loss of strenght", _engine.LossOfStrength, _minStrenght, _maxStrenght);
                    }

                    _engine.LossOfStrength = _lossOfStrength;
                }

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Forward gears", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                // forward gears
                {
                    var _gearsAmountForward = EditorGUILayout.IntField("gears amount", _engine.ForwardGears.Length);
                    _gearsAmountForward = Mathf.Clamp(_gearsAmountForward, GEARS_MIN_AMOUNT, GEARS_MAX_AMOUNT_FORWARD);

                    if (_gearsAmountForward != _engine.ForwardGears.Length)
                    {
                        _engine.ForwardGears = GenerateGearsArray(_engine.ForwardGears, _gearsAmountForward, _engine.MaxForwardVelocity);
                    }

                    _engine.ForwardGears = AdjustGears(_engine.ForwardGears, _engine.MaxForwardVelocity);
                    EditorGUILayout.Separator();
                }

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Backward gears", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                // backward gears
                {
                    var _gearsAmountBackward = EditorGUILayout.IntField("gears amount", _engine.BackwardGears.Length);
                    _gearsAmountBackward = Mathf.Clamp(_gearsAmountBackward, GEARS_MIN_AMOUNT, GEARS_MAX_AMOUNT_FORWARD);

                    if (_gearsAmountBackward != _engine.BackwardGears.Length)
                    {
                        _engine.BackwardGears = GenerateGearsArray(_engine.BackwardGears, _gearsAmountBackward, _engine.MaxReverseVelocity);
                    }

                    _engine.BackwardGears = AdjustGears(_engine.BackwardGears, _engine.MaxReverseVelocity);
                }

                EditorGUI.indentLevel--;

                vehicle.Engine = _engine;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // adding and removing new gears
        float[] GenerateGearsArray(float[] currentGears, int amount, float maxSpeed)
        {
            var _gears = new List<float>(currentGears);

            // adding new gear between the last gear and the maximum speed
            while (_gears.Count < amount)
            {
                var _gear = _gears[_gears.Count - 1];
                _gear += (maxSpeed - _gears[_gears.Count - 1]) / 2;
                _gears.Add(_gear);
            }

            // removing the lasted gears
            while (_gears.Count > amount)
            {
                _gears.Remove(_gears[_gears.Count - 1]);
            }

            return _gears.ToArray();
        }

        // Controls gear shift values
        float[] AdjustGears(float[] current, float maxSpeed)
        {
            const int OFFSET_GEARS = 1;

            for (int i = 0; i < current.Length; i++)
            {
                // clamp gear min and max
                float _min = OFFSET_GEARS;
                float _max = maxSpeed - OFFSET_GEARS;

                // first gear
                if (i == 0)
                {
                    // if there is more than one gear
                    // the limit should be the next gear
                    if (current.Length > 1)
                    {
                        _max = current[i + 1] - OFFSET_GEARS;
                    }
                    else
                    {
                        _max = maxSpeed - OFFSET_GEARS;
                    }
                }

                // middle gears
                if (i > 0 && i < current.Length - 1)
                {
                    _min = current[i - 1] + OFFSET_GEARS;
                    _max = current[i + 1] - OFFSET_GEARS;
                }

                // last gear
                if (i == current.Length - 1)
                {
                    if (current.Length > 1)
                    {
                        _min = current[i - 1] + OFFSET_GEARS;
                    }

                    _max = maxSpeed - OFFSET_GEARS;
                }

                current[i] = EditorGUILayout.Slider("gear " + (i + 1) + " - " + (i + 2), current[i], _min, _max);
            }

            return current;
        }

        /// <summary>
        /// Show turret settings
        /// </summary>
        private void ShowTurretTab()
        {
            MMV_TurretController _turret = vehicle.Turret;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Transforms", MMV_EditorStyle.Label);
            EditorGUILayout.Separator();

            var _turretTransform = (Transform)EditorGUILayout.ObjectField("turret", _turret.Turret, typeof(Transform), true);
            var _cannonTransform = (Transform)EditorGUILayout.ObjectField("cannon", _turret.Gun, typeof(Transform), true);

            EditorGUILayout.Separator();

            if (_turretTransform && _cannonTransform)
            {
                EditorGUILayout.LabelField("Turret", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                var _horizontalVelocity = EditorGUILayout.Slider("horizontal velocity", _turret.RotSpeedHorizontal, TURRET_MIN_ROTATION_VELOCITY, TURRET_MAX_ROTATION_VELOCITY);

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Cannon", MMV_EditorStyle.Label);
                EditorGUILayout.Separator();

                var _verticalVelocity = EditorGUILayout.Slider("vertical velocity", _turret.RotSpeedVertical, TURRET_MIN_ROTATION_VELOCITY, TURRET_MAX_ROTATION_VELOCITY);

                float _min = -_turret.MinGunAngle;
                float _max = _turret.MaxGunAngle;

                EditorGUILayout.MinMaxSlider("angle", ref _min, ref _max, GUN_MIN_ANGLE, GUN_MAX_ANGLE);
                EditorGUILayout.LabelField("min: " + _min + "     max: " + _max);

                EditorGUILayout.Separator();

                _turret.RotSpeedVertical = _verticalVelocity;
                _turret.RotSpeedHorizontal = _horizontalVelocity;
                _turret.MinGunAngle = (int)_min;
                _turret.MaxGunAngle = (int)_max;
            }

            _turret.Turret = _turretTransform;
            _turret.Gun = _cannonTransform;

            vehicle.Turret = _turret;


            EditorGUILayout.Separator();
        }

        private void ShowWheelsTab()
        {
            MMV_MBT_WheelManager _wheelsManager = vehicle.Wheels;

            // wheels characteristics
            {
                EditorGUILayout.Separator();
                wheelSettingsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelSettingsExpanded, "Wheels characteristics");

                if (wheelSettingsExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("Wheel", MMV_EditorStyle.Label);
                    EditorGUILayout.Separator();

                    var _radius = EditorGUILayout.FloatField("radius", _wheelsManager.WheelRadius);

                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("Spring", MMV_EditorStyle.Label);
                    EditorGUILayout.Separator();

                    var _springLenght = EditorGUILayout.FloatField("lenght", _wheelsManager.SpringLenght);
                    var _springHeight = EditorGUILayout.Slider("height", _wheelsManager.SpringHeight, WHEELS_MIN_SPRING_HEIGHT, WHEELS_MAX_SPRING_HEIGHT);
                    var _springStiffness = EditorGUILayout.IntField("stiffness", _wheelsManager.SpringStiffness);
                    var _springDamper = EditorGUILayout.IntField("damper", _wheelsManager.SpringDamper);

                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("Friction", MMV_EditorStyle.Label);
                    EditorGUILayout.Separator();

                    var _forwardFriction = EditorGUILayout.Slider("forward", _wheelsManager.ForwardFriction, WHEELS_MIN_FRICTION, WHEELS_MAX_FRICTION);
                    var _sideFriction = EditorGUILayout.Slider("side", _wheelsManager.SideFriction, WHEELS_MIN_FRICTION, WHEELS_MAX_FRICTION);
                    var _frictionMultiply = EditorGUILayout.Slider("multiply", _wheelsManager.MultiplyFriction, WHEELS_MIN_FRICTION, WHEELS_MAX_FRICTION);

                    EditorGUI.indentLevel--;


                    EditorGUILayout.Separator();

                    //-----------------------------------------------

                    _radius = Mathf.Clamp(_radius, WHEELS_MIN_RADIUS, WHEELS_MAX_RADIUS);
                    _springLenght = Mathf.Clamp(_springLenght, WHEELS_MIN_SPRING_LENGHT, WHEELS_MAX_SPRING_LENGHT);
                    _springStiffness = Mathf.Clamp(_springStiffness, WHEELS_MIN_SPRING_STIFFNESS, WHEELS_MAX_SPRING_STIFFNESS);
                    _springDamper = Mathf.Clamp(_springDamper, WHEELS_MIN_SPRING_DAMPER, WHEELS_MAX_SPRING_DAMPER);


                    //-----------------------------------------------

                    _wheelsManager.WheelRadius = _radius;
                    _wheelsManager.SpringLenght = _springLenght;
                    _wheelsManager.SpringHeight = _springHeight;
                    _wheelsManager.SpringStiffness = _springStiffness;
                    _wheelsManager.SpringDamper = _springDamper;
                    _wheelsManager.ForwardFriction = _forwardFriction;
                    _wheelsManager.SideFriction = _sideFriction;
                    _wheelsManager.MultiplyFriction = _frictionMultiply;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // tracks
            {
                wheelsTracksExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsTracksExpanded, "Tracks");

                if (wheelsTracksExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();

                    var _trackMoveSpeed = EditorGUILayout.FloatField("multiply rotation velocity", _wheelsManager.TrackMoveSpeed);
                    var _leftTrack = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("left track", _wheelsManager.LeftTrack, typeof(SkinnedMeshRenderer), true);
                    var _rightTrack = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("right track", _wheelsManager.RightTrack, typeof(SkinnedMeshRenderer), true);

                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;

                    _wheelsManager.TrackMoveSpeed = _trackMoveSpeed;
                    _wheelsManager.LeftTrack = _leftTrack;
                    _wheelsManager.RightTrack = _rightTrack;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // left wheels
            {
                wheelsLeftWheelsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsLeftWheelsExpanded, "Left wheels");

                if (wheelsLeftWheelsExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    leftWheelsList.DoLayoutList();  // drawing list
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // right wheels
            {
                wheelsRightWheelsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsRightWheelsExpanded, "Right wheels");

                if (wheelsRightWheelsExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();
                    rightWheelsList.DoLayoutList(); // drawing list
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // additional wheels
            {
                EditorGUILayout.PropertyField(leftAdditionalWheels);
                EditorGUILayout.PropertyField(rightAdditionalWheels);
            }

            // particles
            {
                wheelsParticlesExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsParticlesExpanded, "Wheels particles");

                if (wheelsParticlesExpanded)
                {
                    MMV_MBT_WheelManager.TrackParticleEmission _particleSystem = _wheelsManager.TracksParticles;


                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();

                    var _leftParticle = (ParticleSystem)EditorGUILayout.ObjectField("left particle", _particleSystem.LeftParticle, typeof(ParticleSystem), true);
                    var _rightParticle = (ParticleSystem)EditorGUILayout.ObjectField("right particle", _particleSystem.RightParticle, typeof(ParticleSystem), true);


                    if (_particleSystem.RightParticle || _particleSystem.LeftParticle)
                    {
                        var _maxEmission = EditorGUILayout.Slider("max emission", _particleSystem.MaxEmission, WHEELS_MIN_PARTICLES_EMISSION, WHEELS_MAX_PARTICLES_EMISSION);
                        var _stopDelay = EditorGUILayout.Slider("stop delay", _particleSystem.StopDelay, WHEELS_MIN_PARTICLES_STOP_DELAY, WHEELS_MAX_PARTICLES_STOP_DELAY);

                        //-------------------------------------------

                        _particleSystem.MaxEmission = _maxEmission;
                        _particleSystem.StopDelay = _stopDelay;
                    }

                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;

                    //------------------------------------------

                    _particleSystem.LeftParticle = _leftParticle;
                    _particleSystem.RightParticle = _rightParticle;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            vehicle.Wheels = _wheelsManager;
        }

        /// <summary>
        /// Show stability editor 
        /// </summary>
        private void ShowStabilityTab()
        {
            MMV_MBT_Engine _engine = vehicle.Engine;

            var _slopeDeceleration = EditorGUILayout.FloatField("slope deceleration", _engine.AngleDesaceleration);
            var _centerOfMass = (Transform)EditorGUILayout.ObjectField("center of mass", vehicle.CenterOfMass, typeof(Transform), true);


            //----------------------------------------------------


            _slopeDeceleration = Mathf.Clamp(_slopeDeceleration, STABILITY_MIN_SLOPE_DECELERATION, STABILITY_MAX_SLOPE_DECELERATION);

            //----------------------------------------------------

            _engine.AngleDesaceleration = _slopeDeceleration;
            vehicle.CenterOfMass = _centerOfMass;

            vehicle.Engine = _engine;
        }

        /// <summary>
        /// Show engine sound configuration editor
        /// </summary>
        private void EngineSound()
        {
            MMV_MBT_Engine _engine = vehicle.Engine;

            engineSoundExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(engineSoundExpanded, "Engine sound");
            EditorGUILayout.Separator();

            if (engineSoundExpanded)
            {
                const float OFFSET_SOUND = 0.1f;

                EditorGUI.indentLevel++;
                var _audioPlayer = (AudioSource)EditorGUILayout.ObjectField("audio source", _engine.EngineSound.AudioPlayer, typeof(AudioSource), true);

                if (_engine.EngineSound.AudioPlayer)
                {
                    var _audioClip = (AudioClip)EditorGUILayout.ObjectField("audio clip", _engine.EngineSound.Sound, typeof(AudioClip), true);
                    EditorGUILayout.Separator();

                    var _basePitch = EditorGUILayout.Slider("min pitch", _engine.EngineSound.BasePitch, ENGINE_MIN_SOUND_PITCH, _engine.EngineSound.MaxForwardPitch - OFFSET_SOUND);
                    var _maxForwardPitch = EditorGUILayout.Slider("max forward pitch", _engine.EngineSound.MaxForwardPitch, _basePitch + OFFSET_SOUND, ENGINE_MAX_SOUND_PITCH);
                    var _maxBackwardPitch = EditorGUILayout.Slider("max backward pitch", _engine.EngineSound.MaxBackwardPitch, _basePitch + OFFSET_SOUND, _maxForwardPitch);

                    _engine.EngineSound.BasePitch = _basePitch;
                    _engine.EngineSound.MaxForwardPitch = _maxForwardPitch;
                    _engine.EngineSound.MaxBackwardPitch = _maxBackwardPitch;
                    _engine.EngineSound.Sound = _audioClip;
                }

                EditorGUI.indentLevel--;

                _engine.EngineSound.AudioPlayer = _audioPlayer;
            }

            vehicle.Engine = _engine;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Generates the list data to be drawn
        /// </summary>
        /// <param name="list">
        /// Current list 
        /// </param>
        /// <param name="listName">
        /// Name of the list for draw on the top label
        /// </param>
        /// <returns></returns>
        private ReorderableList DrawWheelsList(ReorderableList list, string listName)
        {
            // list name
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, listName);
            };

            // drawing elements 
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                const int OFFSET = 5;

                rect.height = EditorGUIUtility.singleLineHeight;

                // get the elements of the list indexes
                var _wheel = list.serializedProperty.GetArrayElementAtIndex(index);


                // drawing element of the list

                rect.width = 50 + OFFSET;
                EditorGUI.LabelField(rect, "collider");

                rect.x += rect.width + OFFSET;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("collider"), GUIContent.none);

                rect.x += rect.width + OFFSET;
                rect.width = 40;
                EditorGUI.LabelField(rect, "mesh");

                rect.x += rect.width + OFFSET;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("mesh"), GUIContent.none);

                rect.x += rect.width + OFFSET;
                rect.width = 40;
                EditorGUI.LabelField(rect, "bone");

                rect.x += rect.width + OFFSET;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("bone"), GUIContent.none);
            };

            return list;
        }



        //   ---------------------------   HANDLES   ---------------------------   //   


        /// <summary>
        /// Draw wheels gizmos on 3D view
        /// </summary>
        /// <param name="wheels">
        /// Wheels to draw
        /// </param>
        private void DrawWheelsHandles(MMV_MBT_Wheel[] wheels)
        {
            MMV_MBT_WheelManager _wheelManager = vehicle.Wheels;

            var _wheelRadius = _wheelManager.WheelRadius;
            var _springLenght = _wheelManager.SpringLenght;
            var _springHeight = _wheelManager.SpringHeight;

            foreach (var w in wheels)
            {
                if (w.Collider)
                {
                    var _colliderPos = w.Collider.position;
                    var _wheelUp = w.Collider.up;
                    var _onGrounded = w.OnGronded;
                    var _hitPos = w.WheelHit.point;
                    var _springPos = _colliderPos + (w.Collider.up * _springHeight);

                    // --- wheel position

                    var _onAirPos = w.Collider.position + (-_wheelUp * _springLenght);
                    var _onGroundedPos = _colliderPos + (-_wheelUp * (w.WheelHit.distance - _wheelManager.SpringHeight - _wheelManager.WheelRadius));
                    var _finalWheelPos = _onGrounded ? _onGroundedPos : _onAirPos;

                    // suspension start position 
                    Handles.color = springPosColor;
                    Handles.DrawLine(w.Collider.position, _springPos);

                    // spring lenght
                    Handles.color = suspensionColor;
                    Handles.DrawLine(w.Collider.position, _finalWheelPos);

                    // wheel circle
                    Handles.color = wheelCircleColor;
                    Handles.DrawWireArc(_finalWheelPos, w.Collider.right, w.Collider.forward, 360, _wheelRadius);
                }
            }
        }



        //   ---------------------------   SAVE LOAD   ---------------------------   //   


        /// <summary>
        /// Save editor data
        /// </summary>
        private void SaveEditorState()
        {
            EditorPrefs.SetInt(nameof(vehicle) + nameof(currentEditorTab), currentEditorTab);

            EditorPrefs.SetBool(nameof(vehicle) + nameof(engineAccelerationExpanded), engineAccelerationExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(engineBrakeExpanded), engineBrakeExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(engineGearsExpanded), engineGearsExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(engineSoundExpanded), engineSoundExpanded);

            EditorPrefs.SetBool(nameof(vehicle) + nameof(wheelSettingsExpanded), wheelSettingsExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(wheelsLeftWheelsExpanded), wheelsLeftWheelsExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(wheelsParticlesExpanded), wheelsParticlesExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(wheelsRightWheelsExpanded), wheelsRightWheelsExpanded);
            EditorPrefs.SetBool(nameof(vehicle) + nameof(wheelsTracksExpanded), wheelsTracksExpanded);
        }

        /// <summary>
        /// Load editor data
        /// </summary>
        private void LoadEditorState()
        {
            currentEditorTab = EditorPrefs.GetInt(nameof(vehicle) + nameof(currentEditorTab));

            engineAccelerationExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(engineAccelerationExpanded));
            engineBrakeExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(engineBrakeExpanded));
            engineGearsExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(engineGearsExpanded));
            engineSoundExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(engineSoundExpanded));

            wheelSettingsExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(wheelSettingsExpanded));
            wheelsLeftWheelsExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(wheelsLeftWheelsExpanded));
            wheelsParticlesExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(wheelsParticlesExpanded));
            wheelsRightWheelsExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(wheelsRightWheelsExpanded));
            wheelsTracksExpanded = EditorPrefs.GetBool(nameof(vehicle) + nameof(wheelsTracksExpanded));
        }
    }
}