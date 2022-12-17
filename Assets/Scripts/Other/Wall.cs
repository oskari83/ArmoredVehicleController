using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour{
    public GameObject tank;
    void Update(){
        transform.position = tank.transform.position;
    }
}
