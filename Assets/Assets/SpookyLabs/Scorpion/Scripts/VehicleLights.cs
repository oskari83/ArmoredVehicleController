using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleLights : MonoBehaviour
{
    [SerializeField]
    private GameObject[] lights;

    [SerializeField]
    private GameObject[] lightMeshes;

    private void Start() {
        foreach (GameObject l in lights) {
            l.SetActive(false);
        }

        foreach (GameObject mesh in lightMeshes) {
            mesh.SetActive(false);
        }
    }

    void Update() {
        if (Input.GetKeyUp("l"))
            ToggleLights();
    }

    public void ToggleLights() {
        foreach (GameObject l in lights) {
            l.SetActive(!l.activeSelf);
        }

        foreach (GameObject mesh in lightMeshes) {
            mesh.SetActive(!mesh.activeSelf);
        }
    }
}
