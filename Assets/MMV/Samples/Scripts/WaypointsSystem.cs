using UnityEngine;
using System.Collections.Generic;

public class WaypointsSystem : MonoBehaviour
{
    // All children are waypoints
    [HideInInspector] public Transform[] waypoints;

    private void OnDrawGizmos()
    {
        GetAllWaypoints();

        if (waypoints.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.red;

        for (int i = 0; i <= waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1 < waypoints.Length ? i + 1 : 0].position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
        }
    }

    private void GetAllWaypoints()
    {
        var transforms = GetComponentsInChildren<Transform>();

        if (waypoints == null)
        {
            waypoints = new Transform[0];
        }

        // check if there are children and if the list has been changed
        //GetComponentsInChildren<Transform>() returns size 1 even though it has no children as it also takes its own transform
        if (transforms.Length > 1 && waypoints.Length != transforms.Length - 1)
        {
            List<Transform> waypointsList = new List<Transform>();

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != transform)
                {
                    transforms[i].name = "waypoint " + i;
                    waypointsList.Add(transforms[i]);
                }
            }

            waypoints = waypointsList.ToArray();
        }
    }
}
