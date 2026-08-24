using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-click prototype room builder.
// In Unity, use the menu: Lock Up > Build Prototype Room
public static class SceneBuilder
{
    [MenuItem("Lock Up/Build Prototype Room")]
    public static void BuildPrototypeRoom()
    {
        // Floor (30x30 units)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(3f, 1f, 3f);

        // Four walls forming a square room around the floor
        float half = 15f;
        float wallHeight = 4f;
        float wallThickness = 1f;

        CreateWall("Wall_North", new Vector3(0, wallHeight / 2f, half), new Vector3(30f, wallHeight, wallThickness));
        CreateWall("Wall_South", new Vector3(0, wallHeight / 2f, -half), new Vector3(30f, wallHeight, wallThickness));
        CreateWall("Wall_East", new Vector3(half, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, 30f));
        CreateWall("Wall_West", new Vector3(-half, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, 30f));

        // Player capsule with a CharacterController + our movement script
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 1f, 0f);
        cc.height = 2f;
        cc.radius = 0.5f;

        player.AddComponent<PlayerController>();

        // Camera: parented to the player, angled top-down view (fits a stealth prototype),
        // and uses a solid color instead of the skybox shader that errored earlier.
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCam.transform.SetParent(player.transform);
        mainCam.transform.localPosition = new Vector3(0f, 6f, -6f);
        mainCam.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.5f, 0.5f, 0.5f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("Prototype room built: floor, 4 walls, player with WASD movement, camera attached. Scene saved.");
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
    }
}
