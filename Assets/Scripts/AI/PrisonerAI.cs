using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// A fellow inmate. Follows the same timetable you are supposed to follow, which
    /// makes the other prisoners a live hint about where you ought to be.
    /// </summary>
    public class PrisonerAI : Npc
    {
        [Tooltip("Which of the available spots this inmate heads for.")]
        public int slot;

        LevelMap map;
        Vector3 target;
        DaySchedule.Phase lastPhase = (DaySchedule.Phase)(-1);
        float fidgetUntil;

        protected override void Awake()
        {
            base.Awake();
            map = Object.FindAnyObjectByType<LevelMap>();
            target = transform.position;
        }

        void Update()
        {
            DaySchedule.Phase phase = DaySchedule.Current;
            if (phase != lastPhase)
            {
                lastPhase = phase;
                target = DestinationFor(phase);
            }

            if (MoveTowards(target, walkSpeed))
            {
                // Loiter: turn on the spot now and then rather than freezing solid.
                if (Time.time > fidgetUntil)
                {
                    fidgetUntil = Time.time + 3f + Random.value * 4f;
                    transform.rotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
                }
                Idle();
            }
        }

        Vector3 DestinationFor(DaySchedule.Phase phase)
        {
            if (map == null) return transform.position;

            switch (phase)
            {
                case DaySchedule.Phase.Cells:
                    return LevelMap.Pick(map.cellBunks, slot, transform.position);

                case DaySchedule.Phase.Jobs:
                    return LevelMap.Pick(map.jobStations, slot, transform.position);

                case DaySchedule.Phase.Mess:
                    return LevelMap.Pick(map.messSeats, slot, transform.position);

                default:
                    return LevelMap.Pick(map.yardRoute, slot, transform.position);
            }
        }
    }
}
