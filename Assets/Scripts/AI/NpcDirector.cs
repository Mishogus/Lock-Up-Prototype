using System.Collections.Generic;
using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Populates the prison with the cast from the design sheet:
    /// 8 prisoners, 5 guards, 2 head guards and 1 supervisor.
    /// </summary>
    public static class NpcDirector
    {
        public const int Prisoners = 8;
        public const int Guards = 5;
        public const int HeadGuards = 2;

        public static void Populate(Transform parent, LevelMap map, Palette p)
        {
            Transform people = BuildKit.Empty("People", parent).transform;

            SpawnPrisoners(people, map, p);
            SpawnGuards(people, map, p);
            SpawnSupervisor(people, map, p);
        }

        static void SpawnPrisoners(Transform parent, LevelMap map, Palette p)
        {
            for (int i = 0; i < Prisoners; i++)
            {
                // Start them in the cells, one slot each.
                Vector3 at = LevelMap.Pick(map.cellBunks, i, map.playerSpawn) + new Vector3(0f, 0.1f, 0f);

                GameObject go = NpcFactory.Create(NpcFactory.Role.Prisoner, "Prisoner_" + (i + 1),
                                                  at, Random.value * 360f, parent, p);
                go.AddComponent<PrisonerAI>().slot = i;
            }
        }

        static void SpawnGuards(Transform parent, LevelMap map, Palette p)
        {
            // Two walk the corridor in opposite directions, the rest hold posts.
            for (int i = 0; i < Guards; i++)
            {
                bool patrols = i < 2;

                Vector3 at = patrols
                    ? LevelMap.Pick(map.corridorRoute, i * 2, map.playerSpawn)
                    : LevelMap.Pick(map.guardPosts, i - 2, map.playerSpawn);

                GameObject go = NpcFactory.Create(NpcFactory.Role.Guard, "Guard_" + (i + 1),
                                                  at + new Vector3(0f, 0.1f, 0f), i * 47f, parent, p);

                GuardAI ai = go.AddComponent<GuardAI>();
                if (patrols) ai.route = Route(map.corridorRoute, i == 1);
            }

            // Head guards work the yard, where an escape actually happens.
            for (int i = 0; i < HeadGuards; i++)
            {
                Vector3 at = LevelMap.Pick(map.yardRoute, i * 3, map.exitDoor);

                GameObject go = NpcFactory.Create(NpcFactory.Role.HeadGuard, "HeadGuard_" + (i + 1),
                                                  at + new Vector3(0f, 0.1f, 0f), i * 90f, parent, p);

                GuardAI ai = go.AddComponent<GuardAI>();
                ai.route = Route(map.yardRoute, i == 1);
                ai.viewRange = 26f;
                ai.walkSpeed = 2.4f;
            }
        }

        static void SpawnSupervisor(Transform parent, LevelMap map, Palette p)
        {
            GameObject go = NpcFactory.Create(NpcFactory.Role.Supervisor, "Supervisor",
                                              map.supervisorDesk + new Vector3(0f, 0.1f, 0f), 0f, parent, p);

            GuardAI ai = go.AddComponent<GuardAI>();
            ai.viewRange = 16f;
            ai.catchDistance = 1.7f;   // he is the one you least want to walk into
        }

        /// <summary>Copy of a route, optionally reversed so two walkers pass each other.</summary>
        static List<Vector3> Route(List<Vector3> source, bool reversed)
        {
            List<Vector3> copy = new List<Vector3>(source);
            if (reversed) copy.Reverse();
            return copy;
        }
    }
}
