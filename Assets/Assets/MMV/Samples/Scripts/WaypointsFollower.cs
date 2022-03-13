using MMV;
using UnityEngine;

public class WaypointsFollower : MonoBehaviour
{
    private MMV_Shooter shooter;
    public WaypointsSystem waypointSystem;

    [Space(10)]
    public string obstaclesTag;
    public float detectWaypointInDistance;
    public float shotWithDistance;

    private int currentWaypointIndex;
    private MMV_MBT_Vehicle vehicle;

    void Start()
    {
        vehicle = GetComponent<MMV_MBT_Vehicle>();
        shooter = vehicle.GetComponent<MMV_Shooter>();
    }

    void Update()
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

        // shoot
        if (Physics.Raycast(vehicle.Turret.Gun.position, vehicle.Turret.Gun.forward, out RaycastHit hit, shotWithDistance))
        {
            if (hit.transform.tag == obstaclesTag)
            {
                if (shooter) shooter.Shoot();
            }
        }
    }
}
