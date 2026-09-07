using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// The one place that knows whether the prison has noticed you.
    ///
    /// Anything that can see the player (guards, cameras, tower searchlights) calls
    /// <see cref="ReportSighting"/>. Suspicion climbs while you are in view and
    /// decays when you are not; at full it trips the alarm and guards come for you.
    /// </summary>
    public class AlertState : MonoBehaviour
    {
        public static AlertState Instance { get; private set; }

        [Header("Suspicion")]
        [Range(0f, 1f)] public float suspicion;
        public float decayPerSecond = 0.18f;
        public float alarmDuration = 25f;

        [Header("State")]
        public bool alarm;
        public int timesCaught;

        public Vector3 lastKnownPlayerPosition;
        public float lastSightingTime = -999f;

        float alarmEndsAt;
        Transform player;

        public bool RecentlySeen { get { return Time.time - lastSightingTime < 4f; } }

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (alarm && Time.time >= alarmEndsAt)
            {
                alarm = false;
                suspicion = 0.35f;
            }

            if (!RecentlySeen)
                suspicion = Mathf.Max(0f, suspicion - decayPerSecond * Time.deltaTime);
        }

        /// <summary>Called by a watcher that currently has the player in view.
        /// <paramref name="strength"/> scales with how obvious the sighting is.</summary>
        public void ReportSighting(Vector3 playerPosition, float strength)
        {
            lastKnownPlayerPosition = playerPosition;
            lastSightingTime = Time.time;

            suspicion = Mathf.Min(1f, suspicion + strength * Time.deltaTime);
            if (suspicion >= 1f && !alarm) RaiseAlarm();
        }

        public void RaiseAlarm()
        {
            alarm = true;
            suspicion = 1f;
            alarmEndsAt = Time.time + alarmDuration;
        }

        /// <summary>A guard got hold of you: back to the cell.</summary>
        public void Catch(Transform caught)
        {
            timesCaught++;
            alarm = false;
            suspicion = 0f;
            lastSightingTime = -999f;

            LevelMap map = Object.FindAnyObjectByType<LevelMap>();
            Vector3 cell = map != null ? map.playerSpawn : Vector3.zero;

            CharacterController cc = caught.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            caught.position = cell;
            if (cc != null) cc.enabled = true;
        }

        public Transform Player
        {
            get
            {
                if (player == null)
                {
                    PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
                    if (pc != null) player = pc.transform;
                }
                return player;
            }
        }
    }
}
