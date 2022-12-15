using UnityEngine;

public class TrackVisuals : MonoBehaviour{

	[Header("Track Settings")]
	public float trackThiccness = 0.1f;
	public float trackSpeed = 0.005f;
	public float roadWheelSpinMultiplier = 350.0f;
	public float dummyWheelSpinMultiplier = 150.0f;

	[Header("Gameobjects")]
	[SerializeField] private Transform[] leftWheelMeshes;
    [SerializeField] private Transform[] leftDummyWheelMeshes;
    [SerializeField] private Transform[] rightWheelMeshes;
    [SerializeField] private Transform[] rightDummyWheelMeshes;
    [SerializeField] private Transform[] leftTrackBones;
    [SerializeField] private Transform[] rightTrackBones;
    [SerializeField] private Transform leftTrack;
    [SerializeField] private Transform rightTrack;

	private TankMovement tankMovementScript;
	private WheelCollider[] leftWheelColliders;
    private WheelCollider[] rightWheelColliders;
	private Material leftTrackMaterial;
    private Material rightTrackMaterial;
	private Vector3 colliderPos;
	private Quaternion colliderRot;
	private Rigidbody _rigidbody;
	private float leftDirectionalMultiplier = 1.0f; 
    private float rightDirectionalMultiplier = 1.0f; 
    private int trackOffset;

	private void Awake(){
		leftTrackMaterial = leftTrack.GetComponent<Renderer>().material;
        rightTrackMaterial = rightTrack.GetComponent<Renderer>().material;
		tankMovementScript = GetComponent<TankMovement>();
		_rigidbody = this.GetComponent<Rigidbody>();
		leftWheelColliders = tankMovementScript.leftWheelColliders;
		rightWheelColliders = tankMovementScript.rightWheelColliders;
	}	

	private void LateUpdate(){
        SetTrackVisuals();
    }

	private void SetTrackVisuals(){
		//We set the wheel mesh and bones (track deform) positions to the positions of wheel colliders
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            leftWheelColliders[i].GetWorldPose(out colliderPos, out colliderRot);
            leftWheelMeshes[i].position = colliderPos + new Vector3 (0, trackThiccness, 0);
            leftTrackBones[i].position = leftWheelMeshes[i].position + transform.up * -1.0f * leftWheelColliders[i].radius;
        }

        for (int i = 0; i < rightWheelColliders.Length; i++) {
            rightWheelColliders[i].GetWorldPose(out colliderPos, out colliderRot);
            rightWheelMeshes[i].position = colliderPos + new Vector3 (0, trackThiccness, 0);
            rightTrackBones[i].position = rightWheelMeshes[i].position + transform.up * -1.0f * leftWheelColliders[i].radius;
		}

        //We rotate the wheels to simulate movement - I don't use the out rot from wheel collider because I want everything to spin at the same rate
        foreach (Transform wheelMesh in leftWheelMeshes) {
            wheelMesh.Rotate(leftDirectionalMultiplier * tankMovementScript.LocalZVelocity * trackSpeed * roadWheelSpinMultiplier, 0, 0, Space.Self);
        }
        foreach (Transform wheelMesh in leftDummyWheelMeshes) {
            wheelMesh.Rotate(leftDirectionalMultiplier * tankMovementScript.LocalZVelocity * trackSpeed * dummyWheelSpinMultiplier, 0, 0, Space.Self);
        }

        foreach (Transform wheelMesh in rightWheelMeshes) {
            wheelMesh.Rotate(rightDirectionalMultiplier * tankMovementScript.LocalZVelocity * trackSpeed * roadWheelSpinMultiplier, 0, 0, Space.Self);
        }
        foreach (Transform wheelMesh in rightDummyWheelMeshes) {
            wheelMesh.Rotate(rightDirectionalMultiplier * tankMovementScript.LocalZVelocity * trackSpeed * dummyWheelSpinMultiplier, 0, 0, Space.Self);
        }

        //We scroll the track texture to simulate movement
        leftTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, leftTrackMaterial.mainTextureOffset.y + (leftDirectionalMultiplier * -1.0f * _rigidbody.velocity.magnitude * trackSpeed * Mathf.Sign(tankMovementScript.LocalZVelocity))));
        rightTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, rightTrackMaterial.mainTextureOffset.y + (rightDirectionalMultiplier * -1.0f * _rigidbody.velocity.magnitude * trackSpeed * Mathf.Sign(tankMovementScript.LocalZVelocity))));
	}
}