using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Small helpers for assembling levels out of primitives. Works both at runtime
    /// and from editor tooling (see Assets/Editor/LockUpMenu.cs).
    /// </summary>
    public static class BuildKit
    {
        // --------------------------------------------------------------- shapes

        /// <summary>Axis aligned box. <paramref name="size"/> is the full world size.</summary>
        public static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size,
                                     Material material, bool collider = true)
        {
            GameObject go = Primitive(PrimitiveType.Cube, name, parent, material, collider);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            return go;
        }

        /// <summary>Cylinder standing on Y. <paramref name="height"/> is the full height.</summary>
        public static GameObject Cylinder(string name, Transform parent, Vector3 center,
                                          float diameter, float height, Material material,
                                          bool collider = true)
        {
            GameObject go = Primitive(PrimitiveType.Cylinder, name, parent, material, collider);
            go.transform.localPosition = center;
            go.transform.localScale = new Vector3(diameter, height * 0.5f, diameter);
            return go;
        }

        public static GameObject Sphere(string name, Transform parent, Vector3 center,
                                        Vector3 size, Material material, bool collider = false)
        {
            GameObject go = Primitive(PrimitiveType.Sphere, name, parent, material, collider);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            return go;
        }

        public static GameObject Empty(string name, Transform parent, Vector3 localPosition = default)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go;
        }

        static GameObject Primitive(PrimitiveType type, string name, Transform parent,
                                    Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);

            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) Destroy(c);
            }

            if (material != null)
                go.GetComponent<MeshRenderer>().sharedMaterial = material;

            return go;
        }

        /// <summary>Destroy that is safe to call from edit mode.</summary>
        public static void Destroy(Object o)
        {
            if (o == null) return;

            if (Application.isPlaying) Object.Destroy(o);
            else Object.DestroyImmediate(o);
        }

        /// <summary>Flag scenery as static. Lights and anything that animates are
        /// skipped, along with their children — a moving object must not be static.</summary>
        public static void MarkStaticRecursive(GameObject root)
        {
            if (root.GetComponent<Light>() != null) return;
            if (root.GetComponent<TowerSpotlight>() != null) return;

            root.isStatic = true;
            foreach (Transform child in root.transform)
                MarkStaticRecursive(child.gameObject);
        }

        // ------------------------------------------------------------ wall runs

        /// <summary>A hole in a wall run, measured along the run from its start.</summary>
        public struct Opening
        {
            public float center, width, bottom, top;

            public Opening(float center, float width, float bottom, float top)
            {
                this.center = center;
                this.width = width;
                this.bottom = bottom;
                this.top = top;
            }

            public static Opening Door(float center, float width, float height)
            {
                return new Opening(center, width, 0f, height);
            }

            public static Opening Window(float center, float width, float sill, float head)
            {
                return new Opening(center, width, sill, head);
            }
        }

        /// <summary>
        /// A straight wall from A to B (parent local space) with any number of door
        /// or window openings punched through it. Built as solid segments plus
        /// lintels, so it still collides properly.
        /// </summary>
        public static GameObject WallRun(string name, Transform parent, Vector3 from, Vector3 to,
                                         float height, float thickness, Material material,
                                         params Opening[] openings)
        {
            GameObject root = Empty(name, parent, from);

            Vector3 delta = to - from;
            Vector3 flat = new Vector3(delta.x, 0f, delta.z);
            float length = flat.magnitude;
            if (length < 0.001f) return root;

            root.transform.localRotation = Quaternion.LookRotation(flat / length, Vector3.up);

            Opening[] sorted = openings != null ? (Opening[])openings.Clone() : new Opening[0];
            System.Array.Sort(sorted, (a, b) => a.center.CompareTo(b.center));

            float cursor = 0f;
            for (int i = 0; i < sorted.Length; i++)
            {
                Opening o = sorted[i];
                float start = o.center - o.width * 0.5f;
                float end = o.center + o.width * 0.5f;

                if (start > cursor) Segment(root.transform, cursor, start, 0f, height, thickness, material);
                if (o.bottom > 0f) Segment(root.transform, start, end, 0f, o.bottom, thickness, material);
                if (o.top < height) Segment(root.transform, start, end, o.top, height, thickness, material);

                cursor = Mathf.Max(cursor, end);
            }

            if (cursor < length) Segment(root.transform, cursor, length, 0f, height, thickness, material);
            return root;
        }

        static void Segment(Transform parent, float a, float b, float low, float high,
                            float thickness, Material material)
        {
            if (b - a < 0.002f || high - low < 0.002f) return;

            Box("Seg", parent, new Vector3(0f, (low + high) * 0.5f, (a + b) * 0.5f),
                new Vector3(thickness, high - low, b - a), material);
        }

        /// <summary>Ceiling lamp: housing plus a shadowless point light. No shadows is
        /// deliberate — the light bleeds through into adjoining rooms and keeps
        /// interiors readable without a light per cell.</summary>
        public static GameObject Lamp(string name, Transform parent, Vector3 position, Material housing,
                                      float range = 14f, float intensity = 4f)
        {
            GameObject root = Empty(name, parent, position);

            Box("Housing", root.transform, new Vector3(0f, 0.05f, 0f),
                new Vector3(0.6f, 0.12f, 0.25f), housing, false);

            GameObject lightGo = Empty("Light", root.transform, new Vector3(0f, -0.12f, 0f));
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.95f, 0.86f);
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;

            return root;
        }

        // ------------------------------------------------------------ materials

        /// <summary>
        /// Fetch (or create) a flat URP material. In the editor the material is saved
        /// under Assets/Materials so it survives a scene save; at runtime it is just
        /// an instance.
        /// </summary>
        public static Material Lit(string name, Color color, float smoothness = 0.08f, float metallic = 0f)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                const string folder = "Assets/Materials";
                if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Materials");

                string path = folder + "/" + name + ".mat";
                Material existing = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null)
                {
                    Configure(existing, color, smoothness, metallic);
                    UnityEditor.EditorUtility.SetDirty(existing);
                    return existing;
                }

                Material created = NewLit(name, color, smoothness, metallic);
                UnityEditor.AssetDatabase.CreateAsset(created, path);
                return created;
            }
#endif
            return NewLit(name, color, smoothness, metallic);
        }

        static Material NewLit(string name, Color color, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material m = new Material(shader) { name = name };
            Configure(m, color, smoothness, metallic);
            return m;
        }

        static void Configure(Material m, Color color, float smoothness, float metallic)
        {
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        }
    }
}
