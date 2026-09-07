using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Where you are allowed to be, and when. Guards only get suspicious about a
    /// prisoner who is somewhere the timetable does not put him — that is what
    /// turns the schedule into a stealth constraint rather than decoration.
    /// </summary>
    public static class PrisonRules
    {
        public const string Outside = "Outside";
        public const string Corridor = "Corridor";

        /// <summary>Name of the room containing a world position.</summary>
        public static string RoomAt(Vector3 world, LevelMap map)
        {
            if (map == null) return Outside;

            Vector3 local = world - map.origin;

            if (Mathf.Abs(local.x) > PrisonBlock.HalfWidth ||
                Mathf.Abs(local.z) > PrisonBlock.HalfDepth) return Outside;

            if (Mathf.Abs(local.z) <= PrisonBlock.CorridorHalf) return Corridor;

            bool north = local.z > 0f;
            for (int i = 0; i < PrisonBlock.Rooms.Length; i++)
            {
                PrisonBlock.Room r = PrisonBlock.Rooms[i];
                if (r.north == north && local.x >= r.x0 && local.x < r.x1) return r.name;
            }

            return Corridor;
        }

        /// <summary>Staff-only rooms. Being seen in one is always trouble.</summary>
        public static bool IsRestricted(string room)
        {
            return room == "Guards" || room == "Security" || room == "Supervisor" || room == "Stairs";
        }

        /// <summary>Is this a legitimate place to be during the current phase?</summary>
        public static bool IsAllowed(string room, DaySchedule.Phase phase)
        {
            if (IsRestricted(room)) return false;

            switch (phase)
            {
                case DaySchedule.Phase.Cells:
                    return room.StartsWith("Cell");

                case DaySchedule.Phase.Jobs:
                    return room.StartsWith("Job") || room == "Mess Hall" || room == Corridor;

                case DaySchedule.Phase.Mess:
                    return room == "Dining" || room == "Mess Hall" || room == Corridor;

                default: // Yard time
                    return room == Outside || room == "Gym" || room == "Showers" || room == Corridor;
            }
        }

        /// <summary>How hard a watcher should push suspicion for seeing the player here.</summary>
        public static float SuspicionWeight(Vector3 playerPosition, LevelMap map)
        {
            string room = RoomAt(playerPosition, map);

            if (IsRestricted(room)) return 1f;
            if (!IsAllowed(room, DaySchedule.Current)) return 0.6f;
            return 0f;
        }

        public static string Explain(Vector3 playerPosition, LevelMap map)
        {
            string room = RoomAt(playerPosition, map);

            if (IsRestricted(room)) return room + " - staff only";
            if (!IsAllowed(room, DaySchedule.Current)) return room + " - not where you should be";
            return room;
        }
    }
}
