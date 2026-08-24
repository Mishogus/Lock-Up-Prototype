using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Prototype level builders. Menu: Lock Up > Build Full Prison (Level: Easy)
public static class SceneBuilder
{
    private const float WallHeight = 3f;
    private const float WallThickness = 0.25f;
    private const float DoorWidth = 1.1f;
    private const float FloorSpacing = 3.3f;

    // shared building footprint for every floor
    private const float BMinX = -3f, BMaxX = 5f, BMinZ = -15f, BMaxZ = 3f;
    private const float AnnexMinX = 5f, AnnexMaxX = 7.5f;

    [MenuItem("Lock Up/Build Full Prison")]
    public static void BuildFullPrison()
    {
        try { BuildFullPrisonInternal(); }
        catch (System.Exception e) { Debug.LogError("BuildFullPrison failed: " + e); }
    }

    private static void BuildFullPrisonInternal()
    {
        ClearGenerated();
        GameObject root = new GameObject("Generated");

        float groundY = 0f;
        float cellY = FloorSpacing;
        float guardY = FloorSpacing * 2f;
        float roofY = FloorSpacing * 3f;

        BuildGroundFloor(root.transform, groundY);
        BuildCellFloor(root.transform, cellY);
        BuildGuardFloor(root.transform, guardY);
        BuildRoof(root.transform, roofY);
        BuildStairwell(root.transform, groundY, cellY, guardY, roofY);

        GameObject player = CreatePlayer(root.transform, new Vector3(-1.5f, cellY + 1f, -13.5f));

        FinishAndSave("Level: Easy. Full prison built - Work Area / Cell Block (5 cells) / Guard Floor (barracks, armory, break room, office) / rooftop Yard. Doors, labels, and furniture added. Scene saved.");
    }

    // ---------------- GROUND FLOOR: Work Area ----------------
    private static void BuildGroundFloor(Transform root, float y)
    {
        Color wallColor = new Color(0.5f, 0.42f, 0.35f);
        BuildFloorSlab("GroundFloor", root, BMinX, BMaxX, BMinZ, BMaxZ, y, new Color(0.55f, 0.5f, 0.45f));

        CreateWall("Ground_Wall_West", root, new Vector3(BMinX, y + WallHeight / 2f, (BMinZ + BMaxZ) / 2f), new Vector3(WallThickness, WallHeight, BMaxZ - BMinZ), wallColor);
        CreateWall("Ground_Wall_South", root, new Vector3((BMinX + BMaxX) / 2f, y + WallHeight / 2f, BMinZ), new Vector3(BMaxX - BMinX, WallHeight, WallThickness), wallColor);
        CreateWall("Ground_Wall_North", root, new Vector3((BMinX + BMaxX) / 2f, y + WallHeight / 2f, BMaxZ), new Vector3(BMaxX - BMinX, WallHeight, WallThickness), wallColor);

        float stairGapZ = BMinZ + 0.5f;
        CreateDoorAlongZ("Ground_Wall_East", root, BMaxX, BMinZ, BMaxZ, stairGapZ, y, wallColor);

        CreateRoomLabel(root, "WORK AREA", new Vector3((BMinX + BMaxX) / 2f, y + 0.03f, -6f));

        // deco: a workbench with a couple of crates
        Vector3 tableCenter = new Vector3(1f, y, -6f);
        CreateTable(root, tableCenter);
        CreatePropBox("Crate1", root, new Vector3(-2f, y + 0.4f, -12f), new Vector3(0.8f, 0.8f, 0.8f), new Color(0.45f, 0.32f, 0.2f));
        CreatePropBox("Crate2", root, new Vector3(-1.2f, y + 0.4f, -12f), new Vector3(0.8f, 0.8f, 0.8f), new Color(0.45f, 0.32f, 0.2f));
        CreatePropBox("Crate3", root, new Vector3(3.5f, y + 0.4f, -3f), new Vector3(0.8f, 0.8f, 0.8f), new Color(0.45f, 0.32f, 0.2f));
    }

