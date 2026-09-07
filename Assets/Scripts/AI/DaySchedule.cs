using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// The prison timetable. Everyone in the block looks at this to decide where
    /// they should be, which is also what makes being somewhere else suspicious.
    /// </summary>
    public class DaySchedule : MonoBehaviour
    {
        public enum Phase { Cells, Jobs, Mess, Yard }

        public static DaySchedule Instance { get; private set; }

        [Tooltip("Seconds each phase lasts. The sketch says a shift is about 3 hours.")]
        public float cellsDuration = 45f;
        public float jobsDuration = 120f;
        public float messDuration = 60f;
        public float yardDuration = 90f;

        public Phase current = Phase.Cells;
        public float timeLeft;

        void Awake()
        {
            Instance = this;
            timeLeft = DurationOf(current);
        }

        void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0f) return;

            current = Next(current);
            timeLeft = DurationOf(current);
        }

        public float DurationOf(Phase phase)
        {
            switch (phase)
            {
                case Phase.Jobs: return jobsDuration;
                case Phase.Mess: return messDuration;
                case Phase.Yard: return yardDuration;
                default: return cellsDuration;
            }
        }

        public static Phase Next(Phase phase)
        {
            switch (phase)
            {
                case Phase.Cells: return Phase.Jobs;
                case Phase.Jobs: return Phase.Mess;
                case Phase.Mess: return Phase.Yard;
                default: return Phase.Cells;
            }
        }

        public static string Label(Phase phase)
        {
            switch (phase)
            {
                case Phase.Cells: return "Lock-up";
                case Phase.Jobs: return "Work detail";
                case Phase.Mess: return "Mess";
                default: return "Yard time";
            }
        }

        public static Phase Current
        {
            get { return Instance != null ? Instance.current : Phase.Yard; }
        }
    }
}
