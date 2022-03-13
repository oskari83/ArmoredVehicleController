using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnController : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Transform spawnPoint;

    void Update() {
        if (Input.GetKeyUp("r"))
            ResetPlayerPosition();
    }

    public void ResetPlayerPosition() {
        if (player != null) {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            
            if (rb) {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (spawnPoint != null) {  
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;

            } else {
                player.transform.position = new Vector3(0, 5.0f, 0);
                player.transform.rotation = Quaternion.identity;
            }
        }
    }
}