    // ---------------- FLOOR 1: Cell Block ----------------
    private static void BuildCellFloor(Transform root, float y)
    {
        Color wallColor = new Color(0.35f, 0.42f, 0.55f);
        Color dividerColor = new Color(0.28f, 0.33f, 0.45f);
        BuildFloorSlab("CellFloor", root, BMinX, BMaxX, BMinZ, BMaxZ, y, new Color(0.45f, 0.48f, 0.58f));

        CreateWall("Cell_Wall_West", root, new Vector3(BMinX, y + WallHeight / 2f, (BMinZ + BMaxZ) / 2f), new Vector3(WallThickness, WallHeight, BMaxZ - BMinZ), wallColor);
        CreateWall("Cell_Wall_South", root, new Vector3((BMinX + BMaxX) / 2f, y + WallHeight / 2f, BMinZ), new Vector3(BMaxX - BMinX, WallHeight, WallThickness), wallColor);
        CreateWall("Cell_Wall_North", root, new Vector3((BMinX + BMaxX) / 2f, y + WallHeight / 2f, BMaxZ), new Vector3(BMaxX - BMinX, WallHeight, WallThickness), wallColor);

        float stairGapZ = BMinZ + 5.5f;
        CreateDoorAlongZ("Cell_Wall_East", root, BMaxX, BMinZ, BMaxZ, stairGapZ, y, wallColor);

        float corridorX = 0f;
        float cellMinX = BMinX;
        float[] cellBounds = { -15f, -12f, -9f, -6f, -3f, 0f };
        string[] cellNames = { "CELL 1", "CELL 2", "CELL 3", "CELL 4", "CELL 5" };

        // corridor/cell divider with a door per cell
        for (int i = 0; i < 5; i++)
        {
            float z0 = cellBounds[i], z1 = cellBounds[i + 1];
            float doorZ = (z0 + z1) / 2f;
            CreateDoorSegmentAlongZ("Cell_CorridorWall_" + i, root, corridorX, z0, z1, doorZ, y, dividerColor);

            if (i > 0)
            {
                CreateWall("Cell_CrossWall_" + i, root, new Vector3((cellMinX + corridorX) / 2f, y + WallHeight / 2f, z0), new Vector3(corridorX - cellMinX, WallHeight, WallThickness), dividerColor);
            }

            Vector3 cellCenter = new Vector3((cellMinX + corridorX) / 2f, y, (z0 + z1) / 2f);
            CreateRoomLabel(root, cellNames[i], cellCenter + new Vector3(0f, 0.03f, 0f));
            CreateBed(root, cellCenter, y);
            CreateToilet(root, cellCenter, y);
        }

        CreateRoomLabel(root, "CORRIDOR", new Vector3((corridorX + BMaxX) / 2f, y + 0.03f, -6f));
    }

