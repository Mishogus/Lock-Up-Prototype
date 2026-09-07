using System.Collections.Generic;
using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Waypoints the level generator hands to the AI: where cells, jobs, seats and
    /// patrol routes ended up. Builders add points in their own local space and
    /// <see cref="origin"/> converts them to world positions.
    /// </summary>
    public class LevelMap : MonoBehaviour
    {
        [HideInInspector] public Vector3 origin;

        public List<Vector3> corridorRoute = new List<Vector3>();
        public List<Vector3> yardRoute = new List<Vector3>();
        public List<Vector3> guardPosts = new List<Vector3>();
        public List<Vector3> jobStations = new List<Vector3>();
        public List<Vector3> kitchenStations = new List<Vector3>();
        public List<Vector3> messSeats = new List<Vector3>();
        public List<Vector3> cellBunks = new List<Vector3>();
        public List<Vector3> cellCentres = new List<Vector3>();
        public List<Vector3> indoorIdle = new List<Vector3>();
        public List<Transform> cameraMounts = new List<Transform>();

        public Vector3 supervisorDesk;
        public Vector3 exitDoor;
        public Vector3 playerSpawn;
        public Transform metalDetector;

        public Vector3 World(Vector3 local) { return origin + local; }

        public void AddCorridorPoint(Vector3 local) { corridorRoute.Add(World(local)); }
        public void AddGuardPost(Vector3 local) { guardPosts.Add(World(local)); }
        public void AddJobStation(Vector3 local) { jobStations.Add(World(local)); }
        public void AddKitchenStation(Vector3 local) { kitchenStations.Add(World(local)); }
        public void AddMessSeat(Vector3 local) { messSeats.Add(World(local)); }
        public void AddCellBunk(Vector3 local) { cellBunks.Add(World(local)); }
        public void AddCellCentre(Vector3 local) { cellCentres.Add(World(local)); }
        public void AddIndoorIdle(Vector3 local) { indoorIdle.Add(World(local)); }

        public void SetSupervisorDesk(Vector3 local) { supervisorDesk = World(local); }
        public void SetExitDoor(Vector3 local) { exitDoor = World(local); }

        /// <summary>Pick from a list without ever indexing out of range.</summary>
        public static Vector3 Pick(List<Vector3> list, int index, Vector3 fallback)
        {
            if (list == null || list.Count == 0) return fallback;
            return list[((index % list.Count) + list.Count) % list.Count];
        }
    }
}
