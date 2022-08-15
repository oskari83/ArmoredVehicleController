using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class ShootingController : MonoBehaviour{

    [Header("Bullet Prefab")]
    public GameObject bullet;

    private VehicleController vehicleController;
    private GameObject gunObject;
    private GameObject gun2;

    public GameObject crosshairSpread;
    public GameObject crosshairSpreadx2;

    public float fx;
    public float fy;
    public float fz;

    private void Start(){
        vehicleController = gameObject.GetComponent<VehicleController>();
        gunObject = vehicleController.gunShootPos;
        gun2 = vehicleController.gunGO;
    }

    private void Update(){
        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }

        // .25 degrees left and right
        Vector3 aimCircle = gunObject.transform.forward;
        //Quaternion spreadAngle = Quaternion.AngleAxis(0.25f, new Vector3(0, 1, 0));
        //Quaternion spreadAnglex2 = Quaternion.AngleAxis(-0.25f, new Vector3(0, 1, 0));
        Quaternion spreadAngle = Quaternion.AngleAxis(0.25f, gunObject.transform.up);
        Quaternion spreadAnglex2 = Quaternion.AngleAxis(-0.25f, gunObject.transform.up);
        Vector3 newVector = spreadAngle * aimCircle;
        Vector3 newVector2 = spreadAnglex2 * aimCircle;
        //to the right
        Ray crossHairRay2;
        crossHairRay2 = new Ray(gunObject.transform.position,  newVector);
        Vector3 rayHitPoint = new Vector3();
        if (Physics.Raycast(crossHairRay2, out RaycastHit hit,5000f)){
            //Debug.DrawLine(crossHairRay2.origin, hit.point, Color.yellow);
            rayHitPoint = hit.point;
        }
        //to the left
        Ray crossHairRay3;
        crossHairRay3 = new Ray(gunObject.transform.position,  newVector2);
        Vector3 rayHitPoint3 = new Vector3();
        if (Physics.Raycast(crossHairRay3, out RaycastHit hit3,5000f)){
            //Debug.DrawLine(crossHairRay3.origin, hit3.point, Color.yellow);
            rayHitPoint3 = hit3.point;
        }

        Camera _mainCamera = vehicleController.cameraInUse;
        crosshairSpread.transform.position = _mainCamera.WorldToScreenPoint(rayHitPoint);
        crosshairSpreadx2.transform.position = _mainCamera.WorldToScreenPoint(rayHitPoint3);
    }

    private void Shoot(){
        //Vector3 shootDirection = gunObject.transform.rotation.eulerAngles;
        //Vector3 aimCircle = gunObject.transform.forward;
        //Vector3 rot = new Vector3(shootDirection.x,shootDirection.y+0.25f,shootDirection.z);
        //Quaternion bulletRot = Quaternion.Euler(rot);
        //Vector3 aimCircle4 = gunObject.transform.forward;
        Quaternion spreadAngle4 = Quaternion.AngleAxis(0.25f, Vector3.up);
        Quaternion rot = gunObject.transform.rotation;
        rot *= spreadAngle4;
        //Vector3 xx = spreadAngle4 * aimCircle;
        //Quaternion ans = Quaternion.Euler(xx);
        GameObject bul = Instantiate(bullet, gunObject.transform.position, rot);
    }
}
