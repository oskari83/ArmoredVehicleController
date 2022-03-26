using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class ShootingController : MonoBehaviour{

    [Header("Bullet Prefab")]
    public GameObject bullet;

    private VehicleController vehicleController;
    private GameObject gunObject;

    private void Start(){
        vehicleController = gameObject.GetComponent<VehicleController>();
        gunObject = vehicleController.gunShootPos;
    }

    private void Update(){
        if (Input.GetButtonDown("Fire1")){
            Shoot();
        }
    }

    private void Shoot(){
        GameObject bul = Instantiate(bullet, gunObject.transform.position,  gunObject.transform.rotation);
    }
}
