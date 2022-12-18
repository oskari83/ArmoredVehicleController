using UnityEngine;
using System;

public class TankMovement : MonoBehaviour{

    public GameObject leftTrackForceObject;
    public GameObject rightTrackForceObject;
    public float turningForceCoefficient = 1f;
    public float forwardForceCoefficient = 1f;

	[Header("Tank Power Settings")]
    public float turnTorque = 1f;
    public float driveTorque = 2f;
    public float brakeStrength = 3f;
    public float maxRot = 0.75f;
    public float maxSpeed = 20f;

	[Header("Tank Turning Friction")]
    public float movementSidewaysFriction = 2.2f;
    public float stillSidewaysFriction = 0.8f;
	
	[Header("Wheel Colliders")]
	public WheelCollider[] leftWheelColliders;
    public WheelCollider[] rightWheelColliders;
	
	[Header("Tank Physical Settings")]
	public float centerOfMassYOffset = -1.0f;
    public float maxHeightStillGrounded = 1.15f;

	private float lastAngle;
    private float curAngle;
	private bool grounded;

    private WheelFrictionCurve[] sFrictionLeft;
    private WheelFrictionCurve[] sFrictionRight;
	private Rigidbody rigidBody;
	private InputController inputs;

	[Header("Visualised Inspector Data")]
	[SerializeField] private float angularVelocity;
	[SerializeField] private float velocityInKMH;
	[SerializeField] private float height;

	private void Awake(){
		rigidBody = this.GetComponent<Rigidbody>();
		rigidBody.centerOfMass = new Vector3(0, centerOfMassYOffset, 0);
		inputs = GetComponent<InputController>();

		sFrictionLeft = new WheelFrictionCurve[leftWheelColliders.Length];
        sFrictionRight = new WheelFrictionCurve[rightWheelColliders.Length];

        for (int i = 0; i < leftWheelColliders.Length; i++) {
            sFrictionLeft[i] = leftWheelColliders[i].sidewaysFriction;
            sFrictionLeft[i].stiffness = stillSidewaysFriction;
            leftWheelColliders[i].sidewaysFriction = sFrictionLeft[i];
        }

        for (int i = 0; i < rightWheelColliders.Length; i++) {
            sFrictionRight[i] = rightWheelColliders[i].sidewaysFriction;
            sFrictionRight[i].stiffness = stillSidewaysFriction;
            rightWheelColliders[i].sidewaysFriction = sFrictionRight[i];
        }

		turnTorque = turnTorque * 1000000f;
        driveTorque = driveTorque * 100f;
        brakeStrength = brakeStrength * 1000f;

        //rigidBody.maxAngularVelocity = maxRot;
	}

    private void Update(){
        // Sets angular and km/h velocity values to variables
		GetVelocities();
    }

	private void FixedUpdate(){
		// Sets boolean to indicate whether we are on the ground
		GetGroundedStatus();
		// Physically moves the tank
        //MoveTank();
        MoveTank2();
        // Limit our max velocity and angularvelocity
        //LimitMaximumVelocities();
        // Get our current climb angle
        GetClimbAngle();

        //AdjustDrag();

        lastAngle = curAngle;
	}

	private void LateUpdate(){
		SetWheelColliderFriction();
	}

    private void MoveTank2(){

        float localZVelocity = transform.InverseTransformDirection(rigidBody.velocity).z;

        float normalizedRotationVelocity = rigidBody.angularVelocity.y / turningForceCoefficient;
        normalizedRotationVelocity = Math.Abs(normalizedRotationVelocity);
        if (normalizedRotationVelocity>1){
            normalizedRotationVelocity=1;
        }
        float dragTurnCoefficient = 1 - normalizedRotationVelocity;

        float velocityInDirection = Vector3.Dot(rigidBody.velocity, transform.forward);
        float angularVelocityInDirection = Vector3.Dot(rigidBody.angularVelocity, transform.up);

        float normalizedVelocity = velocityInDirection / forwardForceCoefficient;
        normalizedVelocity = Math.Abs(normalizedVelocity);
        if (normalizedVelocity>1){
            normalizedVelocity=1;
        }
        float dragCoefficient = 1 - normalizedVelocity;

        if(grounded){
            if(inputs.DriveInput<0){
                rigidBody.AddTorque(transform.up * -inputs.TurnInput * turnTorque * dragTurnCoefficient * Time.fixedDeltaTime);
            }else{
                rigidBody.AddTorque(transform.up * inputs.TurnInput * turnTorque * dragTurnCoefficient * Time.fixedDeltaTime);
            }

            if(inputs.TurnInput==0){
                // For responsive feel so that tank doest continue turning after input
                rigidBody.AddTorque(transform.up * turnTorque * Time.fixedDeltaTime * -angularVelocityInDirection);
            }

            if(inputs.DriveInput!=0f){
                if(inputs.DriveInput<0f && localZVelocity>0f){
                    rigidBody.AddForce(transform.forward * driveTorque * Time.fixedDeltaTime * -velocityInDirection * 2000f);
                }else if(inputs.DriveInput>0f && localZVelocity<0f){
                    rigidBody.AddForce(transform.forward * driveTorque * Time.fixedDeltaTime * -velocityInDirection * 2000f);
                }
                SetLeftTrackTorque(inputs.DriveInput * driveTorque * dragCoefficient);
                SetRightTrackTorque(inputs.DriveInput * driveTorque * dragCoefficient);
            }else if(inputs.TurnInput!=0f){
                SetLeftTrackTorque(0.01f * driveTorque);
                SetRightTrackTorque(0.01f * driveTorque);
                rigidBody.AddForce(transform.forward * driveTorque * Time.fixedDeltaTime * -velocityInDirection * 1000f);
            }else{
                SetLeftTrackTorque(0f);
                SetRightTrackTorque(0f);
                rigidBody.AddForce(transform.forward * driveTorque * Time.fixedDeltaTime * -velocityInDirection * 1000f);
            }
        }
    }

