using UnityEditor;
using UnityEngine;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_CameraController))]
    public class MMV_CameraControllerEditor : UnityEditor.Editor
    {
        private MMV_CameraController cameraController;

        private SerializedProperty prop;

        // camera position
        private const float CAMERA_POS_MAX_VERTICAL_ANGLE_UP = 90.0f;
        private const float CAMERA_POS_MIN_VERTICAL_ANGLE_UP = -45.0f;
        private const float CAMERA_POS_MAX_VERTICAL_ANGLE_DOWN = 45.0f;
        private const float CAMERA_POS_MIN_VERTICAL_ANGLE_DOWN = 0.0f;
        private const float CAMERA_POS_MAX_CAMERA_HEIGHT = 20.0f;
        private const float CAMERA_POS_MIN_CAMERA_HEIGHT = 0.0f;
        private const float CAMERA_POS_MAX_ROTATION_SPEED = 100.0f;
        private const float CAMERA_POS_MIN_ROTATION_SPEED = 0.1f;

        // camera zoom
        private const float ZOOM_MAX_CAMERA_DISTANCE = 50.0f;
        private const float ZOOM_MIN_CAMERA_DISTANCE = 2.0f;

        // crosshair
        private const int ZOOM_MIN_CROSSHAIR_DISTANCE = 100;
        private const string ZOOM_DEFAULT_LAYER = "Default";

        private bool inputsExpanded;
        private bool positionExpanded;
        private bool zoomExpanded;
        private bool crosshairExpanded;
        private bool camerasExpanded;

        // input tabs
        private int currentTab;

        // editor save
        private const string INPUT_PREFS = "MMV_CameraControllerInputExpanded";

        private void OnEnable()
        {
            cameraController = (MMV_CameraController)target;
            /*
            if (cameraController.Inputs == null)
            {
                var _inputs = new MMV_InputCamera();
                var _keyboard = new MMV_InputCamera.CameraInput();

                _keyboard.Horizontal = "Mouse X";
                _keyboard.Vertical = "Mouse Y";
                _keyboard.CameraZoom = KeyCode.Mouse1; // <- right mouse button
                _keyboard.InvertHorizontal = false;
                _keyboard.InvertVertical = false;

                _inputs.Gamepad = new MMV_InputCamera.CameraInput();
                _inputs.Keyboard = _keyboard;

                cameraController.Inputs = _inputs;
            }
            */

            if (cameraController.CameraPosition == null)
            {
                var _cameraPos = new MMV_CameraController.Position();

                _cameraPos.Height = 4.0f;
                _cameraPos.MaxAngle = 45.0f;
                _cameraPos.MinAngle = 10.0f;
                _cameraPos.RotSpeed = 2.0f;

                cameraController.CameraPosition = _cameraPos;
            }

            if (cameraController.Zoom == null)
            {
                var _zoom = new MMV_CameraController.ZoomCamera();

                _zoom.CamZoomSpeed = -2.0f;
                _zoom.MaxCamDistance = 100.0f;
                _zoom.MinCamDistance = 2.0f;

                cameraController.Zoom = _zoom;
            }

            if (cameraController.GunCrosshair == null)
            {
                var _crosshair = new MMV_CameraController.Crosshair();

                _crosshair.MaxDistance = 5000;
                _crosshair.ObstaclesLayer = LayerMask.NameToLayer(ZOOM_DEFAULT_LAYER);

                cameraController.GunCrosshair = _crosshair;
            }

            LoadEditorData();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            cameraController.Vehicle = (VehicleController)EditorGUILayout.ObjectField("target", cameraController.Vehicle, typeof(VehicleController), true);
            EditorGUILayout.Separator();

            if (!cameraController.Vehicle)
            {
                return;
            }

            //-------------------------------------------------
            /*
            // inputs
            {
                inputsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(inputsExpanded, "Control inputs");

                if (inputsExpanded)
                {
                    ShowInputs();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            */

            EditorGUILayout.Separator();

            // camera position
            {
                positionExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(positionExpanded, "Camera position");

                if (positionExpanded)
                {
                    ShowCameraPosition();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Separator();

            // zoom
            {
                zoomExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(zoomExpanded, "Camera zoom");
                if (zoomExpanded)
                {
                    ShowCameraZoom();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Separator();

            // crosshair
            {
                crosshairExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(crosshairExpanded, "Camera crosshair");
                if (crosshairExpanded)
                {
                    ShowCrosshair();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Separator();

            // cameras
            {
                camerasExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(camerasExpanded, "Sniper Zoom Camera");
                if (camerasExpanded)
                {
                    ShowSniperZoom();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorUtility.SetDirty(cameraController);
            serializedObject.ApplyModifiedProperties();

            SaveEditorData();
        }

        /// <summary>
        /// Draw input settings 
        /// </summary>
        /*
        private void ShowInputs()
        {
            EditorGUI.BeginChangeCheck();

            // draw input settings tabs
            currentTab = GUILayout.Toolbar(currentTab, new string[] { "keyboad & mouse", "gamepad" });

            // checks if the tab has been changed to take the focus out of the currently focused field
            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
                serializedObject.ApplyModifiedProperties();
            }

            if (currentTab == 0) // keyboard & mouse
            {
                MMV_InputCamera.CameraInput _keyboard = cameraController.Inputs.Keyboard;

                string _horizontal = EditorGUILayout.TextField("horizontal axis", _keyboard.Horizontal);
                string _vertical = EditorGUILayout.TextField("vertical axis", _keyboard.Vertical);
                KeyCode _zoomKey = (KeyCode)EditorGUILayout.EnumFlagsField("zoom key", _keyboard.CameraZoom);

                EditorGUILayout.Separator();

                bool _invertHorizontal = EditorGUILayout.Toggle("invert horizontal axis", _keyboard.InvertHorizontal);
                bool _invertVertical = EditorGUILayout.Toggle("invert vertical axis", _keyboard.InvertVertical);

                _keyboard.Horizontal = _horizontal;
                _keyboard.Vertical = _vertical;
                _keyboard.CameraZoom = _zoomKey;
                _keyboard.InvertHorizontal = _invertHorizontal;
                _keyboard.InvertVertical = _invertVertical;

                cameraController.Inputs.Keyboard = _keyboard;
            }

            if (currentTab == 1) // gamepad
            {
                MMV_InputCamera.CameraInput _gamepad = cameraController.Inputs.Gamepad;

                string _horizontal = EditorGUILayout.TextField("horizontal axis", _gamepad.Horizontal);
                string _vertical = EditorGUILayout.TextField("vertical axis", _gamepad.Vertical);
                KeyCode _zoomKey = (KeyCode)EditorGUILayout.EnumFlagsField(_gamepad.CameraZoom);

                EditorGUILayout.Separator();

                bool _invertHorizontal = EditorGUILayout.Toggle("invert horizontal axis", _gamepad.InvertHorizontal);
                bool _invertVertical = EditorGUILayout.Toggle("invert vertical axis", _gamepad.InvertVertical);

                _gamepad.Horizontal = _horizontal;
                _gamepad.Vertical = _vertical;
                _gamepad.CameraZoom = _zoomKey;
                _gamepad.InvertHorizontal = _invertHorizontal;
                _gamepad.InvertVertical = _invertVertical;

                cameraController.Inputs.Gamepad = _gamepad;
            }
        }
        */

        /// <summary>
        /// Draw camera position configuration
        /// </summary>
        private void ShowCameraPosition()
        {
            MMV_CameraController.Position _position = cameraController.CameraPosition;

            float _height = EditorGUILayout.Slider("camera height", _position.Height, CAMERA_POS_MIN_CAMERA_HEIGHT, ZOOM_MAX_CAMERA_DISTANCE);
            float _maxAngle = EditorGUILayout.Slider("max vertical angle", _position.MaxAngle, CAMERA_POS_MIN_VERTICAL_ANGLE_UP, CAMERA_POS_MAX_VERTICAL_ANGLE_UP);
            float _minAngle = EditorGUILayout.Slider("min vertical angle", _position.MinAngle, CAMERA_POS_MIN_VERTICAL_ANGLE_DOWN, CAMERA_POS_MAX_VERTICAL_ANGLE_DOWN);
            float _rotSpeed = EditorGUILayout.Slider("rotation speed", _position.RotSpeed, CAMERA_POS_MIN_ROTATION_SPEED, CAMERA_POS_MAX_ROTATION_SPEED);

            _position.Height = _height;
            _position.MaxAngle = _maxAngle;
            _position.MinAngle = _minAngle;
            _position.RotSpeed = _rotSpeed;

            cameraController.CameraPosition = _position;
        }

        /// <summary>
        /// Show camera zoom configuration
        /// </summary>
        private void ShowCameraZoom()
        {
            MMV_CameraController.ZoomCamera _zoom = cameraController.Zoom;

            float _maxDist = EditorGUILayout.Slider("max camera distance", _zoom.MaxCamDistance, _zoom.MinCamDistance + 0.1f, ZOOM_MAX_CAMERA_DISTANCE);
            float _minDist = EditorGUILayout.Slider("min camera distance", _zoom.MinCamDistance, ZOOM_MIN_CAMERA_DISTANCE, _maxDist);
            float _speed = EditorGUILayout.FloatField("camera zoom speed", _zoom.CamZoomSpeed);

            _zoom.MaxCamDistance = _maxDist;
            _zoom.MinCamDistance = _minDist;
            _zoom.CamZoomSpeed = _speed;

            cameraController.Zoom = _zoom;
        }

        /// <summary>
        /// Vehicle gun sighting configuration
        /// </summary>
        private void ShowCrosshair()
        {
            MMV_CameraController.Crosshair _crosshair = cameraController.GunCrosshair;

            int _maxDistance = EditorGUILayout.IntField("max crosshair distance", _crosshair.MaxDistance);
            LayerMask _obstacles = EditorGUILayout.LayerField("collision layer", _crosshair.ObstaclesLayer);

            if (_maxDistance < ZOOM_MIN_CROSSHAIR_DISTANCE)
            {
                _maxDistance = ZOOM_MIN_CROSSHAIR_DISTANCE;
            }

            _crosshair.MaxDistance = _maxDistance;
            _crosshair.ObstaclesLayer = _obstacles;

            cameraController.GunCrosshair = _crosshair;
        }

        private void ShowSniperZoom()
        {
            float zoomSP = cameraController.sniperZoomSpeed;
            float _sniperZoomSpeed = EditorGUILayout.FloatField("Sniper Zoom Scroll Speed", zoomSP);
            cameraController.sniperZoomSpeed = _sniperZoomSpeed;
        }

        private void SaveEditorData()
        {
            EditorPrefs.SetBool(nameof(cameraController) + nameof(inputsExpanded), inputsExpanded);
            EditorPrefs.SetBool(nameof(cameraController) + nameof(positionExpanded), positionExpanded);
            EditorPrefs.SetBool(nameof(cameraController) + nameof(zoomExpanded), zoomExpanded);
            EditorPrefs.SetBool(nameof(cameraController) + nameof(crosshairExpanded), crosshairExpanded);
            EditorPrefs.SetBool(nameof(cameraController) + nameof(camerasExpanded), camerasExpanded);
        }
        private void LoadEditorData()
        {
            inputsExpanded = EditorPrefs.GetBool(nameof(cameraController) + nameof(inputsExpanded));
            positionExpanded = EditorPrefs.GetBool(nameof(cameraController) + nameof(positionExpanded));
            zoomExpanded = EditorPrefs.GetBool(nameof(cameraController) + nameof(zoomExpanded));
            crosshairExpanded = EditorPrefs.GetBool(nameof(cameraController) + nameof(crosshairExpanded));
            camerasExpanded = EditorPrefs.GetBool(nameof(cameraController) + nameof(camerasExpanded));
        }
    }
}

