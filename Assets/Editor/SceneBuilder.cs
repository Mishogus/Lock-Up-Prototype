using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

// Prototype level builders.
// In Unity, use the menu: Lock Up > Build Full Prison  (or Build Single Room for a quick test)
public static class SceneBuilder
{
    private const float WallHeight = 4f;
    private const float WallThickness = 1f;

    [MenuItem("Lock Up/Build Full Prison")]
    public static void BuildFullPrison()
    {
        try
        {
            BuildFullPrisonInternal();
        }
        catch (System.Exception e)
        {
            Debug.LogError("BuildFullPrison failed: " + e);
        }
    }

    private static void BuildFullPrisonInternal()
    {
        ClearGenerated();
        GameObject root = new GameObject("Generated");

        // Main building footprint: X -20..20, Z -20..20
        // Stairwell annex attached on the east side: X 20..28, Z -20..20
        float bMinX = -20f, bMaxX = 20f, bMinZ = -20f, bMaxZ = 20f;
        float annexMinX = 20f, annexMaxX = 28f;

        float groundY = 0f;
        float cellFloorY = 5f;
        float roofY = 10f;

        // ---------------- GROUND FLOOR: Guard Room (south) + Work Area (north) ----------------
        Color groundWallColor = new Color(0.5f, 0.42f, 0.35f);
        BuildFloorSlab("GroundFloor", root.transform, bMinX, bMaxX, bMinZ, bMaxZ, groundY, new Color(0.55f, 0.5f, 0.45f));

        CreateWall("Ground_Wall_West", root.transform, new Vector3(bMinX, groundY + WallHeight / 2f, (bMinZ + bMaxZ) / 2f), new Vector3(WallThickness, WallHeight, bMaxZ - bMinZ), groundWallColor);
        CreateWall("Ground_Wall_South", root.transform, new Vector3((bMinX + bMaxX) / 2f, groundY + WallHeight / 2f, bMinZ), new Vector3(bMaxX - bMinX, WallHeight, WallThickness), groundWallColor);
        CreateWall("Ground_Wall_North", root.transform, new Vector3((bMinX + bMaxX) / 2f, groundY + WallHeight / 2f, bMaxZ), new Vector3(bMaxX - bMinX, WallHeight, WallThickness), groundWallColor);
        // east wall: one gap for the stairwell entrance
        CreateGappedWallAlongZ("Ground_Wall_East", root.transform, bMaxX, bMinZ, bMaxZ, new float[] { -18f }, 4f, groundY, WallHeight, groundWallColor);
        // split into Guard Room (south) / Work Area (north)
        CreateGappedWallAlongX("Ground_Divider", root.transform, 0f, bMinX, bMaxX, new float[] { 0f }, 4f, groundY, WallHeight, groundWallColor);

        CreateZoneMarker("GuardRoom", root.transform, new Vector3(0f, groundY, -10f), bMaxX - bMinX - WallThickness, 20f, new Color(0.55f, 0.25f, 0.25f));
        CreateZoneMarker("WorkArea", root.transform, new Vector3(0f, groundY, 10f), bMaxX - bMinX - WallThickness, 20f, new Color(0.5f, 0.45f, 0.3f));

        // ---------------- FLOOR 2: Cell Block (corridor on the east, cells on the west) ----------------
        Color cellWallColor = new Color(0.35f, 0.42f, 0.55f);
        Color cellDividerColor = new Color(0.28f, 0.33f, 0.45f);
        BuildFloorSlab("CellFloor", root.transform, bMinX, bMaxX, bMinZ, bMaxZ, cellFloorY, new Color(0.45f, 0.48f, 0.58f));

        CreateWall("Cell_Wall_West", root.transform, new Vector3(bMinX, cellFloorY + WallHeight / 2f, (bMinZ + bMaxZ) / 2f), new Vector3(WallThickness, WallHeight, bMaxZ - bMinZ), cellWallColor);
        CreateWall("Cell_Wall_South", root.transform, new Vector3((bMinX + bMaxX) / 2f, cellFloorY + WallHeight / 2f, bMinZ), new Vector3(bMaxX - bMinX, WallHeight, WallThickness), cellWallColor);
        CreateWall("Cell_Wall_North", root.transform, new Vector3((bMinX + bMaxX) / 2f, cellFloorY + WallHeight / 2f, bMaxZ), new Vector3(bMaxX - bMinX, WallHeight, WallThickness), cellWallColor);
        // east wall: one gap where the stairwell lands
        CreateGappedWallAlongZ("Cell_Wall_East", root.transform, bMaxX, bMinZ, bMaxZ, new float[] { -2f }, 4f, cellFloorY, WallHeight, cellWallColor);

        // corridor / cells divider at X=14, with a door gap per cell
        float corridorX = 14f;
        float[] cellCenters = { -16f, -8f, 0f, 8f, 16f };
        CreateGappedWallAlongZ("Cell_CorridorWall", root.transform, corridorX, bMinZ, bMaxZ, cellCenters, 3f, cellFloorY, WallHeight, cellDividerColor);

        // cross walls separating the 5 cells from each other
        float[] cellBoundaries = { -12f, -4f, 4f, 12f };
        for (int i = 0; i < cellBoundaries.Length; i++)
        {
            CreateWall("Cell_CrossWall_" + i, root.transform,
                new Vector3((bMinX + corridorX) / 2f, cellFloorY + WallHeight / 2f, cellBoundaries[i]),
                new Vector3(corridorX - bMinX, WallHeight, WallThickness), cellDividerColor);
        }

        CreateZoneMarker("Corridor", root.transform, new Vector3((corridorX + bMaxX) / 2f, cellFloorY, 0f), bMaxX - corridorX, bMaxZ - bMinZ, new Color(0.4f, 0.4f, 0.5f));
        for (int i = 0; i < cellCenters.Length; i++)
        {
            CreateZoneMarker("Cell" + (i + 1), root.transform, new Vector3((bMinX + corridorX) / 2f, cellFloorY, cellCenters[i]), corridorX - bMinX, 8f - WallThickness, new Color(0.35f, 0.35f, 0.4f));
        }

        // ---------------- ROOF: Yard (open-air, low fence instead of walls) ----------------
        float fenceHeight = 2.5f;
        Color fenceColor = new Color(0.35f, 0.55f, 0.4f);
        BuildFloorSlab("RoofFloor", root.transform, bMinX, bMaxX, bMinZ, bMaxZ, roofY, new Color(0.45f, 0.6f, 0.45f));

        CreateWall("Roof_Fence_West", root.transform, new Vector3(bMinX, roofY + fenceHeight / 2f, (bMinZ + bMaxZ) / 2f), new Vector3(WallThickness, fenceHeight, bMaxZ - bMinZ), fenceColor);
        CreateWall("Roof_Fence_South", root.transform, new Vector3((bMinX + bMaxX) / 2f, roofY + fenceHeight / 2f, bMinZ), new Vector3(bMaxX - bMinX, fenceHeight, WallThickness), fenceColor);
        CreateGappedWallAlongZ("Roof_Fence_East", root.transform, bMaxX, bMinZ, bMaxZ, new float[] { 18f }, 4f, roofY, fenceHeight, fenceColor);
        // north fence has the exit gap
        CreateGappedWallAlongX("Roof_Fence_North", root.transform, bMaxZ, bMinX, bMaxX, new float[] { 0f }, 4f, roofY, fenceHeight, fenceColor);

        CreateZoneMarker("Yard", root.transform, new Vector3(0f, roofY, 0f), bMaxX - bMinX - WallThickness, bMaxZ - bMinZ - WallThickness, new Color(0.3f, 0.5f, 0.3f));
        CreateExitPoint(root.transform, new Vector3(0f, roofY + 0.1f, bMaxZ + 3f));

        // ---------------- STAIRWELL ANNEX ----------------
        float annexCenterX = (annexMinX + annexMaxX) / 2f;

        // Flight 1: ground -> cell floor, runs Z -20..-2
        CreateStaircase("Stairs_Ground_to_Cell", root.transform, annexCenterX, -20f, -2f, groundY, cellFloorY - groundY, annexMaxX - annexMinX);
        // Landing between flights, Z -2..2, at cell floor height
        GameObject landing1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        landing1.name = "Stairs_Landing";
        landing1.transform.SetParent(root.transform);
        landing1.transform.position = new Vector3(annexCenterX, cellFloorY - 0.1f, 0f);
        landing1.transform.localScale = new Vector3(annexMaxX - annexMinX, 0.2f, 4f);
        // Flight 2: cell floor -> roof, runs Z 2..20
        CreateStaircase("Stairs_Cell_to_Roof", root.transform, annexCenterX, 2f, 20f, cellFloorY, roofY - cellFloorY, annexMaxX - annexMinX);

        // low outer guard rail on the annex's open side so players don't wander off the edge
        CreateWall("Annex_OuterRail", root.transform, new Vector3(annexMaxX, roofY + 0.5f, (bMinZ + bMaxZ) / 2f), new Vector3(0.3f, 1f, bMaxZ - bMinZ));

        // Player spawns in the middle cell (Cell3)
        GameObject player = CreatePlayer(root.transform, new Vector3(-3f, cellFloorY + 1f, 0f));

        FinishAndSave("Full prison built: 3-floor building (Guard Room + Work Area / Cell Block with 5 cells / rooftop Yard), stairwell, exit point. Scene saved.");
    }

