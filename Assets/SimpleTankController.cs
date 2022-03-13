using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTankController : MonoBehaviour
{
	[SerializeField]
	public WheelCollider[] leftWheelColliders;
    
    [SerializeField]
    public WheelCollider[] rightWheelColliders;
	
	[SerializeField]
	public Transform[] leftWheelMeshes;

    [SerializeField]
    public Transform[] leftDummyWheelMeshes;

    [SerializeField]
    public Transform[] rightWheelMeshes;

    [SerializeField]
    public Transform[] rightDummyWheelMeshes;

    [SerializeField]
    public Transform[] leftTrackBones;

    [SerializeField]
    public Transform[] rightTrackBones;
	
    [SerializeField]
    public Transform leftTrack;

    [SerializeField]
    public Transform rightTrack;

	[SerializeField]
    public float trackThiccness = 0.14f;

    [SerializeField]
    private float trackSpeed = 0.005f;

    [SerializeField]
    public float centerOfMassYOffset = -1.0f;

    [SerializeField]
    public float driveSpeed = 100.0f;

    [SerializeField]
    public float turnSpeed = 250.0f;

    [SerializeField]
    public float brakeStrength = 2500.0f;

    [SerializeField]
    public float roadWheelSpinMultiplier = 250.0f;

    [SerializeField]
    public float dummyWheelSpinMultiplier = 150.0f;

    [SerializeField]
    public ParticleSystem[] groundFxs; 

    [SerializeField]
    public AudioSource engineAudioSource;

    [SerializeField]
    public float maxPitch;

    [SerializeField]
    public float idlePitch;

	private Vector3 colliderPos;
	private Quaternion colliderRot;
    private float driveInput;
    private float turnInput;
    private bool brakeInput;
    private int trackOffset;
    private Rigidbody rigidBody;
    private Material leftTrackMaterial;
    private Material rightTrackMaterial;
    private float localZVelocity;
    private float leftDirectionalMultiplier = 1.0f; 
    private float rightDirectionalMultiplier = 1.0f; 
    private float curPitch = 0.5f;

    void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        leftTrackMaterial = leftTrack.GetComponent<Renderer>().material;
        rightTrackMaterial = rightTrack.GetComponent<Renderer>().material;
        SetGroundFxsState(false);
    }

    private void SetGroundFxsState(bool state) {
        foreach (ParticleSystem fx in groundFxs) {
            ParticleSystem.EmissionModule eMod = fx.emission;
            eMod.enabled = state;
        }
    }

    private void SetLeftTrackTorque(float speed) {
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            leftWheelColliders[i].motorTorque = speed;
        }
    }

    private void SetRightTrackTorque(float speed) {
        for (int i = 0; i < rightWheelColliders.Length; i++) {
            rightWheelColliders[i].motorTorque = speed;
        }
    }

    public void SetBrakes(float strength) {
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            leftWheelColliders[i].brakeTorque = strength;
        }

        for (int i = 0; i < rightWheelColliders.Length; i++) {
            rightWheelColliders[i].brakeTorque = strength;
        }
    }

    void Update() {
        driveInput = Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1);
        turnInput = Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1);
        brakeInput = Input.GetKey("space");
    }

    void FixedUpdate() {
        this.transform.GetComponent<Rigidbody>().centerOfMass = new Vector3(0, centerOfMassYOffset, 0);
        localZVelocity = transform.InverseTransformDirection(rigidBody.velocity).z;

        SetLeftTrackTorque(driveInput * driveSpeed);
        SetRightTrackTorque(driveInput * driveSpeed);

        if (turnInput != 0) {
            SetLeftTrackTorque(turnInput * turnSpeed);
            SetRightTrackTorque(-1.0f * turnInput * turnSpeed);
        }

        if (brakeInput) SetBrakes(brakeStrength);
        else SetBrakes(0);
    }

    void LateUpdate()
    {
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

        //We need to give some more push to the track speed if we're rotating in place

        if (rigidBody.velocity.magnitude <= 0.5) {
            if (turnInput != 0) {
                leftDirectionalMultiplier = -25.0f;
                rightDirectionalMultiplier = 25.0f;
            }
            else {
                leftDirectionalMultiplier = 1.0f;
                rightDirectionalMultiplier = 1.0f;
            }
        }
        else {
            leftDirectionalMultiplier = 1.0f;
            rightDirectionalMultiplier = 1.0f;
        }

        //We rotate the wheels to simulate movement - I don't use the out rot from wheel collider because I want everything to spin at the same rate
        foreach (Transform wheelMesh in leftWheelMeshes) {
            wheelMesh.Rotate(leftDirectionalMultiplier * localZVelocity * trackSpeed * roadWheelSpinMultiplier, 0, 0, Space.Self);
        }
        foreach (Transform wheelMesh in leftDummyWheelMeshes) {
            wheelMesh.Rotate(leftDirectionalMultiplier * localZVelocity * trackSpeed * dummyWheelSpinMultiplier, 0, 0, Space.Self);
        }

        foreach (Transform wheelMesh in rightWheelMeshes) {
            wheelMesh.Rotate(rightDirectionalMultiplier * localZVelocity * trackSpeed * roadWheelSpinMultiplier, 0, 0, Space.Self);
        }
        foreach (Transform wheelMesh in rightDummyWheelMeshes) {
            wheelMesh.Rotate(rightDirectionalMultiplier * localZVelocity * trackSpeed * dummyWheelSpinMultiplier, 0, 0, Space.Self);
        }

        //We scroll the track texture to simulate movement
        leftTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, leftTrackMaterial.mainTextureOffset.y + (leftDirectionalMultiplier * -1.0f * rigidBody.velocity.magnitude * trackSpeed * Mathf.Sign(localZVelocity))));
        rightTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, rightTrackMaterial.mainTextureOffset.y + (rightDirectionalMultiplier * -1.0f * rigidBody.velocity.magnitude * trackSpeed * Mathf.Sign(localZVelocity))));
   
        //We toggle the ground dust particle effect depending on our speed
        if (rigidBody.velocity.magnitude > 4) {
            SetGroundFxsState(true);
        }
        else SetGroundFxsState(false);

        curPitch = Mathf.Clamp(idlePitch + rigidBody.velocity.magnitude / 40.0f, idlePitch, maxPitch);
        engineAudioSource.pitch = curPitch;
    }
}
