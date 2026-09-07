using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// The furniture and fittings that dress the prison: bunks, toilets, mess hall
    /// tables, lockers, shower heads, barred doors and so on.
    ///
    /// Every prop is built in the local space of the transform it is parented to,
    /// with its origin on the floor, so rooms can place them by simple offsets.
    /// </summary>
    public static class Props
    {
        // ------------------------------------------------------------- cells

        /// <summary>Two tier bunk. Length runs along local Z.</summary>
        public static void Bunk(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform b = BuildKit.Empty("Bunk", parent, at).transform;
            b.localRotation = Quaternion.Euler(0f, yaw, 0f);

            const float len = 2.0f, wide = 0.85f;
            float[] tiers = { 0.42f, 1.42f };

            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0) ? -1f : 1f;
                float sz = (i < 2) ? -1f : 1f;
                BuildKit.Box("Post", b, new Vector3(sx * (wide * 0.5f - 0.05f), 0.95f, sz * (len * 0.5f - 0.05f)),
                             new Vector3(0.07f, 1.9f, 0.07f), p.metal, false);
            }

            for (int t = 0; t < tiers.Length; t++)
            {
                BuildKit.Box("Frame", b, new Vector3(0f, tiers[t], 0f),
                             new Vector3(wide, 0.08f, len), p.metal, t == 0);
                BuildKit.Box("Mattress", b, new Vector3(0f, tiers[t] + 0.11f, 0.06f),
                             new Vector3(wide - 0.08f, 0.14f, len - 0.12f), p.mattress, false);
                BuildKit.Box("Pillow", b, new Vector3(0f, tiers[t] + 0.2f, -len * 0.5f + 0.28f),
                             new Vector3(wide - 0.28f, 0.1f, 0.4f), p.linen, false);
            }
        }

        public static void Toilet(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform t = BuildKit.Empty("Toilet", parent, at).transform;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Pan", t, new Vector3(0f, 0.2f, 0f), new Vector3(0.42f, 0.4f, 0.55f), p.tile);
            BuildKit.Box("Seat", t, new Vector3(0f, 0.42f, 0.02f), new Vector3(0.46f, 0.06f, 0.5f), p.linen, false);
            BuildKit.Box("Cistern", t, new Vector3(0f, 0.55f, -0.36f), new Vector3(0.46f, 1.1f, 0.2f), p.steel, false);
        }

        public static void Sink(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform s = BuildKit.Empty("Sink", parent, at).transform;
            s.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Basin", s, new Vector3(0f, 0.85f, 0f), new Vector3(0.5f, 0.18f, 0.36f), p.steel);
            BuildKit.Box("Pedestal", s, new Vector3(0f, 0.42f, -0.04f), new Vector3(0.2f, 0.85f, 0.2f), p.steel, false);
            BuildKit.Cylinder("Tap", s, new Vector3(0f, 1.02f, -0.13f), 0.05f, 0.22f, p.steel, false);
        }

        public static void Shelf(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform s = BuildKit.Empty("Shelf", parent, at).transform;
            s.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Board", s, new Vector3(0f, 1.35f, 0f), new Vector3(0.9f, 0.05f, 0.28f), p.wood, false);
            BuildKit.Box("Bracket", s, new Vector3(-0.38f, 1.25f, 0f), new Vector3(0.04f, 0.2f, 0.24f), p.metal, false);
            BuildKit.Box("Bracket", s, new Vector3(0.38f, 1.25f, 0f), new Vector3(0.04f, 0.2f, 0.24f), p.metal, false);
        }

        // ------------------------------------------------------------- bars

        /// <summary>A barred gate leaf hinged on its left edge, swung open by
        /// <paramref name="openAngle"/> degrees. Width runs along local X.</summary>
        public static void BarredDoor(Transform parent, Vector3 hinge, float yaw, float width,
                                      float height, float openAngle, Palette p)
        {
            Transform d = BuildKit.Empty("BarredDoor", parent, hinge).transform;
            d.localRotation = Quaternion.Euler(0f, yaw + openAngle, 0f);

            // Frame: the two verticals carry the collision, the infill bars do not.
            BuildKit.Box("Stile", d, new Vector3(0.04f, height * 0.5f, 0f), new Vector3(0.08f, height, 0.08f), p.bars);
            BuildKit.Box("Stile", d, new Vector3(width - 0.04f, height * 0.5f, 0f), new Vector3(0.08f, height, 0.08f), p.bars);
            BuildKit.Box("Rail", d, new Vector3(width * 0.5f, height - 0.05f, 0f), new Vector3(width, 0.09f, 0.07f), p.bars, false);
            BuildKit.Box("Rail", d, new Vector3(width * 0.5f, 0.05f, 0f), new Vector3(width, 0.09f, 0.07f), p.bars, false);
            BuildKit.Box("Rail", d, new Vector3(width * 0.5f, height * 0.55f, 0f), new Vector3(width, 0.07f, 0.06f), p.bars, false);

            int bars = Mathf.Max(3, Mathf.RoundToInt(width / 0.16f));
            for (int i = 1; i < bars; i++)
            {
                float x = width * i / bars;
                BuildKit.Box("Bar", d, new Vector3(x, height * 0.5f, 0f),
                             new Vector3(0.045f, height - 0.1f, 0.045f), p.bars, false);
            }
        }

        /// <summary>Vertical bars filling a window opening. Width along local X.</summary>
        public static void WindowBars(Transform parent, Vector3 at, float yaw, float width, float height, Palette p)
        {
            Transform w = BuildKit.Empty("WindowBars", parent, at).transform;
            w.localRotation = Quaternion.Euler(0f, yaw, 0f);

            int bars = Mathf.Max(2, Mathf.RoundToInt(width / 0.18f));
            for (int i = 1; i < bars; i++)
            {
                float x = -width * 0.5f + width * i / bars;
                BuildKit.Box("Bar", w, new Vector3(x, height * 0.5f, 0f),
                             new Vector3(0.04f, height, 0.04f), p.bars, false);
            }

            BuildKit.Box("Sill", w, new Vector3(0f, 0f, 0f), new Vector3(width, 0.05f, 0.3f), p.concreteDark, false);
        }

        /// <summary>Solid swinging door (offices, kitchen). Hinged on its left edge.</summary>
        public static void Door(Transform parent, Vector3 hinge, float yaw, float width,
                                float height, float openAngle, Palette p)
        {
            Transform d = BuildKit.Empty("Door", parent, hinge).transform;
            d.localRotation = Quaternion.Euler(0f, yaw + openAngle, 0f);

            BuildKit.Box("Leaf", d, new Vector3(width * 0.5f, height * 0.5f, 0f),
                         new Vector3(width, height, 0.06f), p.wood);
            BuildKit.Box("Handle", d, new Vector3(width - 0.14f, height * 0.45f, 0.07f),
                         new Vector3(0.12f, 0.04f, 0.04f), p.steel, false);
        }

        // ------------------------------------------------------------- canteen

        /// <summary>Mess table with a bench either side. Length runs along local Z.</summary>
        public static void MessTable(Transform parent, Vector3 at, float yaw, float length, Palette p)
        {
            Transform t = BuildKit.Empty("MessTable", parent, at).transform;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Top", t, new Vector3(0f, 0.75f, 0f), new Vector3(0.9f, 0.07f, length), p.wood);
            BuildKit.Box("Leg", t, new Vector3(0f, 0.37f, -length * 0.5f + 0.3f), new Vector3(0.7f, 0.75f, 0.08f), p.metal, false);
            BuildKit.Box("Leg", t, new Vector3(0f, 0.37f, length * 0.5f - 0.3f), new Vector3(0.7f, 0.75f, 0.08f), p.metal, false);

            for (int s = -1; s <= 1; s += 2)
            {
                BuildKit.Box("Bench", t, new Vector3(s * 0.78f, 0.45f, 0f), new Vector3(0.34f, 0.06f, length - 0.2f), p.wood);
                BuildKit.Box("BenchLeg", t, new Vector3(s * 0.78f, 0.22f, -length * 0.5f + 0.4f), new Vector3(0.3f, 0.45f, 0.06f), p.metal, false);
                BuildKit.Box("BenchLeg", t, new Vector3(s * 0.78f, 0.22f, length * 0.5f - 0.4f), new Vector3(0.3f, 0.45f, 0.06f), p.metal, false);
            }
        }

        /// <summary>Serving counter with a sneeze guard. Length runs along local X.</summary>
        public static void ServingCounter(Transform parent, Vector3 at, float yaw, float length, Palette p)
        {
            Transform c = BuildKit.Empty("ServingCounter", parent, at).transform;
            c.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Body", c, new Vector3(0f, 0.45f, 0f), new Vector3(length, 0.9f, 0.75f), p.steel);
            BuildKit.Box("Top", c, new Vector3(0f, 0.93f, 0.02f), new Vector3(length + 0.1f, 0.06f, 0.85f), p.steel, false);

            // Trays sunk into the top.
            int wells = Mathf.Max(2, Mathf.RoundToInt(length / 1.1f));
            for (int i = 0; i < wells; i++)
            {
                float x = -length * 0.5f + length * (i + 0.5f) / wells;
                BuildKit.Box("Tray", c, new Vector3(x, 0.97f, 0.02f),
                             new Vector3(length / wells - 0.2f, 0.04f, 0.55f), p.metal, false);
            }

            BuildKit.Box("Guard", c, new Vector3(0f, 1.45f, -0.3f), new Vector3(length, 0.05f, 0.5f), p.steel, false);
            BuildKit.Box("GuardPost", c, new Vector3(-length * 0.5f + 0.2f, 1.2f, -0.3f), new Vector3(0.05f, 0.5f, 0.05f), p.steel, false);
            BuildKit.Box("GuardPost", c, new Vector3(length * 0.5f - 0.2f, 1.2f, -0.3f), new Vector3(0.05f, 0.5f, 0.05f), p.steel, false);
        }

        public static void Stove(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform s = BuildKit.Empty("Stove", parent, at).transform;
            s.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Body", s, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.8f), p.steel);
            BuildKit.Box("Hood", s, new Vector3(0f, 1.95f, -0.1f), new Vector3(1.8f, 0.5f, 0.9f), p.metal, false);
            for (int i = 0; i < 4; i++)
            {
                float x = -0.55f + (i % 2) * 1.1f;
                float z = -0.2f + (i / 2) * 0.4f;
                BuildKit.Cylinder("Burner", s, new Vector3(x, 0.92f, z), 0.34f, 0.05f, p.metal, false);
            }
        }

        public static void Fridge(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform f = BuildKit.Empty("Fridge", parent, at).transform;
            f.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Body", f, new Vector3(0f, 1.0f, 0f), new Vector3(1.0f, 2.0f, 0.75f), p.steel);
            BuildKit.Box("Seam", f, new Vector3(0f, 1.25f, 0.39f), new Vector3(1.0f, 0.03f, 0.02f), p.metal, false);
            BuildKit.Box("Handle", f, new Vector3(0.38f, 1.5f, 0.4f), new Vector3(0.05f, 0.4f, 0.05f), p.metal, false);
        }

        // ------------------------------------------------------------- offices

        public static void Desk(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform d = BuildKit.Empty("Desk", parent, at).transform;
            d.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Top", d, new Vector3(0f, 0.74f, 0f), new Vector3(1.6f, 0.06f, 0.8f), p.wood);
            BuildKit.Box("Pedestal", d, new Vector3(-0.6f, 0.36f, 0f), new Vector3(0.4f, 0.72f, 0.72f), p.metal, false);
            BuildKit.Box("Leg", d, new Vector3(0.72f, 0.36f, 0f), new Vector3(0.06f, 0.72f, 0.7f), p.metal, false);
            BuildKit.Box("Monitor", d, new Vector3(0.25f, 0.99f, -0.2f), new Vector3(0.55f, 0.36f, 0.05f), p.metal, false);
            BuildKit.Box("Stand", d, new Vector3(0.25f, 0.8f, -0.2f), new Vector3(0.14f, 0.14f, 0.16f), p.metal, false);
        }

        public static void Chair(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform c = BuildKit.Empty("Chair", parent, at).transform;
            c.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Seat", c, new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.07f, 0.45f), p.wood);
            BuildKit.Box("Back", c, new Vector3(0f, 0.72f, -0.2f), new Vector3(0.45f, 0.5f, 0.06f), p.wood, false);
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0) ? -1f : 1f;
                float sz = (i < 2) ? -1f : 1f;
                BuildKit.Box("Leg", c, new Vector3(sx * 0.18f, 0.22f, sz * 0.18f), new Vector3(0.05f, 0.45f, 0.05f), p.metal, false);
            }
        }

        /// <summary>Bank of lockers. Width runs along local X.</summary>
        public static void Lockers(Transform parent, Vector3 at, float yaw, int count, Palette p)
        {
            Transform l = BuildKit.Empty("Lockers", parent, at).transform;
            l.localRotation = Quaternion.Euler(0f, yaw, 0f);

            const float w = 0.45f;
            float total = w * count;
            BuildKit.Box("Body", l, new Vector3(0f, 0.95f, 0f), new Vector3(total, 1.9f, 0.5f), p.metal);

            for (int i = 0; i < count; i++)
            {
                float x = -total * 0.5f + w * (i + 0.5f);
                BuildKit.Box("Seam", l, new Vector3(x + w * 0.5f, 0.95f, 0.26f), new Vector3(0.02f, 1.9f, 0.02f), p.concreteDark, false);
                BuildKit.Box("Vent", l, new Vector3(x, 1.7f, 0.26f), new Vector3(w - 0.14f, 0.12f, 0.02f), p.concreteDark, false);
                BuildKit.Box("Handle", l, new Vector3(x + w * 0.3f, 1.0f, 0.28f), new Vector3(0.04f, 0.14f, 0.04f), p.steel, false);
            }
        }

        // ------------------------------------------------------------- showers

        public static void ShowerHead(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform s = BuildKit.Empty("ShowerHead", parent, at).transform;
            s.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Cylinder("Riser", s, new Vector3(0f, 1.1f, 0f), 0.05f, 2.2f, p.steel, false);
            BuildKit.Box("Arm", s, new Vector3(0f, 2.15f, 0.18f), new Vector3(0.05f, 0.05f, 0.36f), p.steel, false);
            BuildKit.Cylinder("Head", s, new Vector3(0f, 2.08f, 0.34f), 0.18f, 0.08f, p.steel, false);
            BuildKit.Box("Valve", s, new Vector3(0f, 1.3f, 0.1f), new Vector3(0.12f, 0.12f, 0.14f), p.metal, false);
        }

        public static void Drain(Transform parent, Vector3 at, Palette p)
        {
            BuildKit.Cylinder("Drain", parent, at + new Vector3(0f, 0.01f, 0f), 0.3f, 0.02f, p.metal, false);
        }

        public static void Bench(Transform parent, Vector3 at, float yaw, float length, Palette p)
        {
            Transform b = BuildKit.Empty("Bench", parent, at).transform;
            b.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Seat", b, new Vector3(0f, 0.45f, 0f), new Vector3(0.4f, 0.07f, length), p.wood);
            BuildKit.Box("Leg", b, new Vector3(0f, 0.22f, -length * 0.5f + 0.3f), new Vector3(0.34f, 0.45f, 0.07f), p.metal, false);
            BuildKit.Box("Leg", b, new Vector3(0f, 0.22f, length * 0.5f - 0.3f), new Vector3(0.34f, 0.45f, 0.07f), p.metal, false);
        }

        // ------------------------------------------------------------- yard

        /// <summary>Walk-through metal detector arch. Returns the arch so a detector
        /// component can be attached.</summary>
        public static Transform MetalDetector(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform m = BuildKit.Empty("MetalDetector", parent, at).transform;
            m.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Post_L", m, new Vector3(-0.75f, 1.1f, 0f), new Vector3(0.22f, 2.2f, 0.5f), p.steel);
            BuildKit.Box("Post_R", m, new Vector3(0.75f, 1.1f, 0f), new Vector3(0.22f, 2.2f, 0.5f), p.steel);
            BuildKit.Box("Head", m, new Vector3(0f, 2.3f, 0f), new Vector3(1.72f, 0.2f, 0.5f), p.steel, false);
            BuildKit.Box("Lamp", m, new Vector3(0f, 2.3f, 0.26f), new Vector3(0.3f, 0.08f, 0.03f), p.paint, false);
            return m;
        }

        /// <summary>Wall mounted security camera. Returns the yoke, which is what
        /// sweeps — attach the watcher component to it.</summary>
        public static Transform SecurityCamera(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform mount = BuildKit.Empty("CameraMount", parent, at).transform;
            mount.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Bracket", mount, new Vector3(0f, 0f, -0.12f), new Vector3(0.1f, 0.1f, 0.24f), p.metal, false);

            Transform yoke = BuildKit.Empty("CameraYoke", mount, Vector3.zero).transform;
            BuildKit.Box("Body", yoke, new Vector3(0f, 0f, 0.2f), new Vector3(0.18f, 0.18f, 0.4f), p.metal, false);
            BuildKit.Cylinder("Lens", yoke, new Vector3(0f, 0f, 0.42f), 0.12f, 0.06f, p.steel, false);
            BuildKit.Box("Led", yoke, new Vector3(0.07f, 0.07f, 0.4f), new Vector3(0.03f, 0.03f, 0.03f), p.paint, false);

            return yoke;
        }

        public static void Crate(Transform parent, Vector3 at, float yaw, float size, Palette p)
        {
            Transform c = BuildKit.Empty("Crate", parent, at).transform;
            c.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Body", c, new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size, size), p.wood);
            BuildKit.Box("Band", c, new Vector3(0f, size * 0.5f, 0f), new Vector3(size + 0.02f, 0.06f, size + 0.02f), p.metal, false);
        }

        /// <summary>Workshop bench with a vice and a tool board behind it.</summary>
        public static void Workbench(Transform parent, Vector3 at, float yaw, float length, Palette p)
        {
            Transform b = BuildKit.Empty("Workbench", parent, at).transform;
            b.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Top", b, new Vector3(0f, 0.9f, 0f), new Vector3(length, 0.1f, 0.8f), p.wood);
            BuildKit.Box("Leg", b, new Vector3(-length * 0.5f + 0.2f, 0.45f, 0f), new Vector3(0.1f, 0.9f, 0.7f), p.metal, false);
            BuildKit.Box("Leg", b, new Vector3(length * 0.5f - 0.2f, 0.45f, 0f), new Vector3(0.1f, 0.9f, 0.7f), p.metal, false);
            BuildKit.Box("Vice", b, new Vector3(length * 0.35f, 1.02f, 0.2f), new Vector3(0.24f, 0.22f, 0.3f), p.steel, false);
            BuildKit.Box("ToolBoard", b, new Vector3(0f, 1.7f, -0.45f), new Vector3(length, 1.0f, 0.05f), p.wood, false);
            for (int i = 0; i < 5; i++)
                BuildKit.Box("Tool", b, new Vector3(-length * 0.4f + length * 0.2f * i, 1.75f, -0.4f),
                             new Vector3(0.06f, 0.4f, 0.04f), p.metal, false);
        }

        public static void WashingMachine(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform w = BuildKit.Empty("WashingMachine", parent, at).transform;
            w.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Body", w, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.8f), p.steel);
            BuildKit.Cylinder("Door", w, new Vector3(0f, 0.5f, 0.41f), 0.5f, 0.04f, p.metal, false);
            BuildKit.Box("Panel", w, new Vector3(0f, 0.86f, 0.3f), new Vector3(0.8f, 0.1f, 0.2f), p.metal, false);
        }

        public static void WeightBench(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform b = BuildKit.Empty("WeightBench", parent, at).transform;
            b.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Pad", b, new Vector3(0f, 0.45f, 0f), new Vector3(0.35f, 0.12f, 1.5f), p.mattress);
            BuildKit.Box("Frame", b, new Vector3(0f, 0.2f, 0f), new Vector3(0.12f, 0.4f, 1.3f), p.metal, false);
            BuildKit.Box("Upright", b, new Vector3(-0.45f, 0.65f, -0.5f), new Vector3(0.09f, 1.3f, 0.09f), p.metal, false);
            BuildKit.Box("Upright", b, new Vector3(0.45f, 0.65f, -0.5f), new Vector3(0.09f, 1.3f, 0.09f), p.metal, false);
            GameObject bar = BuildKit.Cylinder("Bar", b, new Vector3(0f, 1.25f, -0.5f), 0.06f, 1.8f, p.steel, false);
            bar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            for (int s = -1; s <= 1; s += 2)
                BuildKit.Cylinder("Plate", b, new Vector3(s * 0.72f, 1.25f, -0.5f), 0.5f, 0.09f, p.metal, false)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        public static void WeightRack(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform r = BuildKit.Empty("WeightRack", parent, at).transform;
            r.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Frame", r, new Vector3(0f, 0.5f, 0f), new Vector3(2.2f, 0.12f, 0.5f), p.metal);
            BuildKit.Box("Frame", r, new Vector3(0f, 0.95f, -0.15f), new Vector3(2.2f, 0.12f, 0.5f), p.metal, false);
            BuildKit.Box("Leg", r, new Vector3(-1f, 0.5f, 0f), new Vector3(0.1f, 1f, 0.5f), p.metal, false);
            BuildKit.Box("Leg", r, new Vector3(1f, 0.5f, 0f), new Vector3(0.1f, 1f, 0.5f), p.metal, false);

            for (int i = 0; i < 4; i++)
            {
                float x = -0.75f + i * 0.5f;
                BuildKit.Sphere("Weight", r, new Vector3(x, 0.66f, 0.05f), Vector3.one * 0.22f, p.steel);
                BuildKit.Sphere("Weight", r, new Vector3(x, 1.11f, -0.12f), Vector3.one * 0.18f, p.steel);
            }
        }

        /// <summary>Flight of steps climbing along local +Z.</summary>
        public static void Stairs(Transform parent, Vector3 at, float yaw, float width,
                                  float rise, int steps, Palette p)
        {
            Transform s = BuildKit.Empty("Stairs", parent, at).transform;
            s.localRotation = Quaternion.Euler(0f, yaw, 0f);

            const float tread = 0.32f;
            for (int i = 0; i < steps; i++)
            {
                float h = rise * (i + 1);
                BuildKit.Box("Step", s, new Vector3(0f, h * 0.5f, tread * (i + 0.5f)),
                             new Vector3(width, h, tread), p.concrete);
            }

            float top = rise * steps;
            BuildKit.Box("Landing", s, new Vector3(0f, top - 0.1f, tread * steps + 1f),
                         new Vector3(width, 0.2f, 2f), p.concrete);
            BuildKit.Box("Rail", s, new Vector3(width * 0.5f - 0.06f, top + 0.5f, tread * steps * 0.5f),
                         new Vector3(0.08f, 1f, tread * steps), p.metal, false);
        }

        /// <summary>Wire fence run between two points, posts plus rails and mesh lines.</summary>
        public static void Fence(Transform parent, Vector3 from, Vector3 to, float height, Palette p)
        {
            Transform f = BuildKit.Empty("Fence", parent, from).transform;

            Vector3 delta = to - from;
            float length = new Vector2(delta.x, delta.z).magnitude;
            if (length < 0.01f) return;

            f.localRotation = Quaternion.LookRotation(new Vector3(delta.x, 0f, delta.z).normalized, Vector3.up);

            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 3f));
            for (int i = 0; i <= posts; i++)
                BuildKit.Box("Post", f, new Vector3(0f, height * 0.5f, length * i / posts),
                             new Vector3(0.12f, height, 0.12f), p.metal, false);

            // Rails carry the collision; the mesh lines are decoration only.
            for (int i = 0; i < 3; i++)
                BuildKit.Box("Rail", f, new Vector3(0f, height * (0.15f + 0.4f * i), length * 0.5f),
                             new Vector3(0.06f, 0.06f, length), p.metal, i == 1);

            int lines = Mathf.Max(2, Mathf.RoundToInt(length / 0.9f));
            for (int i = 0; i < lines; i++)
                BuildKit.Box("Mesh", f, new Vector3(0f, height * 0.5f, length * (i + 0.5f) / lines),
                             new Vector3(0.03f, height, 0.03f), p.metal, false);
        }

        public static void BasketballHoop(Transform parent, Vector3 at, float yaw, Palette p)
        {
            Transform h = BuildKit.Empty("Hoop", parent, at).transform;
            h.localRotation = Quaternion.Euler(0f, yaw, 0f);

            BuildKit.Box("Post", h, new Vector3(0f, 1.6f, 0f), new Vector3(0.18f, 3.2f, 0.18f), p.metal);
            BuildKit.Box("Arm", h, new Vector3(0f, 3.15f, 0.4f), new Vector3(0.12f, 0.12f, 0.8f), p.metal, false);
            BuildKit.Box("Board", h, new Vector3(0f, 3.2f, 0.82f), new Vector3(1.7f, 1.05f, 0.06f), p.linen, false);
            BuildKit.Box("Ring", h, new Vector3(0f, 2.9f, 0.6f), new Vector3(0.45f, 0.04f, 0.45f), p.paint, false);
        }
    }
}
