using UnityEngine;

[RequireComponent(typeof(VehicleControllerManager))]
public class ShootingController : MonoBehaviour{

    public delegate void ShootingDelegate();
    public ShootingDelegate shootingEvent;

    public delegate void BulletSwitchDelegate();
    public BulletSwitchDelegate bulletSwitchEvent;

    [Header("Gameobjects")]
    public GameObject bullet;

	[Header("Bullet Attributes")]
    public int[] bulletCountOfType = new int [3] {5,4,3};
    public int selectedBullet = 0;

	[Header("Gun Attributes")]
	public float gunDispersion = 1f;
    public float minDispersion = 0.5f;
    public float maxDispersion = 5f;
    public float shootingDispersion = 4f;
    public float aimSpeed = 1f;
    public float dispersionIncreaseCoefficient = 40f;

    public bool justShot = false;

	private VehicleControllerManager vehicleManager;
	private TankMovement tankMovementScript;

    private GameObject gunShootingPositionObject;
	private GameObject lastSelected;
    private Camera _mainCamera;

    private float lastVel = 0f;
    private float acceleration;
    private float rawrng;
    private float rawdirrng;
    private float scaleddirrng;
    private float scaledrng;
    private float oldX;
	private float tankMaximumVelocity;

    private void Awake(){
		vehicleManager = GetComponent<VehicleControllerManager>();
		tankMovementScript = GetComponent<TankMovement>();

		gunShootingPositionObject = vehicleManager.gunShootingPositionGameObject;
		tankMaximumVelocity = tankMovementScript.maxSpeed;
    }

    private void Update(){
        // Refactor eventually into inputcontroller clas
        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }
        if (Input.GetKey(KeyCode.Alpha1)){
            SwitchBullets(0);
        }
        if (Input.GetKey(KeyCode.Alpha2)){
            SwitchBullets(1);
        }
        if (Input.GetKey(KeyCode.Alpha3)){
            SwitchBullets(2);
        }

        _mainCamera = vehicleManager.CameraInUse;

        CalculateDispersion();
        EnableOutlineOnEnemy();
    }

    private void FixedUpdate(){
        //CalculateDispersion();
    }

    private void CalculateDispersion(){
        // For dispersion calculation
        acceleration = (vehicleManager.VelocityInKMH - lastVel) / Time.fixedDeltaTime;

        // Increase dispersion while accelerating, decrease while decelerating or almost still
        if(acceleration>1f){
            //gunDispersion = ((vehicleManager.VelocityInKMH / (tankMaximumVelocity * 3.6f)) * (maxDispersion-minDispersion))+minDispersion;
            gunDispersion += (vehicleManager.VelocityInKMH / (tankMaximumVelocity * 3.6f)) * dispersionIncreaseCoefficient * Time.deltaTime;
        }else if(acceleration < -1f || vehicleManager.VelocityInKMH<1f){
            float delta = gunDispersion - minDispersion;
            float adjD = 1 - ( delta/(maxDispersion-minDispersion) );
            //Debug.Log("adjD: " + adjD.ToString());
            float coeffAdjuster = 1.6f - (1.2f * adjD);
            delta = Mathf.Clamp(delta, 1f,1.1f);
            gunDispersion -= (aimSpeed * coeffAdjuster) * Time.deltaTime;
            //gunDispersion -= aimSpeed * Time.deltaTime;
        }

        gunDispersion = Mathf.Clamp(gunDispersion,minDispersion,maxDispersion);
        lastVel = vehicleManager.VelocityInKMH;
    }

    private void EnableOutlineOnEnemy(){
        // Show outline when aiming at tank
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
        }
    }

    private void Shoot(){
        // Implements accuracy rng by getting first distance from center and then angle around unit circle
        rawrng = Random.value;
        rawdirrng = Random.value;
        scaleddirrng = 2 * Mathf.PI * rawdirrng;
        float xval = Mathf.Cos(scaleddirrng);
        float yval = Mathf.Sin(scaleddirrng);

        // Do y = a * x^3 so that extreme values are exponentially more rare, determines how accuracy is distributed
        // Basically a pdf-function
        scaledrng = gunDispersion * (rawrng*rawrng*rawrng);
        //Debug.Log("raw rng:" + rawrng + " scl rng:" + scaledrng);
        xval = xval * scaledrng;
        yval = yval * scaledrng;

        // Shoots relative to our gun rotation if we are on a hill etc.
        Quaternion spreadOnX = Quaternion.AngleAxis(xval, Vector3.up);
        Quaternion spreadOnY = Quaternion.AngleAxis(yval, Vector3.left);
        spreadOnX *=spreadOnY;
        Quaternion rot = gunShootingPositionObject.transform.rotation;
        rot *= spreadOnX;
        GameObject bul = Instantiate(bullet, gunShootingPositionObject.transform.position, rot);

        // Adds dispersion after shooting
        ShootDispersion();
        // Removes one from bullet amount
        bulletCountOfType[selectedBullet]-=1;

        // Invoke shootingEvent so that UI and Audio can function
        if(shootingEvent!=null){
            shootingEvent();
        }
    }

    private void SwitchBullets(int type){
        selectedBullet = type;
        if(bulletSwitchEvent!=null){
            bulletSwitchEvent();
        }
    }

    private void ShootDispersion(){
        gunDispersion = shootingDispersion;
        justShot = true;
    }
}