    // ---------------- FLOOR 2 (NEW): Guard Floor ----------------
    private static void BuildGuardFloor(Transform root, float y)
    {
        Color wallColor = new Color(0.5f, 0.3f, 0.3f);
        Color dividerColor = new Color(0.4f, 0.24f, 0.24f);
        BuildFloorSlab("GuardFloor", root, BMinX, BMaxX, BMinZ, BMaxZ, y, new Color(0.55f, 0.35f, 0.35f));

        CreateWall("Guard_Wall_West", root, new Vector3(BMinX, y + WallHeight / 2f, (BMinZ + BMaxZ) / 2f), new Vector3(WallThickness, WallHeight, BMaxZ - BMinZ), wallColor);
        CreateWall("Guard_Wall_South", root, new Vector3((BMinX + BMaxX) / 2f, y + WallHeight / 2f, BMinZ), new Vector3(BMaxX - BMinX, WallHeight, WallThickness), wallColor);
        CreateWall("Guard_Wall_North", root, new Vector3((BMinX + BMaxX) / 2f, y + WallHeight / 2f, BMaxZ), new Vector3(BMaxX - BMinX, WallHeight, WallThickness), wallColor);

        float stairGapZ = BMinZ + 11.5f;
        CreateDoorAlongZ("Guard_Wall_East", root, BMaxX, BMinZ, BMaxZ, stairGapZ, y, wallColor);

        float corridorX = 0f;
        float roomMinX = BMinX;
        float[] roomBounds = { -15f, -10.5f, -6f, -1.5f, 3f };
        string[] roomNames = { "BARRACKS", "ARMORY", "BREAK ROOM", "OFFICE" };

        for (int i = 0; i < 4; i++)
        {
            float z0 = roomBounds[i], z1 = roomBounds[i + 1];
            float doorZ = (z0 + z1) / 2f;
            CreateDoorSegmentAlongZ("Guard_CorridorWall_" + i, root, corridorX, z0, z1, doorZ, y, dividerColor);

            if (i > 0)
            {
                CreateWall("Guard_CrossWall_" + i, root, new Vector3((roomMinX + corridorX) / 2f, y + WallHeight / 2f, z0), new Vector3(corridorX - roomMinX, WallHeight, WallThickness), dividerColor);
            }

            Vector3 roomCenter = new Vector3((roomMinX + corridorX) / 2f, y, (z0 + z1) / 2f);
            CreateRoomLabel(root, roomNames[i], roomCenter + new Vector3(0f, 0.03f, 0f));

            switch (i)
            {
                case 0: CreateBunkBeds(root, roomCenter, y); break;
                case 1: CreateWeaponRack(root, roomCenter, y); break;
                case 2: CreateTable(root, roomCenter); break;
                case 3: CreateDesk(root, roomCenter, y); break;
            }
        }

        CreateRoomLabel(root, "CORRIDOR", new Vector3((corridorX + BMaxX) / 2f, y + 0.03f, -6f));
    }

    // ---------------- ROOF: Yard ----------------
    private static void BuildRoof(Transform root, float y)
    {
        float fenceHeight = 2f;
        Color fenceColor = new Color(0.35f, 0.55f, 0.4f);
        BuildFloorSlab("RoofFloor", root, BMinX, BMaxX, BMinZ, BMaxZ, y, new Color(0.45f, 0.6f, 0.45f));

        CreateWall("Roof_Fence_West", root, new Vector3(BMinX, y + fenceHeight / 2f, (BMinZ + BMaxZ) / 2f), new Vector3(WallThickness, fenceHeight, BMaxZ - BMinZ), fenceColor);
        CreateWall("Roof_Fence_South", root, new Vector3((BMinX + BMaxX) / 2f, y + fenceHeight / 2f, BMinZ), new Vector3(BMaxX - BMinX, fenceHeight, WallThickness), fenceColor);

        float stairGapZ = BMinZ + 17.5f;
        CreateDoorAlongZ("Roof_Fence_East", root, BMaxX, BMinZ, BMaxZ, stairGapZ, y, fenceColor, fenceHeight);
        CreateGappedWallAlongX("Roof_Fence_North", root, BMaxZ, BMinX, BMaxX, new float[] { 1f }, DoorWidth + 0.4f, y, fenceHeight, fenceColor);

        CreateRoomLabel(root, "YARD", new Vector3((BMinX + BMaxX) / 2f, y + 0.03f, -6f));
        CreateExitPoint(root, new Vector3(1f, y + 0.1f, BMaxZ + 2f));

        CreatePropBox("Bench", root, new Vector3(3f, y + 0.25f, -3f), new Vector3(1.6f, 0.5f, 0.5f), new Color(0.4f, 0.3f, 0.2f));
    }

