using UnityEngine;

namespace LockUp
{
    /// <summary>Builds the player object + first person camera. Shared by the
    /// editor scene generator and the runtime bootstrap so both stay identical.</summary>
    public static class PlayerRig
    {
        public static GameObject Create(Vector3 position)
        {
            GameObject player = new GameObject("Player");
            player.transform.position = position;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.radius = 0.35f;
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.slopeLimit = 50f;
            cc.stepOffset = 0.4f;
            cc.skinWidth = 0.03f;

            GameObject camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 600f;
            camGo.AddComponent<AudioListener>();

            player.AddComponent<PlayerController>().cameraPivot = camGo.transform;
            return player;
        }
    }
}