    [MenuItem("Lock Up/Build Single Room (quick test)")]
    public static void BuildSingleRoom()
    {
        ClearGenerated();
        GameObject root = new GameObject("Generated");

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(3f, 1f, 3f);

        float half = 15f;
        CreateWall("Wall_North", root.transform, new Vector3(0, WallHeight / 2f, half), new Vector3(30f, WallHeight, WallThickness));
        CreateWall("Wall_South", root.transform, new Vector3(0, WallHeight / 2f, -half), new Vector3(30f, WallHeight, WallThickness));
        CreateWall("Wall_East", root.transform, new Vector3(half, WallHeight / 2f, 0), new Vector3(WallThickness, WallHeight, 30f));
        CreateWall("Wall_West", root.transform, new Vector3(-half, WallHeight / 2f, 0), new Vector3(WallThickness, WallHeight, 30f));

        GameObject player = CreatePlayer(root.transform, new Vector3(0f, 1f, 0f));

        FinishAndSave("Single test room built. Scene saved.");
    }

    // ---- helpers ----

    private static readonly string[] LegacyLooseNames =
    {
        "Floor", "Player", "Wall_North", "Wall_South", "Wall_East", "Wall_West"
    };

    private static void ClearGenerated()
    {
        GameObject existing = GameObject.Find("Generated");
        if (existing != null) Object.DestroyImmediate(existing);

        foreach (string name in LegacyLooseNames)
        {
            GameObject legacy = GameObject.Find(name);
            if (legacy != null && legacy.transform.parent == null)
            {
                Object.DestroyImmediate(legacy);
            }
        }
    }

