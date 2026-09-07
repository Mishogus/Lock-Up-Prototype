using System.Collections.Generic;
using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// The Level 1 prison building, laid out from the design sketch: one block with
    /// a corridor down the middle, rooms on both sides.
    ///
    ///   north side   Dining | Showers | Gym | Cell 1 | Cell 2 | Cell 3 | Supervisor | Stairs
    ///   ------------------------------ corridor ------------------------------
    ///   south side   Mess Hall | Guards | Job 1 | Job 2 | Job 3 | Security (metal detector)
    ///
    /// The south exit leads out to the yard. Everything is measured in the block's
    /// local space, x from -HalfWidth to +HalfWidth.
    /// </summary>
    public static class PrisonBlock
    {
        public const float HalfWidth = 30f;      // block runs x -30 .. 30
        public const float HalfDepth = 12f;      // and z -12 .. 12
        public const float Height = 3.8f;
        public const float WallThickness = 0.3f;
        public const float CorridorHalf = 1.75f; // corridor spans z -1.75 .. 1.75

        public struct Room
        {
            public string name;
            public float x0, x1;
            public bool north;

            public Room(string name, float x0, float x1, bool north)
            {
                this.name = name; this.x0 = x0; this.x1 = x1; this.north = north;
            }

            public float CentreX { get { return (x0 + x1) * 0.5f; } }
            public float Width { get { return x1 - x0; } }

            /// <summary>Middle of the room, away from the corridor wall.</summary>
            public Vector3 Centre
            {
                get
                {
                    float far = north ? HalfDepth - WallThickness : -HalfDepth + WallThickness;
                    float near = north ? CorridorHalf : -CorridorHalf;
                    return new Vector3(CentreX, 0f, (far + near) * 0.5f);
                }
            }
        }

        public static readonly Room[] Rooms =
        {
            new Room("Dining",     -30f, -21f, true),
            new Room("Showers",    -21f, -15f, true),
            new Room("Gym",        -15f,  -6f, true),
            new Room("Cell 1",      -6f,   0f, true),
            new Room("Cell 2",       0f,   6f, true),
            new Room("Cell 3",       6f,  12f, true),
            new Room("Supervisor",  12f,  20f, true),
            new Room("Stairs",      20f,  30f, true),

            new Room("Mess Hall",  -30f, -17f, false),
            new Room("Guards",     -17f,  -9f, false),
            new Room("Job 1",       -9f,  -1f, false),
            new Room("Job 2",       -1f,   7f, false),
            new Room("Job 3",        7f,  15f, false),
            new Room("Security",    15f,  30f, false),
        };

        public static Room Find(string name)
        {
            for (int i = 0; i < Rooms.Length; i++)
                if (Rooms[i].name == name) return Rooms[i];

            return Rooms[0];
        }

        /// <summary>Builds the block and records waypoints into <paramref name="map"/>.</summary>
        public static Transform Build(Transform parent, Vector3 centre, Palette p, LevelMap map)
        {
            Transform b = BuildKit.Empty("PrisonBlock", parent, centre).transform;
            map.origin = centre;

            float hx = HalfWidth - WallThickness * 0.5f;
            float hz = HalfDepth - WallThickness * 0.5f;

            // Floor, ceiling, roof.
            BuildKit.Box("Floor", b, new Vector3(0f, -0.06f, 0f),
                         new Vector3(HalfWidth * 2f, 0.12f, HalfDepth * 2f), p.floor);
            BuildKit.Box("Ceiling", b, new Vector3(0f, Height + 0.1f, 0f),
                         new Vector3(HalfWidth * 2f, 0.2f, HalfDepth * 2f), p.concreteDark);
            BuildKit.Box("Roof", b, new Vector3(0f, Height + 0.4f, 0f),
                         new Vector3(HalfWidth * 2f + 0.8f, 0.34f, HalfDepth * 2f + 0.8f), p.concreteDark);

            BuildExteriorWalls(b, hx, hz, p);
            BuildCorridorWalls(b, p);
            BuildPartitions(b, p);
            FurnishRooms(b, p, map);
            BuildCorridorFittings(b, p, map);

            // Guard patrol route: up and down the corridor, plus a look into the yard.
            map.AddCorridorPoint(new Vector3(-26f, 0f, 0f));
            map.AddCorridorPoint(new Vector3(-8f, 0f, 0f));
            map.AddCorridorPoint(new Vector3(10f, 0f, 0f));
            map.AddCorridorPoint(new Vector3(26f, 0f, 0f));

            return b;
        }

        // ------------------------------------------------------------ structure

        static void BuildExteriorWalls(Transform b, float hx, float hz, Palette p)
        {
            // North wall: a barred window for every cell, plus daylight for the rest.
            var north = new List<BuildKit.Opening>();
            for (int i = 0; i < Rooms.Length; i++)
            {
                Room r = Rooms[i];
                if (!r.north || r.name == "Stairs") continue;

                bool cell = r.name.StartsWith("Cell");
                north.Add(cell
                    ? BuildKit.Opening.Window(r.CentreX + hx, 0.8f, 1.9f, 2.8f)
                    : BuildKit.Opening.Window(r.CentreX + hx, r.Width * 0.45f, 1.5f, 2.9f));
            }

            BuildKit.WallRun("Wall_N", b, new Vector3(-hx, 0f, hz), new Vector3(hx, 0f, hz),
                             Height, WallThickness, p.concrete, north.ToArray());

            // South wall: the main exit sits in the security room.
            Room security = Find("Security");
            BuildKit.WallRun("Wall_S", b, new Vector3(-hx, 0f, -hz), new Vector3(hx, 0f, -hz),
                             Height, WallThickness, p.concrete,
                             BuildKit.Opening.Door(security.CentreX + hx + 4f, 2.2f, 2.6f),
                             BuildKit.Opening.Window(Find("Mess Hall").CentreX + hx, 5f, 1.5f, 2.9f),
                             BuildKit.Opening.Window(Find("Job 2").CentreX + hx, 4f, 1.5f, 2.9f),
                             BuildKit.Opening.Window(Find("Job 3").CentreX + hx, 4f, 1.5f, 2.9f));

            BuildKit.WallRun("Wall_W", b, new Vector3(-hx, 0f, -hz), new Vector3(-hx, 0f, hz),
                             Height, WallThickness, p.concrete);
            BuildKit.WallRun("Wall_E", b, new Vector3(hx, 0f, -hz), new Vector3(hx, 0f, hz),
                             Height, WallThickness, p.concrete);
        }

        static void BuildCorridorWalls(Transform b, Palette p)
        {
            float run = HalfWidth;

            var northDoors = new List<BuildKit.Opening>();
            var southDoors = new List<BuildKit.Opening>();

            for (int i = 0; i < Rooms.Length; i++)
            {
                Room r = Rooms[i];
                bool cell = r.name.StartsWith("Cell");

                // Cells get a narrow barred gate, everything else a wide doorway.
                var opening = cell
                    ? BuildKit.Opening.Door(r.CentreX + run, 1.2f, 2.3f)
                    : BuildKit.Opening.Door(r.CentreX + run, 2.0f, 2.5f);

                if (r.north) northDoors.Add(opening);
                else southDoors.Add(opening);
            }

            BuildKit.WallRun("CorridorWall_N", b, new Vector3(-run, 0f, CorridorHalf), new Vector3(run, 0f, CorridorHalf),
                             Height, 0.24f, p.concrete, northDoors.ToArray());
            BuildKit.WallRun("CorridorWall_S", b, new Vector3(-run, 0f, -CorridorHalf), new Vector3(run, 0f, -CorridorHalf),
                             Height, 0.24f, p.concrete, southDoors.ToArray());

            // Barred gates on the cells, swung open into the corridor.
            for (int i = 0; i < Rooms.Length; i++)
            {
                Room r = Rooms[i];
                if (!r.name.StartsWith("Cell")) continue;

                Props.BarredDoor(b, new Vector3(r.CentreX - 0.6f, 0f, CorridorHalf), 0f, 1.2f, 2.3f, 72f, p);
            }
        }

        static void BuildPartitions(Transform b, Palette p)
        {
            for (int i = 0; i < Rooms.Length; i++)
            {
                Room r = Rooms[i];
                if (Mathf.Abs(r.x1) >= HalfWidth) continue;   // outer wall already there

                float far = r.north ? HalfDepth : -HalfDepth;
                float near = r.north ? CorridorHalf : -CorridorHalf;
                float depth = Mathf.Abs(far - near);

                BuildKit.Box("Partition", b, new Vector3(r.x1, Height * 0.5f, (far + near) * 0.5f),
                             new Vector3(0.24f, Height, depth), p.concrete);
            }
        }

        static void BuildCorridorFittings(Transform b, Palette p, LevelMap map)
        {
            for (int i = 0; i < 9; i++)
            {
                float x = -26f + i * 6.5f;
                BuildKit.Lamp("CorridorLamp", b, new Vector3(x, Height - 0.2f, 0f), p.metal, 14f, 4.0f);
            }

            // Cameras watching the corridor from the ceiling line.
            map.cameraMounts.Add(Props.SecurityCamera(b, new Vector3(-20f, Height - 0.55f, CorridorHalf - 0.2f), 180f, p));
            map.cameraMounts.Add(Props.SecurityCamera(b, new Vector3(4f, Height - 0.55f, CorridorHalf - 0.2f), 180f, p));
            map.cameraMounts.Add(Props.SecurityCamera(b, new Vector3(22f, Height - 0.55f, -CorridorHalf + 0.2f), 0f, p));
        }

        // ------------------------------------------------------------ furniture

        static void FurnishRooms(Transform b, Palette p, LevelMap map)
        {
            for (int i = 0; i < Rooms.Length; i++)
            {
                Room r = Rooms[i];
                Vector3 c = r.Centre;

                // Every room gets a lamp near the corridor end so doorways read.
                BuildKit.Lamp("Lamp", b, new Vector3(r.CentreX, Height - 0.2f, c.z), p.metal, 10f, 3.0f);

                switch (r.name)
                {
                    case "Dining": Dining(b, r, p, map); break;
                    case "Showers": ShowerRoom(b, r, p); break;
                    case "Gym": Gym(b, r, p, map); break;
                    case "Cell 1":
                    case "Cell 2":
                    case "Cell 3": CellRoom(b, r, p, map); break;
                    case "Supervisor": SupervisorOffice(b, r, p, map); break;
                    case "Stairs": Stairwell(b, r, p); break;
                    case "Mess Hall": MessHall(b, r, p, map); break;
                    case "Guards": GuardRoom(b, r, p, map); break;
                    case "Job 1": Laundry(b, r, p, map); break;
                    case "Job 2": Workshop(b, r, p, map); break;
                    case "Job 3": PackingRoom(b, r, p, map); break;
                    case "Security": SecurityRoom(b, r, p, map); break;
                }
            }
        }

        /// <summary>Six bunks, a toilet and a sink — one cell holds six prisoners.</summary>
        static void CellRoom(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;
            float back = HalfDepth - WallThickness - 0.6f;

            for (int i = 0; i < 3; i++)
            {
                float z = c.z - 2.4f + i * 2.4f;
                Props.Bunk(b, new Vector3(r.x0 + 0.8f, 0f, z), 90f, p);

                map.AddCellBunk(new Vector3(r.x0 + 1.9f, 0f, z));
                map.AddCellBunk(new Vector3(r.x0 + 1.9f, 0f, z));
            }

            Props.Toilet(b, new Vector3(r.x1 - 0.9f, 0f, back), 180f, p);
            Props.Sink(b, new Vector3(r.x1 - 0.9f, 0f, back - 1.2f), 180f, p);
            Props.Shelf(b, new Vector3(r.x1 - 0.35f, 0f, c.z + 0.5f), 270f, p);

            map.AddCellCentre(c);
        }

        static void Dining(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;
            for (int i = 0; i < 3; i++)
            {
                float x = r.x0 + 2.2f + i * 2.4f;
                Props.MessTable(b, new Vector3(x, 0f, c.z), 0f, 5f, p);
                map.AddMessSeat(new Vector3(x + 1.1f, 0f, c.z - 1.5f));
                map.AddMessSeat(new Vector3(x + 1.1f, 0f, c.z + 1.5f));
                map.AddMessSeat(new Vector3(x - 1.1f, 0f, c.z));
            }
        }

        static void MessHall(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            Props.ServingCounter(b, new Vector3(c.x - 1f, 0f, c.z + 2.6f), 0f, 7f, p);
            Props.Stove(b, new Vector3(r.x0 + 2.5f, 0f, -HalfDepth + 1.2f), 0f, p);
            Props.Fridge(b, new Vector3(r.x0 + 1.2f, 0f, c.z - 1.5f), 90f, p);
            Props.MessTable(b, new Vector3(c.x + 1f, 0f, c.z - 2f), 90f, 4f, p);
            Props.Shelf(b, new Vector3(r.x1 - 0.5f, 0f, c.z), 270f, p);

            map.AddKitchenStation(new Vector3(c.x - 1f, 0f, c.z + 1.4f));
            map.AddKitchenStation(new Vector3(r.x0 + 2.5f, 0f, -HalfDepth + 2.4f));
            map.AddKitchenStation(new Vector3(c.x + 2.2f, 0f, c.z - 2f));
        }

        static void ShowerRoom(Transform b, Room r, Palette p)
        {
            Vector3 c = r.Centre;
            float back = HalfDepth - WallThickness;

            BuildKit.Box("TileFloor", b, new Vector3(r.CentreX, 0.02f, c.z),
                         new Vector3(r.Width - 0.3f, 0.04f, HalfDepth - CorridorHalf - 0.4f), p.tile, false);

            for (int i = 0; i < 3; i++)
            {
                float x = r.x0 + 1.4f + i * 1.7f;
                Props.ShowerHead(b, new Vector3(x, 0f, back - 0.25f), 180f, p);
                Props.Drain(b, new Vector3(x, 0f, back - 1.6f), p);
            }

            Props.Bench(b, new Vector3(c.x, 0f, c.z - 2.2f), 90f, 3.5f, p);
        }

        static void Gym(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            Props.WeightBench(b, new Vector3(r.x0 + 2.2f, 0f, c.z + 1f), 0f, p);
            Props.WeightBench(b, new Vector3(r.x0 + 4.6f, 0f, c.z + 1f), 0f, p);
            Props.WeightRack(b, new Vector3(c.x + 1.5f, 0f, HalfDepth - 1f), 180f, p);
            Props.Bench(b, new Vector3(r.x1 - 1f, 0f, c.z), 0f, 3f, p);
            Props.Crate(b, new Vector3(r.x1 - 1.2f, 0f, c.z + 2.6f), 20f, 0.7f, p);

            map.AddIndoorIdle(new Vector3(r.x0 + 3.4f, 0f, c.z - 1.5f));
        }

        static void SupervisorOffice(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            Props.Desk(b, new Vector3(c.x, 0f, c.z + 1.5f), 180f, p);
            Props.Chair(b, new Vector3(c.x, 0f, c.z + 0.3f), 0f, p);
            Props.Chair(b, new Vector3(c.x - 1.6f, 0f, c.z - 1.2f), 25f, p);
            Props.Lockers(b, new Vector3(r.x1 - 1.6f, 0f, HalfDepth - 1f), 180f, 3, p);
            Props.Shelf(b, new Vector3(r.x0 + 0.5f, 0f, c.z), 90f, p);

            map.SetSupervisorDesk(new Vector3(c.x, 0f, c.z + 0.3f));
        }

        static void Stairwell(Transform b, Room r, Palette p)
        {
            Vector3 c = r.Centre;

            // Stairs to the upper floor. The landing is as far as Level 1 goes.
            Props.Stairs(b, new Vector3(c.x, 0f, CorridorHalf + 0.8f), 0f, 2.4f, 0.18f, 14, p);
            BuildKit.Box("UpperFloor", b, new Vector3(c.x, 2.62f, HalfDepth - 2.2f),
                         new Vector3(r.Width - 1f, 0.2f, 4f), p.concrete);
            Props.Crate(b, new Vector3(r.x0 + 1.2f, 0f, HalfDepth - 1.5f), 15f, 0.8f, p);
            Props.Crate(b, new Vector3(r.x0 + 2.1f, 0f, HalfDepth - 1.6f), -20f, 0.6f, p);
        }

        static void GuardRoom(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            Props.Desk(b, new Vector3(c.x - 1.8f, 0f, c.z + 1.6f), 0f, p);
            Props.Chair(b, new Vector3(c.x - 1.8f, 0f, c.z + 0.4f), 180f, p);
            Props.Desk(b, new Vector3(c.x + 1.8f, 0f, c.z + 1.6f), 0f, p);
            Props.Lockers(b, new Vector3(c.x, 0f, -HalfDepth + 0.6f), 0f, 6, p);
            Props.Bench(b, new Vector3(r.x1 - 1f, 0f, c.z - 1f), 0f, 2.4f, p);

            // Monitor wall showing the camera feeds.
            for (int i = 0; i < 4; i++)
                BuildKit.Box("Monitor", b, new Vector3(c.x - 1.5f + (i % 2) * 1.1f, 2.1f + (i / 2) * 0.7f, c.z + 2.4f),
                             new Vector3(0.9f, 0.6f, 0.08f), p.metal, false);

            map.AddGuardPost(new Vector3(c.x, 0f, c.z - 1.5f));
            map.AddGuardPost(new Vector3(c.x + 1.8f, 0f, c.z + 0.4f));
        }

        static void Laundry(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            for (int i = 0; i < 4; i++)
            {
                float x = r.x0 + 1.2f + i * 1.6f;
                Props.WashingMachine(b, new Vector3(x, 0f, -HalfDepth + 0.9f), 0f, p);
                if (i < 3) map.AddJobStation(new Vector3(x + 0.4f, 0f, -HalfDepth + 2.2f));
            }

            Props.MessTable(b, new Vector3(c.x, 0f, c.z + 1.2f), 90f, 4f, p);
            Props.Crate(b, new Vector3(r.x1 - 1f, 0f, c.z - 1.5f), 10f, 0.8f, p);
        }

        static void Workshop(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            Props.Workbench(b, new Vector3(c.x - 1.6f, 0f, -HalfDepth + 1.4f), 0f, 4f, p);
            Props.Workbench(b, new Vector3(c.x + 1.6f, 0f, c.z + 1.8f), 180f, 4f, p);
            Props.Crate(b, new Vector3(r.x0 + 1f, 0f, c.z), 25f, 0.9f, p);
            Props.Crate(b, new Vector3(r.x1 - 1.2f, 0f, c.z - 2f), -15f, 0.7f, p);

            map.AddJobStation(new Vector3(c.x - 1.6f, 0f, -HalfDepth + 2.6f));
            map.AddJobStation(new Vector3(c.x + 1.6f, 0f, c.z + 0.6f));
        }

        static void PackingRoom(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;

            Props.MessTable(b, new Vector3(c.x - 1.6f, 0f, c.z), 0f, 5f, p);
            Props.MessTable(b, new Vector3(c.x + 1.6f, 0f, c.z), 0f, 5f, p);
            Props.Shelf(b, new Vector3(r.x1 - 0.5f, 0f, c.z), 270f, p);

            for (int i = 0; i < 3; i++)
                Props.Crate(b, new Vector3(r.x0 + 0.9f, 0f, -HalfDepth + 1f + i * 1.1f), i * 20f, 0.75f, p);

            map.AddJobStation(new Vector3(c.x - 2.8f, 0f, c.z));
            map.AddJobStation(new Vector3(c.x + 2.8f, 0f, c.z));
        }

        static void SecurityRoom(Transform b, Room r, Palette p, LevelMap map)
        {
            Vector3 c = r.Centre;
            float exitX = c.x + 4f;

            // Everyone leaving the block walks through the detector.
            map.metalDetector = Props.MetalDetector(b, new Vector3(exitX, 0f, -HalfDepth + 2.2f), 0f, p);

            Props.Desk(b, new Vector3(exitX - 3.5f, 0f, -HalfDepth + 1.6f), 90f, p);
            Props.Chair(b, new Vector3(exitX - 2.4f, 0f, -HalfDepth + 1.6f), 270f, p);
            Props.Lockers(b, new Vector3(r.x0 + 2f, 0f, c.z + 2f), 0f, 4, p);
            Props.Bench(b, new Vector3(r.x0 + 1.2f, 0f, c.z - 1.5f), 0f, 2.4f, p);

            map.cameraMounts.Add(Props.SecurityCamera(b, new Vector3(exitX + 2.5f, Height - 0.6f, -HalfDepth + 0.6f), 20f, p));

            map.AddGuardPost(new Vector3(exitX - 2.4f, 0f, -HalfDepth + 2.6f));
            map.SetExitDoor(new Vector3(exitX, 0f, -HalfDepth - 1.5f));
        }
    }
}
