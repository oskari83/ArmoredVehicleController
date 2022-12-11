using UnityEngine;
using System.Collections;

public class bl_UCrosshairUtils
{

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetMousePosition(Canvas m_Canvas)
    {
        Vector3 Return = Vector3.zero;

        if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Return = Input.mousePosition;
        }
        else if (m_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 tempVector = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Canvas.transform as RectTransform, Input.mousePosition, CameraInUse, out tempVector);
            Return = m_Canvas.transform.TransformPoint(tempVector);
        }

        return Return;
    }

    /// <summary>
    /// 
    /// </summary>
    public static Camera CameraInUse
    {
        get
        {
            if(Camera.main != null)
            {
                return Camera.main;
            }
            else
            {
                return Camera.current;
            }
        }
    }
}