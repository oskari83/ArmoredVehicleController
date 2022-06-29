using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour{
    [Header("Bullet Speed")]
    public float bulletSpeed = 5f;

    private void FixedUpdate(){
        transform.Translate(Vector3.forward * bulletSpeed);
    }
}
