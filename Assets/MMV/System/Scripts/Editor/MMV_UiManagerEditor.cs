using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_UiManager))]
    public class MMV_UiManagerEditor : UnityEditor.Editor
    {
        private const float MAX_CROSSHAIR_SMOOTH_TIME = 40.0f;
        private const float MIN_CROSSHAIR_SMOOTH_TIME = 5.0f;

        private MMV_UiManager ui;

        private bool crosshairExpanded;
        private bool engineExpanded;
        private bool vehicleStatusExpanded;

        private void OnEnable()
        {
            ui = (MMV_UiManager)target;

            Load();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();
            ui.Vehicle = (MMV_MBT_Vehicle)EditorGUILayout.ObjectField("vehicle", ui.Vehicle, typeof(MMV_MBT_Vehicle), true);
            ui.CameraController = (MMV_CameraController)EditorGUILayout.ObjectField("camera controller", ui.CameraController, typeof(MMV_CameraController), true);
            EditorGUILayout.Separator();

            if (!ui.Vehicle)
            {
                return;
            }

            // crosshair
            {
                if (ui.CameraController)
                {
                    crosshairExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(crosshairExpanded, "Gun");

                    if (crosshairExpanded)
                    {
                        ui.Crosshair = (RectTransform)EditorGUILayout.ObjectField("crosshair", ui.Crosshair, typeof(RectTransform), true);
                        ui.Reload = (Text)EditorGUILayout.ObjectField("reload", ui.Reload, typeof(Text), true);
                        ui.CrosshairMoveSpeed = EditorGUILayout.Slider("crosshair smooth speed", ui.CrosshairMoveSpeed, MIN_CROSSHAIR_SMOOTH_TIME, MAX_CROSSHAIR_SMOOTH_TIME);

                        EditorGUILayout.Separator();
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }

            // engine
            {
                engineExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(engineExpanded, "Vehicle engine");

                if (engineExpanded)
                {
                    ui.Gear = (Text)EditorGUILayout.ObjectField("gear text", ui.Gear, typeof(Text), true);
                    ui.Velocity = (Text)EditorGUILayout.ObjectField("velocity text", ui.Velocity, typeof(Text), true);

                    EditorGUILayout.Separator();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            Save();
            EditorUtility.SetDirty(ui);
        }

        /// <summary>
        /// Save editor variables
        /// </summary>
        private void Save()
        {
            EditorPrefs.SetBool(nameof(ui) + nameof(crosshairExpanded), crosshairExpanded);
            EditorPrefs.SetBool(nameof(ui) + nameof(engineExpanded), engineExpanded);
            EditorPrefs.SetBool(nameof(ui) + nameof(vehicleStatusExpanded), vehicleStatusExpanded);
        }

        /// <summary>
        /// Load editor variables
        /// </summary>
        private void Load()
        {
            crosshairExpanded = EditorPrefs.GetBool(nameof(ui) + nameof(crosshairExpanded));
            engineExpanded = EditorPrefs.GetBool(nameof(ui) + nameof(engineExpanded));
            vehicleStatusExpanded = EditorPrefs.GetBool(nameof(ui) + nameof(vehicleStatusExpanded));
        }
    }
}