    private static void BuildFloorSlab(string name, Transform parent, float minX, float maxX, float minZ, float maxZ, float y, Color? color = null)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = name;
        floor.transform.SetParent(parent);
        floor.transform.position = new Vector3((minX + maxX) / 2f, y, (minZ + maxZ) / 2f);
        floor.transform.localScale = new Vector3((maxX - minX) / 10f, 1f, (maxZ - minZ) / 10f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(color ?? new Color(0.6f, 0.6f, 0.62f));
    }

    private static GameObject CreatePlayer(Transform parent, Vector3 spawnPosition)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.SetParent(parent);
        player.transform.position = spawnPosition;

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1f, 0f);
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.stepOffset = 0.3f;

        PlayerController pc = player.AddComponent<PlayerController>();

        MeshRenderer bodyRenderer = player.GetComponent<MeshRenderer>();
        if (bodyRenderer != null) bodyRenderer.enabled = false;

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCam.transform.SetParent(player.transform);
        mainCam.transform.localPosition = new Vector3(0f, 1.7f, 0.15f);
        mainCam.transform.localRotation = Quaternion.identity;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.5f, 0.5f, 0.5f);

        pc.cameraTransform = mainCam.transform;

        return player;
    }

    private static readonly Color DefaultWallColor = new Color(0.55f, 0.55f, 0.58f);

    private static void CreateWall(string name, Transform parent, Vector3 position, Vector3 scale, Color? color = null)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(color ?? DefaultWallColor);
    }

    // wall running along X at a fixed Z, with door gaps (in X) at the given centers
    private static void CreateGappedWallAlongX(string name, Transform parent, float z, float minX, float maxX, float[] gapCenters, float gapWidth, float yBase, float wallHeight, Color? color = null)
    {
        List<float> centers = new List<float>(gapCenters);
        centers.Sort();
        float cursor = minX;
        for (int i = 0; i <= centers.Count; i++)
        {
            float segEnd = (i < centers.Count) ? centers[i] - gapWidth / 2f : maxX;
            if (segEnd > cursor + 0.01f)
            {
                float segLen = segEnd - cursor;
                float segCenter = cursor + segLen / 2f;
                CreateWall(name + "_Seg" + i, parent, new Vector3(segCenter, yBase + wallHeight / 2f, z), new Vector3(segLen, wallHeight, WallThickness), color);
            }
            if (i < centers.Count) cursor = centers[i] + gapWidth / 2f;
        }
    }

    // wall running along Z at a fixed X, with door gaps (in Z) at the given centers
    private static void CreateGappedWallAlongZ(string name, Transform parent, float x, float minZ, float maxZ, float[] gapCenters, float gapWidth, float yBase, float wallHeight, Color? color = null)
    {
        List<float> centers = new List<float>(gapCenters);
        centers.Sort();
        float cursor = minZ;
        for (int i = 0; i <= centers.Count; i++)
        {
            float segEnd = (i < centers.Count) ? centers[i] - gapWidth / 2f : maxZ;
            if (segEnd > cursor + 0.01f)
            {
                float segLen = segEnd - cursor;
                float segCenter = cursor + segLen / 2f;
                CreateWall(name + "_Seg" + i, parent, new Vector3(x, yBase + wallHeight / 2f, segCenter), new Vector3(WallThickness, wallHeight, segLen), color);
            }
            if (i < centers.Count) cursor = centers[i] + gapWidth / 2f;
        }
    }

    // solid stepped staircase (each step is a solid block up to its tread height, reliable for CharacterController)
    private static void CreateStaircase(string name, Transform parent, float xCenter, float zStart, float zEnd, float yBase, float totalRise, float width, Color? color = null)
    {
        float stepHeight = 0.25f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(totalRise / stepHeight));
        float actualStepHeight = totalRise / steps;
        float totalRun = zEnd - zStart;
        float stepDepth = totalRun / steps;

        for (int i = 0; i < steps; i++)
        {
            float topY = yBase + actualStepHeight * (i + 1);
            float blockHeight = topY - yBase;
            float z = zStart + stepDepth * (i + 0.5f);

            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = $"{name}_Step{i}";
            step.transform.SetParent(parent);
            // solid block from yBase up to topY, so there are no gaps underneath any step
            step.transform.position = new Vector3(xCenter, yBase + blockHeight / 2f, z);
            step.transform.localScale = new Vector3(width, blockHeight, stepDepth);
            step.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(color ?? new Color(0.75f, 0.55f, 0.2f));
        }
    }

    private static void CreateZoneMarker(string name, Transform parent, Vector3 center, float width, float depth, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = name + "_ZoneMarker";
        marker.transform.SetParent(parent);
        marker.transform.position = center + new Vector3(0f, 0.03f, 0f);
        marker.transform.localScale = new Vector3(width, 0.02f, depth);
        Object.DestroyImmediate(marker.GetComponent<BoxCollider>());
        marker.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(color);
    }

    private static void CreateExitPoint(Transform parent, Vector3 position)
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exit.name = "ExitPoint";
        exit.transform.SetParent(parent);
        exit.transform.position = position;
        exit.transform.localScale = new Vector3(4f, 0.2f, 4f);

        BoxCollider col = exit.GetComponent<BoxCollider>();
        col.isTrigger = true;

        exit.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(new Color(0.1f, 1f, 0.1f));
    }

    private static readonly Dictionary<Color, Material> MaterialCache = new Dictionary<Color, Material>();

    private static Material MakeColorMaterial(Color color)
    {
        if (MaterialCache.TryGetValue(color, out Material cached) && cached != null)
            return cached;

        const string folder = "Assets/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedMaterials");
        }

        string colorTag = $"{Mathf.RoundToInt(color.r * 255)}_{Mathf.RoundToInt(color.g * 255)}_{Mathf.RoundToInt(color.b * 255)}";
        string path = $"{folder}/Color_{colorTag}.mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            MaterialCache[color] = existing;
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        MaterialCache[color] = mat;
        return mat;
    }

    private static void FinishAndSave(string logMessage)
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log(logMessage);
    }
}
