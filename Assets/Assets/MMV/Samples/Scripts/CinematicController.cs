using MMV;
using UnityEngine;

public class CinematicController : MonoBehaviour
{
    private MMV_Shooter shooter;
    public WaypointsSystem waypointSystem;

    [Header("Camera")]

    public bool activeLookTo;
    public bool activeMoveTo;
    public bool activeMoveDir;
    public bool activeMove;
    public Transform cameraTarget;
    public Vector3 cameraMoveTo;
    public Vector3 cameraMoveDir;
    public float cameraMoveToSpeed;

    [Header("Vehicle")]

    public bool stopVehicle;
    public bool activeMoveOnPath;
    public bool activeVehicleMoveToForward;
    public bool activeVehicleTurretLookToForward;
    public bool activeVehicleTurretRotation;
    public bool activeVehicleRotationLeft;
    public bool activeVehicleRotationRight;
    public bool activeVehicleTurretLookAt;
    public bool shot;

    public Transform turretTarget;
    public float turretRotationSpeed;

    [Header("Waypoints")]
    public float detectWaypointInDistance;

    private int currentWaypointIndex;
    private MMV_MBT_Vehicle vehicle;

    void Start()
    {
        vehicle = GetComponent<MMV_MBT_Vehicle>();
        shooter = vehicle.GetComponent<MMV_Shooter>();
    }

    private void Update()
    {
        if (activeVehicleTurretRotation)
        {
            vehicle.Turret.TurretEnabled = false;
            vehicle.Turret.Gun.localEulerAngles = new Vector3(-15, 0, 0);
            vehicle.Turret.Turret.Rotate(0, turretRotationSpeed * Time.deltaTime, 0);
        }
    }

    void FixedUpdate()
    {
        if (activeMoveTo)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position + transform.TransformDirection(cameraMoveTo), Time.deltaTime * cameraMoveToSpeed);
        }

        if (activeMove)
        {
            Camera.main.transform.Translate(transform.forward * vehicle.Rb.velocity.z * Time.fixedDeltaTime, Space.World);
        }

        if (activeMoveDir)
        {
            Camera.main.transform.Translate(cameraMoveDir * Time.fixedDeltaTime, Space.World);
        }

        if (activeLookTo && cameraTarget)
        {
            Camera.main.transform.LookAt(cameraTarget.position);
        }

        if (activeMoveOnPath)
        {
            if (!waypointSystem)
            {
                return;
            }

            var currentWaypoint = waypointSystem.waypoints[this.currentWaypointIndex];

            // switch to the next waypoint when you get close to the current one
            if (Vector3.Distance(transform.position, currentWaypoint.position) < detectWaypointInDistance)
            {
                this.currentWaypointIndex++;

                // go back to the first if you are in the last
                if (this.currentWaypointIndex > waypointSystem.waypoints.Length - 1)
                {
                    this.currentWaypointIndex = 0;
                }
            }

            // direction to target (!!NO NORMALIZED!!)
            var dirToWaypoint = currentWaypoint.position - transform.position;

            vehicle.TurretTargetPosition = currentWaypoint.position;
            vehicle.MoveTo(currentWaypoint.position, stopWithAngle: 45);
        }

        if (activeVehicleMoveToForward)
        {
            vehicle.MoveTo(transform.position + (transform.forward * 10));
        }

        if (stopVehicle)
        {
            vehicle.MoveTo(transform.position);
        }

        if (activeVehicleTurretLookToForward)
        {
            vehicle.TurretTargetPosition = transform.position + Vector3.ProjectOnPlane(transform.forward * 10, Vector3.up) + (Vector3.up * 2);
        }

        if (shot)
        {
            GetComponent<MMV_Shooter>().Shoot();
        }

        if (activeVehicleRotationLeft)
        {
            vehicle.MoveTo(transform.position + (transform.right * -5));
        }

        else if (activeVehicleRotationRight)
        {
            vehicle.MoveTo(transform.position + (transform.right * 5));
        }

        if (activeVehicleTurretLookAt && turretTarget)
        {
            vehicle.TurretTargetPosition = turretTarget.position;
        }
    }
}