    // ---------------- STAIRWELL ----------------
    private static void BuildStairwell(Transform root, float groundY, float cellY, float guardY, float roofY)
    {
        float xCenter = (AnnexMinX + AnnexMaxX) / 2f;
        float width = AnnexMaxX - AnnexMinX;
        Color stairColor = new Color(0.75f, 0.55f, 0.2f);

        CreateStaircase("Stairs_1", root, xCenter, -15f, -10f, groundY, FloorSpacing, width, stairColor);
        CreateLanding("Landing_1", root, xCenter, -10f, -9f, cellY, width);

        CreateStaircase("Stairs_2", root, xCenter, -9f, -4f, cellY, FloorSpacing, width, stairColor);
        CreateLanding("Landing_2", root, xCenter, -4f, -3f, guardY, width);

        CreateStaircase("Stairs_3", root, xCenter, -3f, 2f, guardY, FloorSpacing, width, stairColor);
        CreateLanding("Landing_3", root, xCenter, 2f, 3f, roofY, width);

        CreateWall("Annex_OuterRail", root, new Vector3(AnnexMaxX, roofY + 0.5f, (BMinZ + BMaxZ) / 2f), new Vector3(0.2f, 1f, BMaxZ - BMinZ), new Color(0.4f, 0.4f, 0.4f));
    }

    // ---- structural helpers ----

    private static readonly string[] LegacyLooseNames = { "Floor", "Player", "Wall_North", "Wall_South", "Wall_East", "Wall_West" };

    private static void ClearGenerated()
    {
        GameObject existing = GameObject.Find("Generated");
        if (existing != null) Object.DestroyImmediate(existing);
        foreach (string name in LegacyLooseNames)
        {
            GameObject legacy = GameObject.Find(name);
            if (legacy != null && legacy.transform.parent == null) Object.DestroyImmediate(legacy);
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
        cc.radius = 0.4f;
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
        mainCam.transform.localPosition = new Vector3(0f, 1.6f, 0.1f);
        mainCam.transform.localRotation = Quaternion.identity;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
        mainCam.nearClipPlane = 0.05f;

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

        // simple baseboard trim along the bottom of the wall for a bit of detail
        GameObject baseboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseboard.name = name + "_Baseboard";
        baseboard.transform.SetParent(parent);
        float boardHeight = Mathf.Min(0.2f, scale.y * 0.15f);
        baseboard.transform.position = position - new Vector3(0f, scale.y / 2f - boardHeight / 2f, 0f);
        baseboard.transform.localScale = new Vector3(scale.x + 0.02f, boardHeight, scale.z + 0.02f);
        Object.DestroyImmediate(baseboard.GetComponent<BoxCollider>());
        Color c = color ?? DefaultWallColor;
        baseboard.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(c * 0.6f);
    }

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

    // wall along Z with a single door opening at doorZ (door object created too).
    // doorZ is clamped so both wall segments always have a safe minimum length.
    private static void CreateDoorAlongZ(string name, Transform parent, float x, float minZ, float maxZ, float doorZ, float yBase, Color? color = null, float wallHeight = WallHeight)
    {
        float margin = DoorWidth / 2f + 0.3f;
        doorZ = Mathf.Clamp(doorZ, minZ + margin, maxZ - margin);

        float belowLen = doorZ - DoorWidth / 2f - minZ;
        if (belowLen > 0.05f)
        {
            CreateWall(name + "_Below", parent, new Vector3(x, yBase + wallHeight / 2f, minZ + belowLen / 2f), new Vector3(WallThickness, wallHeight, belowLen), color);
        }

        float aboveLen = maxZ - (doorZ + DoorWidth / 2f);
        if (aboveLen > 0.05f)
        {
            CreateWall(name + "_Above", parent, new Vector3(x, yBase + wallHeight / 2f, doorZ + DoorWidth / 2f + aboveLen / 2f), new Vector3(WallThickness, wallHeight, aboveLen), color);
        }

        CreateDoorObject(name + "_Door", parent, new Vector3(x, yBase, doorZ), true, wallHeight);
    }

    // one room-side segment of a corridor divider wall along Z, with its own door
    private static void CreateDoorSegmentAlongZ(string name, Transform parent, float x, float z0, float z1, float doorZ, float yBase, Color? color = null)
    {
        CreateDoorAlongZ(name, parent, x, z0, z1, doorZ, yBase, color);
    }

    private static void CreateDoorObject(string name, Transform parent, Vector3 gapCenter, bool alongZ, float wallHeight = WallHeight)
    {
        GameObject doorRoot = new GameObject(name);
        doorRoot.transform.SetParent(parent);
        doorRoot.transform.position = gapCenter;

        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Panel";
        panel.transform.SetParent(doorRoot.transform);
        panel.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        panel.transform.localScale = alongZ
            ? new Vector3(WallThickness * 1.5f, wallHeight * 0.9f, DoorWidth - 0.05f)
            : new Vector3(DoorWidth - 0.05f, wallHeight * 0.9f, WallThickness * 1.5f);
        panel.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(new Color(0.4f, 0.25f, 0.15f));

        GameObject trigger = new GameObject("TriggerZone");
        trigger.transform.SetParent(doorRoot.transform);
        trigger.transform.localPosition = new Vector3(0f, wallHeight / 2f, 0f);
        BoxCollider box = trigger.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = alongZ ? new Vector3(2.2f, wallHeight, DoorWidth + 1.2f) : new Vector3(DoorWidth + 1.2f, wallHeight, 2.2f);

        AutoDoor autoDoor = trigger.AddComponent<AutoDoor>();
        autoDoor.doorPanel = panel.transform;
    }

    private static void CreateLanding(string name, Transform parent, float xCenter, float zStart, float zEnd, float y, float width)
    {
        GameObject landing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        landing.name = name;
        landing.transform.SetParent(parent);
        landing.transform.position = new Vector3(xCenter, y - 0.1f, (zStart + zEnd) / 2f);
        landing.transform.localScale = new Vector3(width, 0.2f, zEnd - zStart);
        landing.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(new Color(0.6f, 0.6f, 0.6f));
    }

    private static void CreateStaircase(string name, Transform parent, float xCenter, float zStart, float zEnd, float yBase, float totalRise, float width, Color? color = null)
    {
        float stepHeight = 0.33f;
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

    private static void CreateRoomLabel(Transform parent, string text, Vector3 position)
    {
        GameObject labelObj = new GameObject("Label_" + text.Replace(" ", ""));
        labelObj.transform.SetParent(parent);
        labelObj.transform.position = position;
        labelObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 1f, 1f, 0.85f);
        RectTransform rt = labelObj.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(4f, 1.5f);
    }

    private static void CreateExitPoint(Transform parent, Vector3 position)
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exit.name = "ExitPoint";
        exit.transform.SetParent(parent);
        exit.transform.position = position;
        exit.transform.localScale = new Vector3(2f, 0.2f, 2f);
        exit.GetComponent<BoxCollider>().isTrigger = true;
        exit.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(new Color(0.1f, 1f, 0.1f));
    }

