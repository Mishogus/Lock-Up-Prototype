using UnityEngine;

namespace LockUp
{
    /// <summary>Builds the blocky little people that populate the prison.</summary>
    public static class NpcFactory
    {
        public enum Role { Prisoner, Guard, HeadGuard, Supervisor }

        public static GameObject Create(Role role, string name, Vector3 position, float yaw,
                                        Transform parent, Palette p)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            CharacterController cc = go.AddComponent<CharacterController>();
            cc.radius = 0.32f;
            cc.height = 1.75f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 50f;

            Material uniform = UniformFor(role, p);
            Transform body = go.transform;

            BuildKit.Box("Leg_L", body, new Vector3(-0.14f, 0.38f, 0f), new Vector3(0.19f, 0.76f, 0.22f), uniform, false);
            BuildKit.Box("Leg_R", body, new Vector3(0.14f, 0.38f, 0f), new Vector3(0.19f, 0.76f, 0.22f), uniform, false);

            BuildKit.Box("Torso", body, new Vector3(0f, 1.16f, 0f), new Vector3(0.52f, 0.82f, 0.3f), uniform, false);
            BuildKit.Box("Arm_L", body, new Vector3(-0.34f, 1.15f, 0f), new Vector3(0.15f, 0.7f, 0.2f), uniform, false);
            BuildKit.Box("Arm_R", body, new Vector3(0.34f, 1.15f, 0f), new Vector3(0.15f, 0.7f, 0.2f), uniform, false);

            BuildKit.Sphere("Head", body, new Vector3(0f, 1.71f, 0f), Vector3.one * 0.27f, p.skin);

            if (role != Role.Prisoner)
            {
                Material capMat = role == Role.Supervisor ? p.supervisor : p.guardTrim;
                BuildKit.Box("Cap", body, new Vector3(0f, 1.87f, 0f), new Vector3(0.3f, 0.1f, 0.31f), capMat, false);
                BuildKit.Box("Peak", body, new Vector3(0f, 1.85f, 0.2f), new Vector3(0.28f, 0.04f, 0.12f), capMat, false);
                BuildKit.Box("Belt", body, new Vector3(0f, 0.82f, 0f), new Vector3(0.55f, 0.1f, 0.33f), p.guardTrim, false);
            }
            else
            {
                // Inmate number stencilled on the chest.
                BuildKit.Box("Number", body, new Vector3(0f, 1.3f, 0.16f), new Vector3(0.22f, 0.1f, 0.02f), p.linen, false);
            }

            return go;
        }

        static Material UniformFor(Role role, Palette p)
        {
            switch (role)
            {
                case Role.Guard: return p.guardUniform;
                case Role.HeadGuard: return p.headGuard;
                case Role.Supervisor: return p.supervisor;
                default: return p.prisoner;
            }
        }
    }
}
