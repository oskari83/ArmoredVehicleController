using UnityEngine;

public class BulletScript : MonoBehaviour{

    [Header("Bullet Speed")]
    public float bulletSpeed = 22f;
    public float lerpAmount = 5f;

    private float oldDist;
    private float deltaDist;
    private Vector3 newPos;

    private void FixedUpdate(){
        newPos = transform.position + (transform.forward*bulletSpeed);
    }

    private void Update(){
        transform.position = Vector3.Lerp(transform.position,newPos,Time.deltaTime*lerpAmount);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit2, 1000f)){
            Vector3 _pos = hit2.point;
            //Debug.DrawLine(transform.position, hit2.point, Color.magenta);

            float dst = hit2.distance;
            deltaDist = oldDist - dst;
            //Debug.Log("dist: " + hit2.distance.ToString() + "deltaDist: " + deltaDist.ToString());

            if(dst<= 2f * deltaDist){
                Destroy(gameObject);
                //Debug.Log("hit something");
            }

            oldDist = dst;
        }
    }
}
