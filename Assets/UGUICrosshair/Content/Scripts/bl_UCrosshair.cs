using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bl_UCrosshair : MonoBehaviour
{
    #region Public members   
    [Header("Settings")]
    [Range(1, 10)] public float ScaleLerp = 5;
    [Range(0.1f, 5)] public float RotationSpeed = 2;
    [Range(0.01f, 1)] public float OnFireScaleRate = 0.1f;
    public bool RotateCrosshair = false;
    public bool FollowMouse = false;
    public bool HideMouseCursor = true;

    [Header("Hit Marker")]
    [Range(0.1f, 3)] public float Duration = 1;
    [Range(5, 50)] public float IncreaseAmount = 25;
    public Color HitMarkerColor = Color.white;

    [Header("Target Detection")]
    public bool useDetection = true;
    public LayerMask TargetLayer;
    public Color OnTargetFocusColor = Color.red;
    [Range(1, 500)] public float TargetDetectMaxDistance = 100;
    [Range(0.01f, 2)] public float ColorTransitionDuration = 0.15f;

    [Header("References")]
    [SerializeField] private bl_UCrosshairInfo[] Crosshairs = null;
    [SerializeField] private RectTransform RootContent = null;
    [SerializeField] private RectTransform HitMarkerRoot = null;
    #endregion

    #region Private members    
    private Vector2 InitSizeDelta;
    private bool isAim = false;
    private int currentCross = 0;
    private Canvas m_Canvas;
    private Vector3 InitialPosition;
    private Vector3 InitialRotation;
    private float lastTimeFire;
    private CanvasGroup m_HitAlpha;
    private Vector2 defaultHitSize;
    private float hitDuration;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (RootContent != null)
        {
            InitSizeDelta = RootContent.sizeDelta;
            InitialPosition = RootContent.position;
            InitialRotation = RootContent.eulerAngles;
        }
        if (HitMarkerRoot != null)
        {
            m_HitAlpha = HitMarkerRoot.GetComponent<CanvasGroup>();
            defaultHitSize = HitMarkerRoot.sizeDelta;
            if (m_HitAlpha != null) { m_HitAlpha.alpha = 0; }
            Graphic[] hmg = HitMarkerRoot.GetComponentsInChildren<Graphic>();
            foreach (Graphic g in hmg) { g.color = HitMarkerColor; }
        }
        m_Canvas = transform.root.GetComponent<Canvas>();
        if (m_Canvas == null) { m_Canvas = transform.root.GetComponentInChildren<Canvas>(); }
        if (HideMouseCursor) { Cursor.visible = false; }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (GetCrosshair.isStatic)
            return;

        ScaleContent();
        Rotate();
        FollowMouseControll();
        RaycastControll();
    }

    /// <summary>
    /// 
    /// </summary>
    void FollowMouseControll()
    {
        if (RootContent == null)
            return;
        if (GetCrosshair.isStatic || !FollowMouse)
            return;

        RootContent.position = bl_UCrosshairUtils.GetMousePosition(m_Canvas);
    }

    /// <summary>
    /// 
    /// </summary>
    void ScaleContent()
    {
        if (RootContent == null)
            return;

        Vector2 target = (isAim) ? new Vector2(GetCrosshair.OnAimScaleAmount, GetCrosshair.OnAimScaleAmount) : InitSizeDelta;
        RootContent.sizeDelta = Vector2.Lerp(RootContent.sizeDelta, target, Time.unscaledDeltaTime * ScaleLerp);
    }

    /// <summary>
    /// 
    /// </summary>
    void Rotate()
    {
        if (!RotateCrosshair || RootContent == null)
            return;

        RootContent.Rotate(Vector3.forward * RotationSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    void RaycastControll()
    {
        if (!useDetection || RenderCamera == null)
            return;

        //get middle screen position
        Ray ray = RenderCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (FollowMouse)
        {
            //get the mouse position and convert to world position
            ray = RenderCamera.ScreenPointToRay(Input.mousePosition);
        }
        //Vector3 point = ray.origin + (ray.direction * 100);
        if (Physics.Raycast(ray.origin, ray.direction, TargetDetectMaxDistance, TargetLayer))
        {
            GetCrosshair.SetColor(OnTargetFocusColor, ColorTransitionDuration);
        }
        else
        {
            GetCrosshair.SetDefaultColors(ColorTransitionDuration);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Change(int id)
    {
        if (id <= Crosshairs.Length - 1)
        {
            currentCross = id;
            foreach (bl_UCrosshairInfo g in Crosshairs) { g.gameObject.SetActive(false); }
            GetCrosshair.gameObject.SetActive(true);
            if (GetCrosshair.isStatic) { Reset(); }
        }
        else
        {
            Debug.LogWarning("the id is more bigger that cross hair list length!");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnFire()
    {
        if (RootContent == null)
            return;
        if (GetCrosshair.isStatic)
            return;
        if (Time.time < lastTimeFire)
            return;

        RootContent.sizeDelta = new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount);
        lastTimeFire = Time.time + OnFireScaleRate;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnAim(bool aim)
    {
        if (RootContent == null)
            return;
        if (GetCrosshair.isStatic)
            return;

        isAim = aim;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnHit()
    {
        if (HitMarkerRoot == null)
            return;

        StopCoroutine("OnHitMarker");
        StartCoroutine("OnHitMarker");
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetColor(Color c)
    {
        GetCrosshair.SetColor(c, ColorTransitionDuration);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetDefaultColors()
    {
        GetCrosshair.SetDefaultColors(ColorTransitionDuration);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
        RootContent.position = InitialPosition;
        RootContent.eulerAngles = InitialRotation;
    }

    /// <summary>
    /// 
    /// </summary>
    public bl_UCrosshairInfo GetCrosshair
    {
        get
        {
            return Crosshairs[currentCross];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator OnHitMarker()
    {
        hitDuration = 0;
        HitMarkerRoot.sizeDelta = defaultHitSize;
        if (m_HitAlpha != null) { m_HitAlpha.alpha = 1; }
        Vector2 sizeTarget = new Vector2(IncreaseAmount, IncreaseAmount);
        while (hitDuration < 1)
        {
            HitMarkerRoot.sizeDelta = Vector2.Lerp(HitMarkerRoot.sizeDelta, sizeTarget, hitDuration);
            if (m_HitAlpha != null) { m_HitAlpha.alpha = Mathf.Lerp(m_HitAlpha.alpha, 0, hitDuration); }
            hitDuration += Time.unscaledDeltaTime / Duration;
            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (RootContent == null)
            return;

        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
    }
#endif

    private Camera m_camera;
    public Camera RenderCamera
    {
        get
        {
            if (m_camera == null)
            {
                m_camera = Camera.main;
                if (m_camera == null) m_camera = Camera.current;
            }
            return m_camera;
        }
        set => m_camera = value;
    }

    private static bl_UCrosshair m_instance;
    public static bl_UCrosshair Instance
    {
        get
        {
            if (m_instance == null)
            {
                bl_UCrosshair[] all = FindObjectsOfType<bl_UCrosshair>();
                if (all.Length <= 0) { Debug.LogWarning("There are not an cross hair in this scene!"); return null; }
                else
                {
                    if (all.Length > 1) { Debug.LogWarning("There are 2 or more cross hair in this scene, if you use multiple cross hair, get the reference manually instead of by singleton!"); }
                    m_instance = all[0];
                }
            }
            return m_instance;
        }
    }
}