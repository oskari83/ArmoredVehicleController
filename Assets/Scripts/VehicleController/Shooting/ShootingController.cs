using UnityEngine;
using System.Collections;

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
    public float reloadTime = 5f;
    public float reloadTimeLeft;
    public bool isReloading = false;
    public bool justShot = false;

	private VehicleControllerManager vehicleManager;
	private TankMovement tankMovementScript;

    private GameObject gunShootingPositionObject;
	private GameObject lastSelected;
    private Camera _mainCamera;

    private float rawrng;
    private float rawdirrng;
    private float scaleddirrng;
    private float scaledrng;
    private float oldX;
	private float tankMaximumVelocity;

    private int currentAmmunition = 0;
    private int maxAmmo = 1;

    private void Awake(){
		vehicleManager = GetComponent<VehicleControllerManager>();
		tankMovementScript = GetComponent<TankMovement>();

		gunShootingPositionObject = vehicleManager.gunShootingPositionGameObject;
		tankMaximumVelocity = tankMovementScript.maxSpeed;
        reloadTimeLeft = reloadTime;
    }

    private void Update(){
        // Refactor eventually into inputcontroller class
        if (Input.GetButton("Fire1") && currentAmmunition>0){
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

        if(!isReloading && currentAmmunition<=0){
            StartCoroutine(Reload());
        }
        if(isReloading){
            reloadTimeLeft-=Time.deltaTime;
            if(reloadTimeLeft<0f){
                reloadTimeLeft=0f;
            }
        }

        CalculateDispersion();
        EnableOutlineOnEnemy();
    }

    private void CalculateDispersion(){
        // Adds dispersion when moving relative to velocity
        float velocityProportion = vehicleManager.VelocityInKMH / (tankMaximumVelocity * 3.6f);
        float dispersionTarget = ((maxDispersion-minDispersion) * velocityProportion) + minDispersion;
        if (gunDispersion<(dispersionTarget)){
            gunDispersion = dispersionTarget;
        }

        // Deducts dispersion as a result of aiming
        float delta = gunDispersion - minDispersion;
        float adjD = 1 - ( delta/(maxDispersion-minDispersion) );
        //Debug.Log("adjD: " + adjD.ToString());
        float coeffAdjuster = 1.6f - (1.2f * adjD);
        gunDispersion -= (aimSpeed * coeffAdjuster) * Time.deltaTime;

        gunDispersion = Mathf.Clamp(gunDispersion,minDispersion,maxDispersion);
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
        // Removes one ammunition from "clip"
        currentAmmunition -= 1;

        // Invoke shootingEvent so that UI and Audio can function
        if(shootingEvent!=null){
            shootingEvent();
        }
    }

    private IEnumerator Reload(){
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        currentAmmunition = maxAmmo;
        isReloading = false;
        reloadTimeLeft = reloadTime;
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
