using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Sweeping wall camera. Unlike a guard it never chases — it just feeds
    /// suspicion, and at full suspicion the alarm brings the guards to you.
    /// </summary>
    public class SecurityCamera : MonoBehaviour
    {
        public float sweepAngle = 45f;
        public float sweepSpeed = 0.14f;
        public float phase;

        public float viewRange = 20f;
        public float viewHalfAngle = 26f;
        public float reportStrength = 0.7f;

        [HideInInspector] public bool seesPlayer;

        float baseYaw;
        float basePitch;
        LevelMap map;

        void Awake()
        {
            Vector3 e = transform.localEulerAngles;
            baseYaw = e.y;
            basePitch = 12f;
            map = Object.FindAnyObjectByType<LevelMap>();
        }

        void Update()
        {
            float t = (Time.time * sweepSpeed + phase) * Mathf.PI * 2f;
            transform.localRotation = Quaternion.Euler(basePitch, baseYaw + Mathf.Sin(t) * sweepAngle, 0f);

            seesPlayer = false;

            AlertState alert = AlertState.Instance;
            if (alert == null) return;

            Transform player = alert.Player;
            if (player == null) return;

            if (!Vision.CanSee(transform.position, transform.forward, player, viewRange, viewHalfAngle, out float clarity))
                return;

            float weight = PrisonRules.SuspicionWeight(player.position, map);
            if (weight <= 0f) return;

            seesPlayer = true;
            alert.ReportSighting(player.position, weight * clarity * reportStrength);
        }
    }
}
