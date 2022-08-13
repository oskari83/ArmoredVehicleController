using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour{
    [Header("Bullet Speed")]
    public float bulletSpeed = 5f;
    public float lerpAmount = 5f;

    private float speed;
    private Vector3 pos,vel, newPos;

    private void Start(){
        pos = transform.position;
        newPos = pos;
    }

    private void FixedUpdate(){
        newPos = transform.position + (transform.forward*bulletSpeed);
        //transform.Translate(Vector3.forward * bulletSpeed);
        vel = (transform.position-pos) / Time.fixedDeltaTime;
        pos = transform.position;
    }

    private void Update(){
        transform.position = Vector3.Lerp(transform.position,newPos,Time.deltaTime*lerpAmount);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit2, 1000f)){
            Vector3 _pos = hit2.point;
            //Debug.Log(hit2.distance.ToString()+ " vel=" + vel.magnitude.ToString());
            //Debug.DrawLine(transform.position, hit2.point, Color.magenta);
            float dst = hit2.distance;
            dst *=dst;
            if(dst<(vel.sqrMagnitude/50f)){
                Debug.Log("hit something");
                Destroy(gameObject);
            }
        }
    }
}
