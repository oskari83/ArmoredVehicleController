using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VehicleController))]
public class ShootingController : MonoBehaviour{

    [Header("Bullet Prefab")]
    public GameObject bullet;
    
    public RectTransform crosshairCircleRectTransform;

    private AudioSource shootAudioSource;

    private VehicleController vehicleController;
    private GameObject gunObject;
    private GameObject gun2;
    private Vector3 crossPos;
    private Vector3 finalPos;

    public GameObject crosshairSpread;
    public GameObject crosshairSpreadx2;

    public float fx;
    public float fy;
    public float fz;

    private float scaleFactor = 0.018f / 16.557f;

    public float gunDispersion = 1f;
    public float minDispersion = 0.5f;
    public float maxDispersion = 5f;
    public float aimSpeed = 0.01f;
    public float crossHairLerp = 1f;
    private float cx;
    private float lastVel;
    private float acceleration;

    private float rawrng;
    private float rawdirrng;
    private float scaleddirrng;
    private float scaledrng;

    private void Start(){
        vehicleController = gameObject.GetComponent<VehicleController>();
        shootAudioSource = gameObject.GetComponent<AudioSource>();
        gunObject = vehicleController.gunShootPos;
        gun2 = vehicleController.gunGO;
        lastVel = 0f;
    }

    private void Update(){
        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }

        // .25 degrees left and right
        Vector3 aimCircle = gunObject.transform.forward;
        Quaternion spreadAngle = Quaternion.AngleAxis(gunDispersion, gunObject.transform.up);
        Quaternion spreadAnglex2 = Quaternion.AngleAxis(-gunDispersion, gunObject.transform.up);
        Vector3 newVector = spreadAngle * aimCircle;
        Vector3 newVector2 = spreadAnglex2 * aimCircle;

        //do the raycasts
        Vector3 ray25right = CrosshairCast(gunObject.transform.position, newVector);
        Vector3 ray25left = CrosshairCast(gunObject.transform.position, newVector2);
        Vector3 rayStraight = CrosshairCast(gunObject.transform.position, aimCircle);
        //get world to screenpoint
        Camera _mainCamera = vehicleController.cameraInUse;
        Vector3 rightScreenPos = _mainCamera.WorldToScreenPoint(ray25right);
        Vector3 straightScreenPos = _mainCamera.WorldToScreenPoint(rayStraight);
        crosshairSpread.transform.position = rightScreenPos;
        crosshairSpreadx2.transform.position = _mainCamera.WorldToScreenPoint(ray25left);
        //crosshair size
        float crosshairspreadDistance = Mathf.Sqrt(Mathf.Pow((rightScreenPos.x-straightScreenPos.x),2) + Mathf.Pow(rightScreenPos.y-straightScreenPos.y,2));

        cx = scaleFactor*crosshairspreadDistance;
        cx = Mathf.Clamp(cx, 0,0.2f);
        crossPos = new Vector3(cx,cx,1);
        finalPos = Vector3.Lerp(finalPos, crossPos, Time.deltaTime * crossHairLerp);
        crosshairCircleRectTransform.localScale = finalPos;

        acceleration = (vehicleController.VeL - lastVel) / Time.fixedDeltaTime;
    }

    private void FixedUpdate(){
        //for dispersion calculation
        acceleration = (vehicleController.VeL - lastVel) / Time.fixedDeltaTime;
        //increase dispersion while accelerating, decrease while decelerating or almost still
        if(acceleration>1f){
            gunDispersion = ((vehicleController.VeL / (vehicleController.maxSpeed * 3.6f)) * (maxDispersion-minDispersion))+minDispersion;
            //gunDispersion += (acceleration/500);
        }else if(acceleration < -1f || vehicleController.VeL<1f){
            float delta = gunDispersion - minDispersion;
            delta = Mathf.Clamp(delta, 1f,1.41f);
            gunDispersion -= (aimSpeed * (delta*delta));
        }
        //clam dispersion
        gunDispersion = Mathf.Clamp(gunDispersion,minDispersion,maxDispersion);
        //for acceleration calculations
        lastVel = vehicleController.VeL;
        //Debug.Log("acceleration=" + acceleration);
    }

    private void Shoot(){
        //implements accuracy rng by getting first distance from center and then angle around unit circle
        rawrng = Random.value;
        rawdirrng = Random.value;
        scaleddirrng = 2 * Mathf.PI * rawdirrng;
        float xval = Mathf.Cos(scaleddirrng);
        float yval = Mathf.Sin(scaleddirrng);
        // do x^3 so that extreme values are exponentially more rare
        scaledrng = gunDispersion * (rawrng*rawrng*rawrng);
        Debug.Log("raw rng:" + rawrng + " scl rng:" + scaledrng);
        xval = xval * scaledrng;
        yval = yval * scaledrng;
        // shoots relative to our gun rotation
        //shoots on x-axis
        Quaternion spreadOnX = Quaternion.AngleAxis(xval, Vector3.up);
        //shoots on y-axis
        Quaternion spreadOnY = Quaternion.AngleAxis(yval, Vector3.left);
        spreadOnX *=spreadOnY;
        Quaternion rot = gunObject.transform.rotation;
        rot *= spreadOnX;
        GameObject bul = Instantiate(bullet, gunObject.transform.position, rot);
        shootAudioSource.Play();
    }

    private Vector3 CrosshairCast(Vector3 pos, Vector3 dirVector){
        Ray crosshairCast;
        crosshairCast = new Ray(pos,  dirVector);
        Vector3 hitp = new Vector3();
        if (Physics.Raycast(crosshairCast, out RaycastHit hit,10000f)){
            //Debug.DrawLine(crossHairRay3.origin, hit3.point, Color.yellow);
            hitp = hit.point;
        }
        return hitp;
    }
}
