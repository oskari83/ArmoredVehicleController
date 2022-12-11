using UnityEngine;
using UnityEngine.UI;

public class bl_UCrosshairInfo : MonoBehaviour
{

    [Header("Info")]
    public bool isStatic = false;
    [Range(15, 150)] public float NormalScaleAmount = 50;
    [Range(15, 150)] public float OnFireScaleAmount = 75;
    [Range(10, 100)] public float OnAimScaleAmount = 35;

    private Graphic[] m_Graphics;
    private Color[] defaultsColors;
    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Graphics = transform.GetComponentsInChildren<Graphic>(true);
        defaultsColors = new Color[m_Graphics.Length];
        for(int i = 0; i < m_Graphics.Length; i++)
        {
            defaultsColors[i] = m_Graphics[i].color;
            m_Graphics[i].color = Color.white;
            m_Graphics[i].canvasRenderer.SetColor(defaultsColors[i]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetColor(Color c,float speed)
    {
        foreach(Graphic g in m_Graphics) { g.CrossFadeColor(c, speed, true, true); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetDefaultColors(float speed)
    {
        for (int i = 0; i < m_Graphics.Length; i++)
        {
            m_Graphics[i].CrossFadeColor(defaultsColors[i], speed, true, true);
        }
    }
}