    // ---- furniture helpers ----

    private static GameObject CreatePropBox(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMaterial(color);
        return box;
    }

    private static void CreateBed(Transform parent, Vector3 cellCenter, float y)
    {
        CreatePropBox("Bed", parent, cellCenter + new Vector3(0.6f, y + 0.25f, -0.6f), new Vector3(0.8f, 0.5f, 1.6f), new Color(0.6f, 0.5f, 0.3f));
    }

    private static void CreateToilet(Transform parent, Vector3 cellCenter, float y)
    {
        CreatePropBox("Toilet_Base", parent, cellCenter + new Vector3(-0.9f, y + 0.2f, 0.9f), new Vector3(0.45f, 0.4f, 0.45f), Color.white);
        CreatePropBox("Toilet_Tank", parent, cellCenter + new Vector3(-0.9f, y + 0.55f, 1.1f), new Vector3(0.4f, 0.3f, 0.12f), Color.white);
    }

    private static void CreateTable(Transform parent, Vector3 center)
    {
        float y = center.y;
        CreatePropBox("Table_Top", parent, center + new Vector3(0f, y + 0.5f, 0f), new Vector3(1.4f, 0.08f, 0.8f), new Color(0.5f, 0.35f, 0.2f));
        Vector3[] legOffsets = { new Vector3(0.6f, 0f, 0.3f), new Vector3(-0.6f, 0f, 0.3f), new Vector3(0.6f, 0f, -0.3f), new Vector3(-0.6f, 0f, -0.3f) };
        foreach (Vector3 off in legOffsets)
        {
            CreatePropBox("Table_Leg", parent, center + off + new Vector3(0f, y + 0.25f, 0f), new Vector3(0.08f, 0.5f, 0.08f), new Color(0.3f, 0.2f, 0.12f));
        }
        CreatePropBox("Chair1", parent, center + new Vector3(0f, y + 0.25f, 0.7f), new Vector3(0.4f, 0.5f, 0.4f), new Color(0.35f, 0.25f, 0.15f));
        CreatePropBox("Chair2", parent, center + new Vector3(0f, y + 0.25f, -0.7f), new Vector3(0.4f, 0.5f, 0.4f), new Color(0.35f, 0.25f, 0.15f));
    }

