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
    public int bulletSwitchCheck = 0;

	[Header("Aiming and dispersion Attributes")]
	public float gunDispersion = 1f;
    public float minDispersion = 0.5f;
    public float maxDispersion = 5f;
    public float shootingDispersion = 4f;
    public float aimSpeed = 1f;

    [Header("Reload Attributes")]
    public bool hasClip = true;
    public float reloadTime = 5f;
    public float intraClipReloadTime = 2f;
    public float reloadTimeLeft;
    public int clipSize = 5;
    public int currentAmmunition = 0;
    public bool isReloading = false;
    public bool justShot = false;
    public bool finishedFullReload = false;

	private VehicleControllerManager vehicleManager;
	private TankMovement tankMovementScript;

    private GameObject gunShootingPositionObject;
	private GameObject lastSelected;
    private Camera _mainCamera;

    private float rawrng;
    private float rawdirrng;
    private float scaleddirrng;
    private float scaledrng;
	private float tankMaximumVelocity;

    private void Awake(){
		vehicleManager = GetComponent<VehicleControllerManager>();
		tankMovementScript = GetComponent<TankMovement>();

		gunShootingPositionObject = vehicleManager.gunShootingPositionGameObject;
		tankMaximumVelocity = tankMovementScript.maxSpeed;
        reloadTimeLeft = reloadTime;

        FullReload();
    }

    private void Update(){
        // Refactor eventually into inputcontroller class
        if (Input.GetButton("Fire1") && bulletCountOfType[selectedBullet] > 0 && currentAmmunition > 0 && !isReloading){
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            if (bulletSwitchCheck != selectedBullet && bulletSwitchCheck==0){
                SwitchBullets(0);
                bulletSwitchCheck = selectedBullet;
                return;
            }
            bulletSwitchCheck = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){
            if(bulletSwitchCheck != selectedBullet && bulletSwitchCheck == 1)
            {
                SwitchBullets(1);
                bulletSwitchCheck = selectedBullet;
                return;
            }
            bulletSwitchCheck = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){
            if(bulletSwitchCheck != selectedBullet && bulletSwitchCheck == 2){
                SwitchBullets(2);
                bulletSwitchCheck = selectedBullet;
                return;
            }
            bulletSwitchCheck = 2;
        }

        _mainCamera = vehicleManager.CameraInUse;

        if(isReloading){
            reloadTimeLeft -= Time.deltaTime;
            reloadTimeLeft = reloadTimeLeft < 0f ? 0f : reloadTimeLeft;
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
        // calculates percentage of dispersion i.e 0% = fully aimed, 100% = maximum dispersion, and then does 1 - that
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
                //Debug.Log("hit: " + lastSelected.name);
                hit5.transform.root.gameObject.GetComponent<QuickOutline>().enabled = true;
            }else{
                if(lastSelected!=null){
                    lastSelected.GetComponent<QuickOutline>().enabled = false;
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

        if(currentAmmunition <= 0){
            FullReload();
        }else{
            StartCoroutine(IntraClipReload());
        }
    }

    private void FullReload(){
        if(currentAmmunition > 0){
            currentAmmunition = 0;
        }
        StartCoroutine(Reload());
    }

    private IEnumerator Reload(){
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        if(bulletCountOfType[currentAmmunition] >= clipSize){
            currentAmmunition = clipSize;
        }else{
            currentAmmunition = bulletCountOfType[currentAmmunition];
        }

        isReloading = false;
        reloadTimeLeft = intraClipReloadTime;
        finishedFullReload = true;
    }

    private IEnumerator IntraClipReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(intraClipReloadTime);

        isReloading = false;
        if(currentAmmunition == 1){
            reloadTimeLeft = reloadTime;
        }else{
            reloadTimeLeft = intraClipReloadTime;
        }
    }

    private void SwitchBullets(int _type){
        selectedBullet = _type;

        FullReload();
        reloadTimeLeft = reloadTime;

        if (bulletSwitchEvent!=null){
            bulletSwitchEvent();
        }
    }

    private void ShootDispersion(){
        gunDispersion = shootingDispersion;
        justShot = true;
    }
}
