using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour{

	[Header("Crosshair Gameobject")]
	public GameObject crossHair;

    [Header("Crosshair Properties")]
    public float crossHairLerp = 10f;

	[Header("Crosshair Dispersion Gameobjects")]
    public RectTransform crosshairCircleRectTransformImproved;
	public GameObject crosshairSpread;
    public GameObject crosshairSpreadx2;
    public GameObject crosshairSpread_v;
    public GameObject crosshairSpreadx2_v;

    [Header("Crosshair TD Mode Gameobjects")]
	public GameObject crosshairTDLimit;
    public GameObject crosshairTDLimit2;

	private VehicleControllerManager vehicleManager;
	private ShootingController shootingController;
	private Camera cameraInUse;
	private Vector3 finalCrosshairPos;
    private Vector3 crossPos;
    private Vector3 finalPos;
    private GameObject _gunShootingPositionObject;
    private GameObject _gunGameObject;
    private GameObject _gunRigGunObject;

    private Image crosshairSpreadLeftImage;
    private Image crosshairSpreadRightImage;
    private Image crosshairSpreadUpImage;
    private Image crosshairSpreadDownImage;

    private float SCALEFACTOR = 0.018f / 16.557f;
    private float cx;

    private bool hasTurret = true;
    private float tdModeMaxYValue;

	private void Awake(){
		vehicleManager = GetComponent<VehicleControllerManager>();
		shootingController = GetComponent<ShootingController>();
        _gunShootingPositionObject = vehicleManager.gunShootingPositionGameObject;
        _gunGameObject = vehicleManager.tankGunGameObject;

        crosshairSpreadLeftImage = crosshairSpread.GetComponent<Image>();
        crosshairSpreadRightImage = crosshairSpreadx2.GetComponent<Image>();
        crosshairSpreadUpImage = crosshairSpread_v.GetComponent<Image>();
        crosshairSpreadDownImage = crosshairSpreadx2_v.GetComponent<Image>();
	}

    private void Start(){
        hasTurret = vehicleManager.TurretMovementScript.hasTurret;
        tdModeMaxYValue = vehicleManager.TurretMovementScript.maxGunHorizontalRotationAngle;
        _gunRigGunObject = vehicleManager.cameraController.sniperRigGun;
    }

    private void Update(){
        cameraInUse = vehicleManager.CameraInUse;

		MoveCrosshair();
		MoveDispersionCrosshairs();
        // expensive, try to solve getting this in update through 2 references
        if(!hasTurret && vehicleManager.cameraController.inSniperMode) {
            crosshairTDLimit.SetActive(true);
            crosshairTDLimit2.SetActive(true);
            MoveTDLimitCrosshairs();
        } else {
            crosshairTDLimit.SetActive(false);
            crosshairTDLimit2.SetActive(false);
        }
        DisableCrosshairsIfNotInSight();

        if(shootingController.isReloading){
            crosshairSpreadLeftImage.color = UIColours.blackSelected;
            crosshairSpreadRightImage.color = UIColours.blackSelected;
            crosshairSpreadUpImage.color = UIColours.blackSelected;
            crosshairSpreadDownImage.color = UIColours.blackSelected;
        }else{
            crosshairSpreadLeftImage.color = UIColours.blue;
            crosshairSpreadRightImage.color = UIColours.blue;
            crosshairSpreadUpImage.color = UIColours.blue;
            crosshairSpreadDownImage.color = UIColours.blue;
        }
	}

	private void MoveCrosshair(){
        float _maxCrosshairDistance = 10000f;
        Vector3 _forwardGun = _gunGameObject.transform.position + (_gunGameObject.transform.forward * _maxCrosshairDistance);

        // Check for obstacles in front of the cannon
        Ray crossHairRay;
        crossHairRay = new Ray(_gunGameObject.transform.position,  _gunGameObject.transform.forward);
        if (Physics.Raycast(crossHairRay, out RaycastHit hit, _maxCrosshairDistance)){
            _forwardGun = hit.point;
            // Debug.DrawLine(crossHairRay.origin, hit.point, Color.green);
        }

        // Placing the target's sprite in front of the cannon
        Quaternion _lookToHit = Quaternion.LookRotation(_forwardGun - cameraInUse.transform.position);
        Vector3 _crosshairPos = cameraInUse.transform.position + (_lookToHit * Vector3.forward);

        // Convert the world position of the crosshairs to the position of the screen (smoothed)
        finalCrosshairPos = Vector3.Lerp(finalCrosshairPos, cameraInUse.WorldToScreenPoint(_crosshairPos), Time.deltaTime * 20f);
        // finalCrosshairPos = cameraInUse.WorldToScreenPoint(_crosshairPos);
        crossHair.transform.position = finalCrosshairPos;
    }

    private void MoveTDLimitCrosshairs(){
        float gunZvalue = _gunRigGunObject.transform.localEulerAngles.y;
        if(gunZvalue > 300) {
            gunZvalue -= 360f;
        }
        float gunAdjustedZValueLeft;
        float gunAdjustedZValueRight;
        if(gunZvalue>-tdModeMaxYValue - 5f && gunZvalue < tdModeMaxYValue + 5f){
            // TODO adjust the 11f scale number to take into account zoom level to improve how the UI functions
            gunAdjustedZValueLeft = 11f * (tdModeMaxYValue - gunZvalue);
            gunAdjustedZValueRight = 11f * (-tdModeMaxYValue - gunZvalue);
        }else{
            gunAdjustedZValueLeft = 1000f; //just to get it out of sight
            gunAdjustedZValueRight = 1000f;
        }

        Debug.Log("horizontal td angle" + gunAdjustedZValueRight.ToString());
        Vector3 tdLimitCrosshairPos = new Vector3(Screen.width/2 + gunAdjustedZValueLeft, Screen.height/2, 0f);
        Vector3 tdLimitCrosshairPos2 = new Vector3(Screen.width/2 + gunAdjustedZValueRight, Screen.height/2, 0f);
        crosshairTDLimit.transform.position = tdLimitCrosshairPos;
        crosshairTDLimit2.transform.position = tdLimitCrosshairPos2;
    }

	private void MoveDispersionCrosshairs(){
        float currentDispersion = shootingController.gunDispersion;
        // Get gun current dispersion in vectors
        Vector3 aimCircle = _gunShootingPositionObject.transform.forward;
        Quaternion spreadAngle = Quaternion.AngleAxis(currentDispersion, _gunShootingPositionObject.transform.up);
        Quaternion spreadAnglex2 = Quaternion.AngleAxis(-currentDispersion, _gunShootingPositionObject.transform.up);
        Vector3 newVector = spreadAngle * aimCircle;
        Vector3 newVector2 = spreadAnglex2 * aimCircle;

        // Raycast to get hitpoint
        Vector3 ray25right = CrosshairCast(_gunShootingPositionObject.transform.position, newVector);
        Vector3 ray25left = CrosshairCast(_gunShootingPositionObject.transform.position, newVector2);
        Vector3 rayStraight = CrosshairCast(_gunShootingPositionObject.transform.position, aimCircle);

        // Get world to screenpoint for UI
        Vector3 leftScreenPos = cameraInUse.WorldToScreenPoint(ray25left);
        Vector3 rightScreenPos = cameraInUse.WorldToScreenPoint(ray25right);
        Vector3 straightScreenPos = cameraInUse.WorldToScreenPoint(rayStraight);
		Vector3 crosshairSpreadPositionLeft = new Vector3(rightScreenPos.x, rightScreenPos.y, 0f);
		Vector3 crosshairSpreadPositionRight = new Vector3(leftScreenPos.x, leftScreenPos.y, 0f);
		Vector3 crosshairSpreadPositionUp = new Vector3(straightScreenPos.x, straightScreenPos.y + Mathf.Abs(rightScreenPos.x-straightScreenPos.x), 0f);
		Vector3 crosshairSpreadPositionDown = new Vector3(straightScreenPos.x, straightScreenPos.y - Mathf.Abs(rightScreenPos.x-straightScreenPos.x), 0f);

        crosshairSpread.transform.position = crosshairSpreadPositionLeft;
        crosshairSpreadx2.transform.position = crosshairSpreadPositionRight;
        crosshairSpread_v.transform.position = crosshairSpreadPositionUp;
        crosshairSpreadx2_v.transform.position = crosshairSpreadPositionDown;

        // Debug system for finding circlecrosshair positions
        //Debug.Log("delta: " + ((oldX-leftScreenPos.x) * Time.deltaTime * 1000f).ToString());
        //oldX = leftScreenPos.x;

        // Crosshair size calculation using pythagoras
        float crosshairspreadDistance = Mathf.Sqrt(Mathf.Pow((rightScreenPos.x-straightScreenPos.x),2) + Mathf.Pow(rightScreenPos.y-straightScreenPos.y,2));

        // Use scalefactor to convert to whatever crosshair we are using
        cx = SCALEFACTOR*crosshairspreadDistance;
        // Clamp so that crosshair size never becomes overpoweringly big
        cx = Mathf.Clamp(cx, 0,0.2f);
        crossPos = new Vector3(cx,cx,1);

        // Don't interpolate UI when just shot, otherwise interpolate crosshair size change to make it smoother
        if(shootingController.justShot){
            finalPos = crossPos;
            shootingController.justShot = false;
        }else{
            finalPos = Vector3.Lerp(finalPos, crossPos, crossHairLerp * Time.deltaTime);
        }
        
		Vector2 crosshairSpreadCirclePosition = new Vector2(2010f*finalPos.x * 10f, 2010f*finalPos.x*10f);
        crosshairCircleRectTransformImproved.sizeDelta = crosshairSpreadCirclePosition;
	}

    private void DisableCrosshairsIfNotInSight(){
        // Disable the sight if the is not in front of the camera
        float _angleBetweenGunAndCamera = Mathf.Abs(Vector3.Angle(_gunGameObject.transform.forward, cameraInUse.transform.forward));

        // CrossHair.GetComponent<Image>().enabled = _angleBetweenGunAndCamera > cameraInUse.fieldOfView ? false : true;
        if(_angleBetweenGunAndCamera > cameraInUse.fieldOfView){
            crossHair.SetActive(false);
            crosshairSpread.SetActive(false);
            crosshairSpreadx2.SetActive(false);
            crosshairSpread_v.SetActive(false);
            crosshairSpreadx2_v.SetActive(false);
        }else{
            crossHair.SetActive(true);
            crosshairSpread.SetActive(true);
            crosshairSpreadx2.SetActive(true);
            crosshairSpread_v.SetActive(true);
            crosshairSpreadx2_v.SetActive(true);
        }
    }

    private Vector3 CrosshairCast(Vector3 pos, Vector3 dirVector){
        Ray crosshairCast;
        crosshairCast = new Ray(pos, dirVector);
        Vector3 hitp = new Vector3();
        if (Physics.Raycast(crosshairCast, out RaycastHit hit, 5000f)){
            //Debug.DrawLine(crosshairCast.origin, hit.point, Color.yellow);
            hitp = hit.point;
        }
        return hitp;
    }
}