    private static void CreateBunkBeds(Transform parent, Vector3 center, float y)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 pos = center + new Vector3(side * 0.9f, 0f, 1f);
            CreatePropBox("Bunk_Lower", parent, pos + new Vector3(0f, y + 0.25f, 0f), new Vector3(0.8f, 0.4f, 1.8f), new Color(0.5f, 0.4f, 0.25f));
            CreatePropBox("Bunk_Upper", parent, pos + new Vector3(0f, y + 0.9f, 0f), new Vector3(0.8f, 0.4f, 1.8f), new Color(0.5f, 0.4f, 0.25f));
        }
    }

    private static void CreateWeaponRack(Transform parent, Vector3 center, float y)
    {
        CreatePropBox("Rack", parent, center + new Vector3(0.9f, y + 1f, 0f), new Vector3(0.2f, 2f, 1.6f), new Color(0.3f, 0.3f, 0.32f));
        CreatePropBox("Crate1", parent, center + new Vector3(-0.8f, y + 0.3f, 0.8f), new Vector3(0.6f, 0.6f, 0.6f), new Color(0.45f, 0.32f, 0.2f));
        CreatePropBox("Crate2", parent, center + new Vector3(-0.8f, y + 0.3f, -0.5f), new Vector3(0.6f, 0.6f, 0.6f), new Color(0.45f, 0.32f, 0.2f));
    }

    private static void CreateDesk(Transform parent, Vector3 center, float y)
    {
        CreatePropBox("Desk_Top", parent, center + new Vector3(0f, y + 0.5f, 0.5f), new Vector3(1.2f, 0.08f, 0.6f), new Color(0.45f, 0.3f, 0.18f));
        CreatePropBox("Desk_Leg1", parent, center + new Vector3(0.5f, y + 0.25f, 0.5f), new Vector3(0.08f, 0.5f, 0.5f), new Color(0.3f, 0.2f, 0.12f));
        CreatePropBox("Desk_Leg2", parent, center + new Vector3(-0.5f, y + 0.25f, 0.5f), new Vector3(0.08f, 0.5f, 0.5f), new Color(0.3f, 0.2f, 0.12f));
        CreatePropBox("Chair", parent, center + new Vector3(0f, y + 0.25f, -0.2f), new Vector3(0.4f, 0.5f, 0.4f), new Color(0.35f, 0.25f, 0.15f));
    }

    // ---- materials ----

    private static readonly Dictionary<Color, Material> MaterialCache = new Dictionary<Color, Material>();

    private static Material MakeColorMaterial(Color color)
    {
        if (MaterialCache.TryGetValue(color, out Material cached) && cached != null) return cached;

        const string folder = "Assets/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "GeneratedMaterials");

        string colorTag = $"{Mathf.RoundToInt(color.r * 255)}_{Mathf.RoundToInt(color.g * 255)}_{Mathf.RoundToInt(color.b * 255)}";
        string path = $"{folder}/Color_{colorTag}.mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) { MaterialCache[color] = existing; return existing; }

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
