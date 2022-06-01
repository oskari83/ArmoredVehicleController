using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionScript : MonoBehaviour{
    private GameObject parent;

    private void Start(){
        parent = transform.parent.gameObject;
    }
    
    private void OnTriggerEnter(Collider other){
        Destroy(parent);
    }
}
