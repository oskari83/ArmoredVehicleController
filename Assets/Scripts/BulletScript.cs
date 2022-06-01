using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour{
    [Header("Bullet Speed")]
    public float bulletSpeed = 5f;

    private Rigidbody child;

    private void Start(){
        child = gameObject.transform.GetChild(0).GetComponent<Rigidbody>();
    }

    private void FixedUpdate(){
        //child.MovePosition(transform.position + Vector3.forward * bulletSpeed);
        transform.Translate(Vector3.forward * bulletSpeed);
    }
}
