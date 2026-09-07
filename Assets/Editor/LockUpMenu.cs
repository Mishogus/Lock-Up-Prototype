using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LockUp.EditorTools
{
    /// <summary>
    /// One click scene setup: sun, ground, prison, forest and a playable player rig.
    /// </summary>
    public static class LockUpMenu
    {
        const string ScenePath = "Assets/Scenes/Prison.unity";

        [MenuItem("Lock Up/Build Escape Scene (New Scene)", false, 0)]
        public static void BuildEscapeScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            LevelBuilder builder = CreateLevel();
            PlayerRig.Create(builder.PlayerSpawn);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            RegisterInBuildSettings();

            Debug.Log("Lock Up: built " + ScenePath + " - press Play and use WASD / mouse.");
        }

        [MenuItem("Lock Up/Add Level To Open Scene", false, 1)]
        public static void AddLevelToOpenScene()
        {
            Scene scene = SceneManager.GetActiveScene();

            LevelBuilder builder = Object.FindAnyObjectByType<LevelBuilder>();
            if (builder == null) builder = CreateLevel();
            else builder.Rebuild();

            if (Object.FindAnyObjectByType<PlayerController>() == null)
            {
                DisableStrayCameras();
                CreateLighting();
                PlayerRig.Create(builder.PlayerSpawn);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Lock Up: level added to '" + scene.name + "'. Save the scene to keep it.");
        }

        [MenuItem("Lock Up/Rebuild Level", false, 20)]
        public static void RebuildLevel()
        {
            LevelBuilder builder = Object.FindAnyObjectByType<LevelBuilder>();
            if (builder == null)
            {
                Debug.LogWarning("Lock Up: no LevelBuilder in the open scene. Use 'Build Escape Scene' first.");
                return;
            }

            builder.Rebuild();
            EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
        }

        // ------------------------------------------------------------ pieces

        static void CreateLighting()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
                if (lights[i].type == LightType.Directional && lights[i].enabled)
                {
                    RenderSettings.sun = lights[i];
                    ApplyAtmosphere();
                    return;
                }

            GameObject sunGo = new GameObject("Sun");
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.35f;
            sun.shadows = LightShadows.Soft;

            RenderSettings.sun = sun;
            ApplyAtmosphere();
        }

        static void ApplyAtmosphere()
        {
            // Sky ambient gives shadowed walls a believable bounce; the trilight
            // values are the fallback for scenes with no skybox assigned.
            if (RenderSettings.skybox != null)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1f;
            }
            else
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            }

            RenderSettings.ambientSkyColor = new Color(0.52f, 0.57f, 0.65f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.43f, 0.43f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.21f, 0.18f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.60f, 0.65f, 0.70f);
        }

        static LevelBuilder CreateLevel()
        {
            GameObject go = new GameObject("World");
            LevelBuilder builder = go.AddComponent<LevelBuilder>();
            builder.Build();
            return builder;
        }

        static void DisableStrayCameras()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                AudioListener listener = cameras[i].GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;

                cameras[i].enabled = false;
            }
        }

        static void RegisterInBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
                if (scenes[i].path == ScenePath) return;

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(updated, 0);
            updated[scenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
