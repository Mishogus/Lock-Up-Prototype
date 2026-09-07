using System.Collections.Generic;
using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// A guard: walks his route, looks around, and comes after you if he sees you
    /// somewhere you should not be. Touch him and you are back in your cell.
    /// </summary>
    public class GuardAI : Npc
    {
        public enum State { Patrol, Investigate, Chase }

        [Header("Route")]
        public List<Vector3> route = new List<Vector3>();
        public float waitAtWaypoint = 2f;

        [Header("Senses")]
        public float viewRange = 22f;
        public float viewHalfAngle = 55f;
        public float catchDistance = 1.5f;

        [Header("State")]
        public State state = State.Patrol;

        int waypoint;
        float waitUntil;
        float investigateUntil;
        float scanOffset;
        LevelMap map;

        protected override void Awake()
        {
            base.Awake();
            map = Object.FindAnyObjectByType<LevelMap>();
            scanOffset = Random.value * 10f;
        }

        void Update()
        {
            Look();

            switch (state)
            {
                case State.Chase: Chase(); break;
                case State.Investigate: Investigate(); break;
                default: Patrol(); break;
            }
        }

        // ------------------------------------------------------------- senses

        void Look()
        {
            AlertState alert = AlertState.Instance;
            if (alert == null) return;

            Transform player = alert.Player;
            if (player == null) return;

            if (!Vision.CanSee(EyePosition, transform.forward, player, viewRange, viewHalfAngle, out float clarity))
                return;

            float weight = PrisonRules.SuspicionWeight(player.position, map);

            // Seen somewhere legitimate: a guard glances and thinks nothing of it.
            if (weight <= 0f && !alert.alarm) return;

            alert.ReportSighting(player.position, Mathf.Max(weight, 0.5f) * clarity * 1.6f);

            state = (alert.alarm || alert.suspicion > 0.55f) ? State.Chase : State.Investigate;
            investigateUntil = Time.time + 8f;
        }

        // ------------------------------------------------------------ behaviour

        void Patrol()
        {
            if (route.Count == 0) { ScanFromPost(); return; }

            if (Time.time < waitUntil) { Idle(); ScanFromPost(); return; }

            if (MoveTowards(route[waypoint % route.Count], walkSpeed))
            {
                waypoint = (waypoint + 1) % route.Count;
                waitUntil = Time.time + waitAtWaypoint;
            }
        }

        /// <summary>Standing post: sweep your head so you are not a statue.</summary>
        void ScanFromPost()
        {
            float sweep = Mathf.Sin((Time.time + scanOffset) * 0.5f) * 60f;
            transform.rotation = Quaternion.Euler(0f, StartYaw + sweep, 0f);
        }

        float startYaw = float.NaN;
        float StartYaw
        {
            get
            {
                if (float.IsNaN(startYaw)) startYaw = transform.eulerAngles.y;
                return startYaw;
            }
        }

        void Investigate()
        {
            AlertState alert = AlertState.Instance;
            if (alert == null) { state = State.Patrol; return; }

            if (Time.time > investigateUntil)
            {
                state = State.Patrol;
                waitUntil = Time.time + 1f;
                return;
            }

            if (MoveTowards(alert.lastKnownPlayerPosition, walkSpeed * 1.3f))
            {
                Idle();
                ScanFromPost();
            }
        }

        void Chase()
        {
            AlertState alert = AlertState.Instance;
            Transform player = alert != null ? alert.Player : null;

            if (alert == null || player == null) { state = State.Patrol; return; }

            // Lost him: go and check the last place he was seen.
            if (!alert.RecentlySeen && !alert.alarm)
            {
                state = State.Investigate;
                investigateUntil = Time.time + 6f;
                return;
            }

            Vector3 target = alert.alarm ? player.position : alert.lastKnownPlayerPosition;

            if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            {
                alert.Catch(player);
                state = State.Patrol;
                waitUntil = Time.time + 2f;
                return;
            }

            MoveTowards(target, runSpeed);
        }
    }
}
