using UnityEngine;

public class BulletScript : MonoBehaviour{

    [Header("Bullet Speed")]
    public float bulletSpeed = 22f;
    public float lerpAmount = 5f;

    public GameObject hitPointImpactPrefab;

    private float oldDist;
    private float deltaDist;
    private Vector3 newPos;

    private void Start(){
        newPos = transform.position;
    }

    private void FixedUpdate(){
        newPos = transform.position + (transform.forward*bulletSpeed);
    }

    private void Update(){
        transform.position = Vector3.Lerp(transform.position,newPos,Time.deltaTime*lerpAmount);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit2, 1000f)){
            //Debug.DrawLine(transform.position, hit2.point, Color.magenta);

            float dst = hit2.distance;
            deltaDist = oldDist - dst;
            //Debug.Log("dist: " + hit2.distance.ToString() + "deltaDist: " + deltaDist.ToString());

            if(dst<= 2f * deltaDist){
                Destroy(gameObject);

                if(hit2.transform.root.gameObject.tag == "Shootable"){
                    GameObject tankObject = hit2.transform.root.gameObject;
          
                    Instantiate(hitPointImpactPrefab, hit2.point, hit2.transform.rotation, tankObject.transform);

                    float cosine = Vector3.Dot(transform.forward, hit2.normal);
                    float cosineDegrees = Mathf.Acos(cosine);
                    // Give hit angle where 0 means straight on, 90 means autobounce
                    float cleanedAngle = 180f - (cosineDegrees * Mathf.Rad2Deg);

                    tankObject.GetComponent<Armor>().TakeHit(100f, cleanedAngle, "Scorpion");

                    Debug.Log("Angle: " + (cleanedAngle).ToString());

                }
                //Debug.Log("hit something");
            }

            oldDist = dst;
        }
    }
}
