using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class ShootingController : MonoBehaviour{

    [Header("Bullet Prefab")]
    public GameObject bullet;

    public int[] bulletsOfT = new int [3] {5,4,3};
    public int selectedBullet = 0;
    
    public RectTransform crosshairCircleRectTransformImproved;
    private AudioSource shootAudioSource;

    private VehicleController vehicleController;

    private UIController uicontroller;
    private GameObject gunObject;
    private GameObject gun2;
    private Vector3 crossPos;
    private Vector3 finalPos;

    public GameObject crosshairSpread;
    public GameObject crosshairSpreadx2;
    public GameObject crosshairSpread_v;
    public GameObject crosshairSpreadx2_v;

    private GameObject lastSelected;

    private float scaleFactor = 0.018f / 16.557f;

    public float gunDispersion = 1f;
    public float minDispersion = 0.5f;
    public float maxDispersion = 5f;
    public float shootingDispersion = 4f;
    public float aimSpeed = 1f;
    public float dispersionIncreaseCoefficient = 40f;
    public float crossHairLerp = 10f;

    private float cx;
    private float lastVel;
    private float acceleration;

    private float rawrng;
    private float rawdirrng;
    private float scaleddirrng;
    private float scaledrng;
    private bool justShot = false;
    private float oldX;

    private void Start(){
        vehicleController = gameObject.GetComponent<VehicleController>();
        uicontroller = gameObject.GetComponent<UIController>();
        shootAudioSource = gameObject.GetComponent<AudioSource>();
        gunObject = vehicleController.gunShootPos;
        gun2 = vehicleController.gunGO;
        lastVel = 0f;
    }

    private void Update(){
        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }

        CalculateDispersion();

        //get gun dispersion in vectors
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
        Vector3 leftScreenPos = _mainCamera.WorldToScreenPoint(ray25left);
        Vector3 rightScreenPos = _mainCamera.WorldToScreenPoint(ray25right);
        Vector3 straightScreenPos = _mainCamera.WorldToScreenPoint(rayStraight);
        crosshairSpread.transform.position = new Vector3(rightScreenPos.x, rightScreenPos.y, 0f);
        crosshairSpreadx2.transform.position = new Vector3(leftScreenPos.x, leftScreenPos.y, 0f);
        crosshairSpread_v.transform.position = new Vector3(straightScreenPos.x, straightScreenPos.y + Mathf.Abs(rightScreenPos.x-straightScreenPos.x), 0f);
        crosshairSpreadx2_v.transform.position = new Vector3(straightScreenPos.x, straightScreenPos.y - Mathf.Abs(rightScreenPos.x-straightScreenPos.x), 0f);

        //Debug.Log("delta: " + ((oldX-leftScreenPos.x) * Time.deltaTime * 1000f).ToString());
        //oldX = leftScreenPos.x;
        //crosshair size
        float crosshairspreadDistance = Mathf.Sqrt(Mathf.Pow((rightScreenPos.x-straightScreenPos.x),2) + Mathf.Pow(rightScreenPos.y-straightScreenPos.y,2));

        cx = scaleFactor*crosshairspreadDistance;
        cx = Mathf.Clamp(cx, 0,0.2f);
        crossPos = new Vector3(cx,cx,1);

        //dont interpolate UI when just shot
        if(justShot){
            finalPos = crossPos;
            justShot = false;
        }else{
            finalPos = Vector3.Lerp(finalPos, crossPos, crossHairLerp * Time.deltaTime);
        }
        
        //set UI element width and height accordingly
        crosshairCircleRectTransformImproved.sizeDelta = new Vector2(2010f*finalPos.x * 10f, 2010f*finalPos.x*10f);
        
        //calculates acceleration
        acceleration = (vehicleController.VeL - lastVel) / Time.fixedDeltaTime;

        //show outline when aiming at tank
        Ray tankRay;
        tankRay = new Ray(_mainCamera.transform.position,  _mainCamera.transform.forward);
        if (Physics.Raycast(tankRay, out RaycastHit hit5, 5000f)){
            if(hit5.transform.root.gameObject.tag=="Shootable"){
                lastSelected = hit5.transform.root.gameObject;
                hit5.transform.root.gameObject.GetComponent<Outline>().enabled = true;
            }else{
                if(lastSelected!=null){
                    lastSelected.GetComponent<Outline>().enabled = false;
                }
            }
            //Debug.Log(hit5.transform.root.gameObject.name);
        }
    }

    private void FixedUpdate(){
        //CalculateDispersion();
    }

    private void CalculateDispersion(){
        //for dispersion calculation
        acceleration = (vehicleController.VeL - lastVel) / Time.fixedDeltaTime;
        //increase dispersion while accelerating, decrease while decelerating or almost still
        if(acceleration>1f){
            //gunDispersion = ((vehicleController.VeL / (vehicleController.maxSpeed * 3.6f)) * (maxDispersion-minDispersion))+minDispersion;
            gunDispersion += (vehicleController.VeL / (vehicleController.maxSpeed * 3.6f)) * dispersionIncreaseCoefficient * Time.deltaTime;
        }else if(acceleration < -1f || vehicleController.VeL<1f){
            float delta = gunDispersion - minDispersion;
            float adjD = 1 - ( delta/(maxDispersion-minDispersion) );
            //Debug.Log("adjD: " + adjD.ToString());
            float coeffAdjuster = 1.6f - (1.2f * adjD);
            delta = Mathf.Clamp(delta, 1f,1.1f);
            gunDispersion -= (aimSpeed * coeffAdjuster) * Time.deltaTime;
            //gunDispersion -= aimSpeed * Time.deltaTime;
        }
        //clam dispersion
        gunDispersion = Mathf.Clamp(gunDispersion,minDispersion,maxDispersion);
        //for acceleration calculations
        lastVel = vehicleController.VeL;
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
        //actually instantiates object
        GameObject bul = Instantiate(bullet, gunObject.transform.position, rot);

        //removes one from bullet amount
        bulletsOfT[selectedBullet]-=1;
        uicontroller.UpdateBulletCountUI();
        //plays sound
        shootAudioSource.Play();

        //add dispersion after shooting
        ShootDispersion();
    }

    private void ShootDispersion(){
        gunDispersion = shootingDispersion;
        justShot = true;
    }

    private Vector3 CrosshairCast(Vector3 pos, Vector3 dirVector){
        Ray crosshairCast;
        crosshairCast = new Ray(pos,  dirVector);
        Vector3 hitp = new Vector3();
        if (Physics.Raycast(crosshairCast, out RaycastHit hit, 5000f)){
            //Debug.DrawLine(crosshairCast.origin, hit.point, Color.yellow);
            hitp = hit.point;
        }
        return hitp;
    }
}
