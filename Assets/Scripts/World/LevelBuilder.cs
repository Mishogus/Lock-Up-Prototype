using System.Collections.Generic;
using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Builds Level 1 from the design sketch: one prison block (dining, showers,
    /// gym, three cells, supervisor's office and stairs on the north side; mess
    /// hall, guard room, three job rooms and security on the south side), a walled
    /// yard in front of it with searchlight towers on every corner, and forest
    /// beyond the wire.
    ///
    /// Hit "Build Level" in the inspector to regenerate.
    /// </summary>
    [RequireComponent(typeof(LevelMap))]
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Ground")]
        public float groundSize = 320f;

        [Header("Compound")]
        public float compoundHalfWidth = 44f;
        public float compoundNorth = 30f;
        public float compoundSouth = -40f;
        public float wallHeight = 6f;
        public float wallThickness = 0.8f;
        public float gateWidth = 8f;

        [Header("Watchtowers")]
        public float towerHeight = 9f;
        public float towerFootprint = 4.4f;
        public bool towerSpotlights = true;

        [Header("Contents")]
        public bool buildBlock = true;
        public bool spawnPeople = true;

        [Header("Trees")]
        public int treeCount = 150;
        public float treeSpacing = 3.5f;
        public int seed = 1337;

        [Header("Build")]
        public bool markStatic = true;

        const string RootName = "Level";

        // Interior floors finish just above the yard sheet, so the dirt does not
        // cover them and doorways get a shallow threshold instead of a step.
        const float FloorTop = 0.06f;

        /// <summary>The block sits across the north end of the compound.</summary>
        static readonly Vector3 BlockAt = new Vector3(0f, FloorTop, 16f);

        Palette palette;
        LevelMap map;

        public Transform LevelRoot
        {
            get { return transform.Find(RootName); }
        }

        public Vector3 PlayerSpawn
        {
            get
            {
                if (!buildBlock) return transform.position + new Vector3(0f, 1.2f, -20f);

                // Cell 1, clear of the bunks.
                PrisonBlock.Room cell = PrisonBlock.Find("Cell 1");
                Vector3 c = cell.Centre;
                return transform.position + BlockAt + new Vector3(c.x + 1.6f, 1.2f, c.z);
            }
        }

        public void Rebuild()
        {
            Clear();
            Build();
        }

        public void Clear()
        {
            Transform root = LevelRoot;
            if (root != null) BuildKit.Destroy(root.gameObject);

            LevelMap existing = GetComponent<LevelMap>();
            if (existing != null) ResetMap(existing);
        }

        public void Build()
        {
            if (LevelRoot != null) Clear();

            palette = Palette.Create();

            map = GetComponent<LevelMap>();
            if (map == null) map = gameObject.AddComponent<LevelMap>();
            ResetMap(map);

            Transform root = BuildKit.Empty(RootName, transform).transform;

            BuildGround(root);
            if (buildBlock) PrisonBlock.Build(root, BlockAt, palette, map);
            BuildYard(root);
            BuildPerimeter(root);
            BuildForest(root);

            map.playerSpawn = PlayerSpawn;

            // Static flags only mean anything for scene-authored geometry.
            if (markStatic && !Application.isPlaying) BuildKit.MarkStaticRecursive(root.gameObject);
        }

        public void PopulateNpcs()
        {
            if (!spawnPeople || map == null) return;

            Transform root = LevelRoot;
            if (root == null) return;

            NpcDirector.Populate(root, map, palette != null ? palette : Palette.Create());
        }

        static void ResetMap(LevelMap m)
        {
            m.corridorRoute.Clear();
            m.yardRoute.Clear();
            m.guardPosts.Clear();
            m.jobStations.Clear();
            m.kitchenStations.Clear();
            m.messSeats.Clear();
            m.cellBunks.Clear();
            m.cellCentres.Clear();
            m.indoorIdle.Clear();
            m.cameraMounts.Clear();
            m.metalDetector = null;
        }

        // --------------------------------------------------------------- ground

        void BuildGround(Transform root)
        {
            Transform g = BuildKit.Empty("Ground", root).transform;

            BuildKit.Box("Terrain", g, new Vector3(0f, -0.5f, 0f),
                         new Vector3(groundSize, 1f, groundSize), palette.grass);

            float depth = compoundNorth - compoundSouth;
            float centreZ = (compoundNorth + compoundSouth) * 0.5f;

            BuildKit.Box("CompoundGround", g, new Vector3(0f, 0.015f, centreZ),
                         new Vector3(compoundHalfWidth * 2f, 0.03f, depth), palette.dirt, collider: false);

            // Approach road running south from the gate.
            BuildKit.Box("Road", g, new Vector3(0f, 0.02f, compoundSouth - 22f),
                         new Vector3(9f, 0.04f, 46f), palette.gravel, collider: false);
        }

        // ----------------------------------------------------------------- yard

        void BuildYard(Transform root)
        {
            Transform y = BuildKit.Empty("Yard", root).transform;

            // Exercise area between the block and the gate.
            BuildKit.Box("Court", y, new Vector3(-10f, 0.05f, -12f), new Vector3(22f, 0.08f, 15f), palette.gravel, false);
            BuildKit.Box("Line_Centre", y, new Vector3(-10f, 0.1f, -12f), new Vector3(0.12f, 0.02f, 15f), palette.paint, false);

            Props.BasketballHoop(y, new Vector3(-20f, 0f, -12f), 90f, palette);
            Props.BasketballHoop(y, new Vector3(0f, 0f, -12f), 270f, palette);

            Props.Bench(y, new Vector3(-24f, 0f, -4f), 0f, 3f, palette);
            Props.Bench(y, new Vector3(6f, 0f, -6f), 0f, 3f, palette);
            Props.Bench(y, new Vector3(14f, 0f, -14f), 0f, 3f, palette);

            // Trees inside the wire, as drawn on the plan.
            BuildTree(y, new Vector3(20f, 0f, -6f), new System.Random(11));
            BuildTree(y, new Vector3(28f, 0f, -20f), new System.Random(12));
            BuildTree(y, new Vector3(-30f, 0f, -24f), new System.Random(13));

            // Inner wire fence short of the wall, so the yard has a real edge.
            float fenceZ = compoundSouth + 6f;
            Props.Fence(y, new Vector3(-compoundHalfWidth + 4f, 0f, fenceZ),
                        new Vector3(-gateWidth * 0.5f - 1f, 0f, fenceZ), 3f, palette);
            Props.Fence(y, new Vector3(gateWidth * 0.5f + 1f, 0f, fenceZ),
                        new Vector3(compoundHalfWidth - 4f, 0f, fenceZ), 3f, palette);

            // Yard patrol route for the head guards, and where inmates loiter.
            map.yardRoute.Add(new Vector3(-24f, 0f, -8f));
            map.yardRoute.Add(new Vector3(-6f, 0f, -18f));
            map.yardRoute.Add(new Vector3(12f, 0f, -10f));
            map.yardRoute.Add(new Vector3(18f, 0f, -24f));
            map.yardRoute.Add(new Vector3(-2f, 0f, -28f));
            map.yardRoute.Add(new Vector3(-20f, 0f, -20f));
        }

        // ------------------------------------------------------------ perimeter

        void BuildPerimeter(Transform root)
        {
            Transform prison = BuildKit.Empty("Perimeter", root).transform;
            Transform walls = BuildKit.Empty("Walls", prison).transform;

            float hx = compoundHalfWidth;
            float zN = compoundNorth;
            float zS = compoundSouth;
            float t = wallThickness;
            float width = hx * 2f;

            BuildKit.WallRun("Wall_S", walls, new Vector3(-hx, 0f, zS), new Vector3(hx, 0f, zS),
                             wallHeight, t, palette.concrete,
                             BuildKit.Opening.Door(width * 0.5f, gateWidth, wallHeight));
            BuildKit.WallRun("Wall_N", walls, new Vector3(-hx, 0f, zN), new Vector3(hx, 0f, zN),
                             wallHeight, t, palette.concrete);
            BuildKit.WallRun("Wall_W", walls, new Vector3(-hx, 0f, zS), new Vector3(-hx, 0f, zN),
                             wallHeight, t, palette.concrete);
            BuildKit.WallRun("Wall_E", walls, new Vector3(hx, 0f, zS), new Vector3(hx, 0f, zN),
                             wallHeight, t, palette.concrete);

            float depth = zN - zS;
            float centreZ = (zN + zS) * 0.5f;
            BuildKit.Box("Cap_S", walls, new Vector3(0f, wallHeight + 0.1f, zS), new Vector3(width + 0.3f, 0.35f, t + 0.3f), palette.concreteDark, false);
            BuildKit.Box("Cap_N", walls, new Vector3(0f, wallHeight + 0.1f, zN), new Vector3(width + 0.3f, 0.35f, t + 0.3f), palette.concreteDark, false);
            BuildKit.Box("Cap_W", walls, new Vector3(-hx, wallHeight + 0.1f, centreZ), new Vector3(t + 0.3f, 0.35f, depth), palette.concreteDark, false);
            BuildKit.Box("Cap_E", walls, new Vector3(hx, wallHeight + 0.1f, centreZ), new Vector3(t + 0.3f, 0.35f, depth), palette.concreteDark, false);

            BuildGate(prison, new Vector3(0f, 0f, zS));

            Transform towers = BuildKit.Empty("Watchtowers", prison).transform;
            BuildTower(towers, new Vector3(-hx, 0f, zS), "Watchtower_SW", 0);
            BuildTower(towers, new Vector3(hx, 0f, zS), "Watchtower_SE", 1);
            BuildTower(towers, new Vector3(hx, 0f, zN), "Watchtower_NE", 2);
            BuildTower(towers, new Vector3(-hx, 0f, zN), "Watchtower_NW", 3);
        }

        void BuildGate(Transform parent, Vector3 at)
        {
            Transform gate = BuildKit.Empty("MainGate", parent, at).transform;
            float leaf = gateWidth * 0.5f;

            Props.BarredDoor(gate, new Vector3(-leaf, 0f, 0f), 0f, leaf, wallHeight - 0.6f, 0f, palette);
            Props.BarredDoor(gate, new Vector3(leaf, 0f, 0f), 180f, leaf, wallHeight - 0.6f, 0f, palette);

            BuildKit.Box("Pier", gate, new Vector3(-leaf - 0.35f, wallHeight * 0.5f + 0.4f, 0f),
                         new Vector3(0.7f, wallHeight + 0.8f, wallThickness + 0.5f), palette.concreteDark);
            BuildKit.Box("Pier", gate, new Vector3(leaf + 0.35f, wallHeight * 0.5f + 0.4f, 0f),
                         new Vector3(0.7f, wallHeight + 0.8f, wallThickness + 0.5f), palette.concreteDark);
            BuildKit.Box("Lintel", gate, new Vector3(0f, wallHeight + 0.6f, 0f),
                         new Vector3(gateWidth + 1.4f, 0.5f, wallThickness + 0.5f), palette.concreteDark, false);

            Transform hut = BuildKit.Empty("GateHouse", gate, new Vector3(-leaf - 3.5f, 0f, 2.6f)).transform;
            BuildKit.Box("Body", hut, new Vector3(0f, 1.3f, 0f), new Vector3(3f, 2.6f, 3f), palette.concrete);
            BuildKit.Box("Roof", hut, new Vector3(0f, 2.75f, 0f), new Vector3(3.6f, 0.25f, 3.6f), palette.concreteDark, false);
            BuildKit.Box("Window", hut, new Vector3(0f, 1.7f, 1.52f), new Vector3(1.8f, 0.9f, 0.06f), palette.steel, false);
        }

        void BuildTower(Transform parent, Vector3 corner, string name, int index)
        {
            Transform tower = BuildKit.Empty(name, parent, corner).transform;

            // Face the middle of the compound.
            Vector3 centre = new Vector3(0f, 0f, (compoundNorth + compoundSouth) * 0.5f);
            Vector3 toCentre = centre - corner;
            toCentre.y = 0f;
            if (toCentre.sqrMagnitude > 0.001f)
                tower.localRotation = Quaternion.LookRotation(toCentre.normalized, Vector3.up);

            float f = towerFootprint;
            float legHalf = f * 0.5f;
            float platformSize = f + 1.6f;
            float platformY = towerHeight;
            const float platformThickness = 0.35f;
            const float railHeight = 1.0f;
            const float roofPostHeight = 2.4f;

            for (int i = 0; i < 4; i++)
            {
                float sx = (i == 0 || i == 3) ? -1f : 1f;
                float sz = (i < 2) ? -1f : 1f;
                BuildKit.Box("Leg", tower, new Vector3(sx * legHalf, platformY * 0.5f, sz * legHalf),
                             new Vector3(0.45f, platformY, 0.45f), palette.wood);
            }

            for (int level = 1; level <= 2; level++)
            {
                float by = platformY * (level / 3f);
                BuildKit.Box("Brace", tower, new Vector3(0f, by, -legHalf), new Vector3(f, 0.28f, 0.28f), palette.wood, false);
                BuildKit.Box("Brace", tower, new Vector3(0f, by, legHalf), new Vector3(f, 0.28f, 0.28f), palette.wood, false);
                BuildKit.Box("Brace", tower, new Vector3(-legHalf, by, 0f), new Vector3(0.28f, 0.28f, f), palette.wood, false);
                BuildKit.Box("Brace", tower, new Vector3(legHalf, by, 0f), new Vector3(0.28f, 0.28f, f), palette.wood, false);
            }

            BuildKit.Box("Platform", tower, new Vector3(0f, platformY + platformThickness * 0.5f, 0f),
                         new Vector3(platformSize, platformThickness, platformSize), palette.wood);

            float railY = platformY + platformThickness + railHeight * 0.5f;
            float railOffset = platformSize * 0.5f - 0.1f;
            BuildKit.Box("Rail_N", tower, new Vector3(0f, railY, railOffset), new Vector3(platformSize, railHeight, 0.18f), palette.wood);
            BuildKit.Box("Rail_S", tower, new Vector3(0f, railY, -railOffset), new Vector3(platformSize, railHeight, 0.18f), palette.wood);
            BuildKit.Box("Rail_E", tower, new Vector3(railOffset, railY, 0f), new Vector3(0.18f, railHeight, platformSize), palette.wood);
            BuildKit.Box("Rail_W", tower, new Vector3(-railOffset, railY, 0f), new Vector3(0.18f, railHeight, platformSize), palette.wood);

            float postBase = platformY + platformThickness;
            float postOffset = platformSize * 0.5f - 0.25f;
            for (int i = 0; i < 4; i++)
            {
                float sx = (i == 0 || i == 3) ? -1f : 1f;
                float sz = (i < 2) ? -1f : 1f;
                BuildKit.Box("RoofPost", tower, new Vector3(sx * postOffset, postBase + roofPostHeight * 0.5f, sz * postOffset),
                             new Vector3(0.22f, roofPostHeight, 0.22f), palette.wood, false);
            }

            BuildKit.Box("Roof", tower, new Vector3(0f, postBase + roofPostHeight + 0.15f, 0f),
                         new Vector3(platformSize + 0.6f, 0.3f, platformSize + 0.6f), palette.concreteDark);

            Transform ladder = BuildKit.Empty("Ladder", tower, new Vector3(0f, 0f, legHalf + 0.35f)).transform;
            BuildKit.Box("Rail_L", ladder, new Vector3(-0.35f, platformY * 0.5f, 0f), new Vector3(0.12f, platformY, 0.12f), palette.metal, false);
            BuildKit.Box("Rail_R", ladder, new Vector3(0.35f, platformY * 0.5f, 0f), new Vector3(0.12f, platformY, 0.12f), palette.metal, false);
            int rungs = Mathf.Max(2, Mathf.RoundToInt(platformY / 0.4f));
            for (int i = 1; i < rungs; i++)
                BuildKit.Box("Rung", ladder, new Vector3(0f, platformY / rungs * i, 0f), new Vector3(0.82f, 0.07f, 0.07f), palette.metal, false);

            if (towerSpotlights) BuildSpotlight(tower, postBase + roofPostHeight - 0.4f, index);
        }

        void BuildSpotlight(Transform tower, float y, int index)
        {
            Transform pivot = BuildKit.Empty("SpotlightPivot", tower, new Vector3(0f, y, 0f)).transform;
            pivot.localRotation = Quaternion.Euler(35f, 0f, 0f);

            BuildKit.Box("Housing", pivot, new Vector3(0f, 0f, 0.25f), new Vector3(0.5f, 0.5f, 0.5f), palette.metal, false);

            Transform lightGo = BuildKit.Empty("Spotlight", pivot, new Vector3(0f, 0f, 0.5f)).transform;
            Light light = lightGo.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.96f, 0.85f);
            light.intensity = 12f;
            light.range = 80f;
            light.spotAngle = 34f;
            light.innerSpotAngle = 18f;
            light.shadows = LightShadows.None;

            TowerSpotlight sweep = pivot.gameObject.AddComponent<TowerSpotlight>();
            sweep.sweepAngle = 55f;
            sweep.sweepSpeed = 0.18f;
            sweep.phase = index * 0.27f;
        }

        // --------------------------------------------------------------- forest

        void BuildForest(Transform root)
        {
            Transform forest = BuildKit.Empty("Forest", root).transform;

            System.Random rng = new System.Random(seed);
            List<Vector2> placed = new List<Vector2>(treeCount);

            float limit = groundSize * 0.5f - 4f;
            float minDistSqr = treeSpacing * treeSpacing;
            int attempts = treeCount * 30;

            float keepOutX = compoundHalfWidth + 7f;
            float keepOutN = compoundNorth + 7f;
            float keepOutS = compoundSouth - 7f;

            while (placed.Count < treeCount && attempts-- > 0)
            {
                float x = (float)(rng.NextDouble() * 2.0 - 1.0) * limit;
                float z = (float)(rng.NextDouble() * 2.0 - 1.0) * limit;

                // Keep the compound clear, and leave the approach road open.
                if (Mathf.Abs(x) < keepOutX && z < keepOutN && z > keepOutS) continue;
                if (Mathf.Abs(x) < 7f && z < keepOutS) continue;

                Vector2 p = new Vector2(x, z);
                bool tooClose = false;
                for (int i = 0; i < placed.Count; i++)
                    if ((placed[i] - p).sqrMagnitude < minDistSqr) { tooClose = true; break; }

                if (tooClose) continue;

                placed.Add(p);
                BuildTree(forest, new Vector3(x, 0f, z), rng);
            }
        }

        void BuildTree(Transform parent, Vector3 position, System.Random rng)
        {
            float height = 4.5f + (float)rng.NextDouble() * 3.5f;
            float trunkDiameter = 0.32f + (float)rng.NextDouble() * 0.22f;
            float lean = (float)rng.NextDouble() * 4f - 2f;

            Transform tree = BuildKit.Empty("Tree", parent, position).transform;
            tree.localRotation = Quaternion.Euler(lean, (float)rng.NextDouble() * 360f, lean * 0.5f);

            BuildKit.Cylinder("Trunk", tree, new Vector3(0f, height * 0.5f, 0f), trunkDiameter, height, palette.bark);

            Material leaves = palette.foliage[rng.Next(palette.foliage.Length)];
            float canopy = height * (0.62f + (float)rng.NextDouble() * 0.18f);

            BuildKit.Sphere("Canopy_0", tree, new Vector3(0f, height * 0.78f, 0f),
                            new Vector3(canopy, canopy * 0.8f, canopy), leaves);
            BuildKit.Sphere("Canopy_1", tree, new Vector3(canopy * 0.16f, height * 0.98f, -canopy * 0.12f),
                            Vector3.one * (canopy * 0.72f), leaves);
            BuildKit.Sphere("Canopy_2", tree, new Vector3(-canopy * 0.18f, height * 1.1f, canopy * 0.14f),
                            Vector3.one * (canopy * 0.55f), leaves);
        }
    }
}
