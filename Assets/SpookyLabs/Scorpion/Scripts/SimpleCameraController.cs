using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [Header("Camera Variables")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float xSpeed = 30f;
    [SerializeField] private float ySpeed = 30f;
    [SerializeField] private float yMinLimit = -18f;
    [SerializeField] private float yMaxLimit = 30f;
    [SerializeField] private float distanceMin = 2f;
    [SerializeField] private float distanceMax = 4f;
    [SerializeField] private float scrollSpeed = 20f;

    private float x = 0.0f;
    private float y = 0.0f;
    private Transform target;
    private Rigidbody requiredRigidbody;

    public GameObject targetGO;

    private void Start(){
        //gets the real angles as shown in inspector and sets x and y variables as such
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        //gets the rigidbody
        requiredRigidbody = GetComponent<Rigidbody>();
        //makes the rigid body not change rotation
        if (requiredRigidbody != null){
            requiredRigidbody.freezeRotation = true;
        }
    }

    private void LateUpdate(){
        target = targetGO.transform;
        
        if (target){
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            //sets the rotation as mouse x and y
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            //sets distance based on scrollwheel while setting a min and max
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollSpeed, distanceMin, distanceMax);
            //gets hit distance
            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit)){
                distance -= hit.distance;
            }
            //apply position to camera
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;
            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max){
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
