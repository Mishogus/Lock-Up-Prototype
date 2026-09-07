using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Sweeps a watchtower searchlight back and forth, and reports anyone it
    /// catches out in the open where they should not be.
    /// </summary>
    public class TowerSpotlight : MonoBehaviour
    {
        [Tooltip("Maximum yaw either side of the tower's forward direction.")]
        public float sweepAngle = 55f;

        [Tooltip("Sweeps per second.")]
        public float sweepSpeed = 0.18f;

        [Range(0f, 1f)]
        [Tooltip("Offset so neighbouring towers are out of sync.")]
        public float phase = 0f;

        [Header("Detection")]
        public float viewRange = 70f;
        public float viewHalfAngle = 17f;
        public float reportStrength = 0.9f;

        [HideInInspector] public bool seesPlayer;

        float basePitch;
        LevelMap map;

        void Awake()
        {
            basePitch = transform.localEulerAngles.x;
            map = Object.FindAnyObjectByType<LevelMap>();
        }

        void Update()
        {
            float t = (Time.time * sweepSpeed + phase) * Mathf.PI * 2f;
            transform.localRotation = Quaternion.Euler(basePitch, Mathf.Sin(t) * sweepAngle, 0f);

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