	private void MoveTank(){
		// Only turn if we are on the ground
        if(grounded){
            if(inputs.DriveInput<0){
                rigidBody.AddTorque(transform.up * -inputs.TurnInput * turnTorque * Time.fixedDeltaTime);
            }else{
                rigidBody.AddTorque(transform.up * inputs.TurnInput * turnTorque * Time.fixedDeltaTime);
            }

            if(inputs.TurnInput==0){
                // For responsive feel so that tank doest continue turning after input
                rigidBody.AddTorque(transform.up * turnTorque * Time.fixedDeltaTime * -rigidBody.angularVelocity.y);
            }
        }

		if(inputs.DriveInput!=0f){
            SetLeftTrackTorque(inputs.DriveInput * driveTorque);
            SetRightTrackTorque(inputs.DriveInput * driveTorque);
        }else if(inputs.TurnInput!=0f){
            SetLeftTrackTorque(0.01f * driveTorque);
            SetRightTrackTorque(0.01f * driveTorque);
        }else{
            SetLeftTrackTorque(0f);
            SetRightTrackTorque(0f);
        }

		if((inputs.DriveInput==0 && inputs.TurnInput==0) || (inputs.DriveInput<0f && rigidBody.velocity.magnitude>5f)){
            rigidBody.drag = 2;
            SetBrakes(brakeStrength);
        }else if (inputs.DriveInput==0 && inputs.TurnInput!=0 && rigidBody.velocity.magnitude>1f){
            rigidBody.drag = 2f;
            SetBrakes(brakeStrength);
        }else{
            rigidBody.drag = 0.01f;
            SetBrakes(0);
        }
	}

	private void GetGroundedStatus(){
		GetHeight();
        grounded = (height<= maxHeightStillGrounded) ? true : false;
	}

    private void GetHeight(){
        // How high off the ground are we
        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -transform.up);
        // Debug.DrawRay(downRay.origin,downRay.direction * 100, Color.red);
        if (Physics.Raycast(downRay, out hit)){
            height = hit.distance;
        }
    }

	private void GetVelocities(){
        angularVelocity = rigidBody.angularVelocity.magnitude;
        velocityInKMH = rigidBody.velocity.magnitude * 3.6f;
	}

    private void GetClimbAngle(){
        curAngle = Vector3.Angle(Vector3.up, transform.up);
    }

	private void LimitMaximumVelocities(){
		if(grounded){
            rigidBody.angularVelocity = Vector3.ClampMagnitude(rigidBody.angularVelocity, maxRot);
            rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxSpeed);
        }
	}

    private void AdjustDrag(){
        // Adjusts drag when we are climbing vertically to slow us down
        if(curAngle>20f && height<= maxHeightStillGrounded && lastAngle<curAngle){
            rigidBody.drag=curAngle*0.03f;
        }

        if(!grounded){
            rigidBody.drag = 0.1f;
        }
    }

	private void SetLeftTrackTorque(float speed) {
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            if(velocityInKMH<-0.25f){
                // Gives initial boost so that controls seem more responsive
                leftWheelColliders[i].motorTorque = inputs.DriveInput < 0f ? speed*20f : speed*5f;
            }else{
                leftWheelColliders[i].motorTorque = speed;
            }
        }
    }

    private void SetRightTrackTorque(float speed) {
        for (int i = 0; i < rightWheelColliders.Length; i++) {
            if(velocityInKMH<-0.25f){
                // Gives initial boost so that controls seem more responsive
                rightWheelColliders[i].motorTorque = inputs.DriveInput < 0f ? speed*20f : speed*5f;
            }else{
                rightWheelColliders[i].motorTorque = speed;
            }
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

	private void SetWheelColliderFriction(){
		//If we are still or almost still reduce sideways friction
        if (rigidBody.velocity.magnitude <= 1f || curAngle > 22f) {
            for (int i = 0; i < leftWheelColliders.Length; i++) {
                sFrictionLeft[i].stiffness = stillSidewaysFriction;
                leftWheelColliders[i].sidewaysFriction = sFrictionLeft[i];
            }

            for (int i = 0; i < rightWheelColliders.Length; i++) {
                sFrictionRight[i].stiffness = stillSidewaysFriction;
                rightWheelColliders[i].sidewaysFriction = sFrictionRight[i];
            }
        }
        else {
            for (int i = 0; i < leftWheelColliders.Length; i++) {
                sFrictionLeft[i].stiffness = movementSidewaysFriction;
                leftWheelColliders[i].sidewaysFriction = sFrictionLeft[i];
            }

            for (int i = 0; i < rightWheelColliders.Length; i++) {
                sFrictionRight[i].stiffness = movementSidewaysFriction;
                rightWheelColliders[i].sidewaysFriction = sFrictionRight[i];
            }
        }
	}
}