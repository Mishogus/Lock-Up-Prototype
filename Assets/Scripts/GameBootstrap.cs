using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Makes Play work from any scene, even an empty one: builds the level if it is
    /// missing, drops in a player, then starts the running prison — timetable,
    /// alert state, cameras and the cast of guards and inmates.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Boot()
        {
            LevelBuilder builder = Object.FindAnyObjectByType<LevelBuilder>();
            if (builder == null)
                builder = new GameObject("World").AddComponent<LevelBuilder>();

            if (builder.LevelRoot == null) builder.Build();

            EnsureSun();

            if (Object.FindAnyObjectByType<PlayerController>() == null)
            {
                SilenceStrayCameras();
                PlayerRig.Create(builder.PlayerSpawn);
            }

            StartPrison(builder);

            Debug.Log("Lock Up: Level 1 running. WASD move, Shift run, Ctrl crouch, Esc frees the cursor. " +
                      "Stay where the timetable says you should be.");
        }

        /// <summary>Timetable, alert state, HUD, cameras and people.</summary>
        static void StartPrison(LevelBuilder builder)
        {
            if (AlertState.Instance == null)
            {
                GameObject systems = new GameObject("Systems");
                systems.AddComponent<AlertState>();
                systems.AddComponent<DaySchedule>();
                systems.AddComponent<PrisonHUD>();
            }

            LevelMap map = Object.FindAnyObjectByType<LevelMap>();
            if (map != null)
            {
                for (int i = 0; i < map.cameraMounts.Count; i++)
                {
                    Transform mount = map.cameraMounts[i];
                    if (mount == null || mount.GetComponent<SecurityCamera>() != null) continue;

                    SecurityCamera cam = mount.gameObject.AddComponent<SecurityCamera>();
                    cam.phase = i * 0.31f;
                }
            }

            if (Object.FindAnyObjectByType<GuardAI>() == null) builder.PopulateNpcs();
        }

        static void EnsureSun()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
                if (lights[i].type == LightType.Directional && lights[i].enabled) return;

            GameObject sunGo = new GameObject("Sun");
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.35f;
            sun.shadows = LightShadows.Soft;

            RenderSettings.sun = sun;
        }

        /// <summary>Disable a scene's leftover template camera so ours is the only view.</summary>
        static void SilenceStrayCameras()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                AudioListener listener = cameras[i].GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;

                cameras[i].enabled = false;
            }
        }
    }
}
