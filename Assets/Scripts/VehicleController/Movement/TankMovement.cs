using UnityEngine;
using System;

public class TankMovement : MonoBehaviour{

	[Header("Tank Power Settings")]
    public float turnTorque = 1f;
    public float driveTorque = 2f;
    public float brakeStrength = 3f;
    public float turningForceCoefficient = 0.7f;
    public float forwardForceCoefficient = 12f;
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
		rigidBody = GetComponent<Rigidbody>();
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
	}

    private void Update(){
        // Sets angular and km/h velocity values to variables
		GetVelocities();
    }

	private void FixedUpdate(){
		// Sets boolean to indicate whether we are on the ground
		GetGroundedStatus();
		// Physically moves the tank
        MoveTank();
        // Get our current climb angle
        GetClimbAngle();
	}

	private void LateUpdate(){
		SetWheelColliderFriction();
	}

    private void MoveTank(){
        // Gets the forward/backward velocity
        float velocityInDirection = Vector3.Dot(rigidBody.velocity, transform.forward);
        float dragCoefficient = CalculateDragCoefficient(velocityInDirection, forwardForceCoefficient);

        // Gets the turning angular velocity
        float angularVelocityInDirection = Vector3.Dot(rigidBody.angularVelocity, transform.up);
        float dragTurnCoefficient = CalculateDragCoefficient(angularVelocityInDirection, turningForceCoefficient);;

        // what is this?
        float turnForwardVelocityCoefficient = 2 * dragCoefficient;
        turnForwardVelocityCoefficient = turnForwardVelocityCoefficient > 1f ? 1f : turnForwardVelocityCoefficient;

        if(grounded){
            // Turns tank using turnTorque * dragTurnCoefficient, which is a scaled value from 0..1 depending on angular velocity
            // First part makes sure direction and rotation of turning is correct
            Vector3 turningTorqueValue = dragTurnCoefficient * turnForwardVelocityCoefficient * turnTorque * (inputs.TurnInput * Time.fixedDeltaTime * transform.up);
            if(inputs.DriveInput<0){
                rigidBody.AddTorque(turningTorqueValue * -1f);
            }else{
                rigidBody.AddTorque(turningTorqueValue);
            }

            // For responsive feel, stops tank rotation when we do not want to turn, otherwise would continue turning 
            if(inputs.TurnInput==0){
                rigidBody.AddTorque(-angularVelocityInDirection * Time.fixedDeltaTime * turnTorque * transform.up);
            }

            // If we switch from forwards to backwards, we want tank to move responsively instead of sliding
            if(inputs.DriveInput!=0f){
                if(inputs.DriveInput<0f && velocityInDirection>0f){
                    rigidBody.AddForce(2000f * driveTorque * Time.fixedDeltaTime * -velocityInDirection * transform.forward);
                }else if(inputs.DriveInput>0f && velocityInDirection<0f){
                    rigidBody.AddForce(2000f * driveTorque * Time.fixedDeltaTime * -velocityInDirection * transform.forward);
                }

                // Moves tank using driveTorque * dragCoefficient, which is a scaled value from 0..1 depending on velocity
                // Disable brakes first so we can move
                SetBrakes(0);
                SetLeftTrackTorque(inputs.DriveInput * driveTorque * dragCoefficient);
                SetRightTrackTorque(inputs.DriveInput * driveTorque * dragCoefficient);
            }else if(inputs.TurnInput!=0f){
                // Moves tank slightly forward if we are only turning, otherwise friction would not allow AddTorque to function properly
                // Disable brakes (idk if necessary here)
                SetBrakes(0);
                SetLeftTrackTorque(0.01f * driveTorque);
                SetRightTrackTorque(0.01f * driveTorque);

                // Stops tank if we stop wanting to move forwards/backwards instead of slowly coming to a stop like a car
                rigidBody.AddForce(1000f * driveTorque * Time.fixedDeltaTime * -velocityInDirection * transform.forward);
            }else{
                // Enable brakes so that we dont slide on a slope or a hill when still
                SetBrakes(brakeStrength);
                SetLeftTrackTorque(0f);
                SetRightTrackTorque(0f);

                // Stops tank if we stop wanting to move forwards/backwards instead of slowly coming to a stop like a car
                rigidBody.AddForce(1000f * driveTorque * Time.fixedDeltaTime * -velocityInDirection * transform.forward);
            }
        }
    }

    private float CalculateDragCoefficient(float _velocity, float _coefficient){
        float normalizedVelocity = _velocity / _coefficient;
        normalizedVelocity = Math.Abs(normalizedVelocity);
        if (normalizedVelocity>1f){
            normalizedVelocity=1f;
        }
        return (1 - normalizedVelocity);
    }

    private void GetVelocities(){
        angularVelocity = rigidBody.angularVelocity.magnitude;
        velocityInKMH = rigidBody.velocity.magnitude * 3.6f;
	}

	private void GetGroundedStatus(){
		GetHeight();

        // Assumes equal amount of wheelcolliders on left and right side
        int countOfWheelColliders = leftWheelColliders.Length * 2;
        int notGroundedCount = 0;
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            if(!leftWheelColliders[i].isGrounded){
                notGroundedCount+=1;
            }
            if(!rightWheelColliders[i].isGrounded){
                notGroundedCount+=1;
            }
        }
        //grounded = (height<= maxHeightStillGrounded) ? true : false;
        grounded = notGroundedCount >= countOfWheelColliders-2 ? false : true;
	}

    private void GetHeight(){
        // How high off the ground are we
        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -transform.up);
        // Debug.DrawRay(downRay.origin,downRay.direction * 100, Color.red);
        if (Physics.Raycast(downRay, out hit)){
            height = hit.distance;
            //Debug.Log(height.ToString());
        }
    }

    private void GetClimbAngle(){
        curAngle = Vector3.Angle(Vector3.up, transform.up);
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