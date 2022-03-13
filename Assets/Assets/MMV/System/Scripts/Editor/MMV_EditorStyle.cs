using UnityEngine;
using UnityEditor;

namespace MMV.Editor
{
    public class MMV_EditorStyle
    {
        private MMV_EditorStyle() { }

        private static GUIStyle labelStyle;

        public static GUIStyle Label
        {
            get
            {
                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.fontStyle = FontStyle.Bold;
                }

                return labelStyle;
            }

        }
    }
}