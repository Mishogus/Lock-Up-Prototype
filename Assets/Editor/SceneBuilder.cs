using UnityEditor;
using UnityEditor.SceneManagement;
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
        ClearGenerated();
        GameObject root = new GameObject("Generated");

        // Footprint: X from -12 to 12 (width 24), Z from 0 to 100 (length 100)
        // Zones along Z: Cell Block [0,20] -> Work Area [20,45] -> Guard Room [45,65] -> Yard [65,100]
        float minX = -12f, maxX = 12f;
        float minZ = 0f, maxZ = 100f;
        float width = maxX - minX;
        float length = maxZ - minZ;

        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3((minX + maxX) / 2f, 0f, (minZ + maxZ) / 2f);
        floor.transform.localScale = new Vector3(width / 10f, 1f, length / 10f);

        // Outer perimeter walls (west, east, south = solid; north has the exit gap)
        CreateWall("Wall_West", root.transform, new Vector3(minX, WallHeight / 2f, (minZ + maxZ) / 2f), new Vector3(WallThickness, WallHeight, length));
        CreateWall("Wall_East", root.transform, new Vector3(maxX, WallHeight / 2f, (minZ + maxZ) / 2f), new Vector3(WallThickness, WallHeight, length));
        CreateWall("Wall_South", root.transform, new Vector3((minX + maxX) / 2f, WallHeight / 2f, minZ), new Vector3(width, WallHeight, WallThickness));

        // North wall with a gap in the middle for the exit doorway
        CreateDividerWithGap("Wall_North", root.transform, maxZ, minX, maxX, 4f);

        // Divider walls between zones, each with a 4-unit doorway gap in the middle
        CreateDividerWithGap("Divider_CellBlock_WorkArea", root.transform, 20f, minX, maxX, 4f);
        CreateDividerWithGap("Divider_WorkArea_GuardRoom", root.transform, 45f, minX, maxX, 4f);
        CreateDividerWithGap("Divider_GuardRoom_Yard", root.transform, 65f, minX, maxX, 4f);

        // Zone floor markers (visual only, no collision) so the areas are distinguishable
        CreateZoneMarker("CellBlock", root.transform, new Vector3(0f, 0f, 10f), width, 20f, new Color(0.4f, 0.4f, 0.5f));
        CreateZoneMarker("WorkArea", root.transform, new Vector3(0f, 0f, 32.5f), width, 25f, new Color(0.5f, 0.45f, 0.3f));
        CreateZoneMarker("GuardRoom", root.transform, new Vector3(0f, 0f, 55f), width, 20f, new Color(0.55f, 0.25f, 0.25f));
        CreateZoneMarker("Yard", root.transform, new Vector3(0f, 0f, 82.5f), width, 35f, new Color(0.3f, 0.5f, 0.3f));

        // Exit point marker just past the north wall gap
        CreateExitPoint(root.transform, new Vector3(0f, 0.1f, maxZ + 3f));

        // Player, spawned in the Cell Block
        GameObject player = CreatePlayer(root.transform, new Vector3(0f, 1f, 8f));

        FinishAndSave($"Full prison built: cell block, work area, guard room, yard, exit point. Player spawned in cell block. Scene saved.");
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

    // names used by older versions of this builder that didn't parent everything
    // under "Generated" (destroying Player also destroys its child Main Camera)
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
            // only remove root-level (unparented) objects, never touch things nested under Generated
            if (legacy != null && legacy.transform.parent == null)
            {
                Object.DestroyImmediate(legacy);
            }
        }
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

        PlayerController pc = player.AddComponent<PlayerController>();

        // hide the capsule's own body mesh from the first-person view so it doesn't block the camera
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
        mainCam.transform.localPosition = new Vector3(0f, 1.7f, 0.15f); // eye height, first-person
        mainCam.transform.localRotation = Quaternion.identity;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.5f, 0.5f, 0.5f);

        pc.cameraTransform = mainCam.transform;

        return player;
    }

    private static void CreateWall(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
    }

    // Builds a wall across [minX,maxX] at the given Z, split into two segments with a doorway gap in the middle
    private static void CreateDividerWithGap(string name, Transform parent, float z, float minX, float maxX, float gapWidth)
    {
        float totalWidth = maxX - minX;
        float segmentWidth = (totalWidth - gapWidth) / 2f;

        float leftCenter = minX + segmentWidth / 2f;
        float rightCenter = maxX - segmentWidth / 2f;

        CreateWall(name + "_Left", parent, new Vector3(leftCenter, WallHeight / 2f, z), new Vector3(segmentWidth, WallHeight, WallThickness));
        CreateWall(name + "_Right", parent, new Vector3(rightCenter, WallHeight / 2f, z), new Vector3(segmentWidth, WallHeight, WallThickness));
    }

    private static void CreateZoneMarker(string name, Transform parent, Vector3 center, float width, float depth, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = name + "_ZoneMarker";
        marker.transform.SetParent(parent);
        marker.transform.position = center + new Vector3(0f, 0.02f, 0f);
        marker.transform.localScale = new Vector3(width - WallThickness, 0.02f, depth);
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

    private static Material MakeColorMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        return mat;
    }

    private static void FinishAndSave(string logMessage)
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log(logMessage);
    }
}
