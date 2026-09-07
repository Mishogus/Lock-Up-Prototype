using UnityEngine;

namespace LockUp
{
    /// <summary>Every material the level uses, created once per build and shared
    /// by all the pieces so the whole prison stays on one small colour scheme.</summary>
    public class Palette
    {
        public Material grass, dirt, gravel;
        public Material concrete, concreteDark, floor, tile;
        public Material wood, metal, bars, steel;
        public Material mattress, linen, paint;
        public Material bark;
        public Material[] foliage;

        public Material prisoner, guardUniform, guardTrim, headGuard, supervisor, skin;

        public static Palette Create()
        {
            return new Palette
            {
                grass = BuildKit.Lit("Grass", new Color(0.24f, 0.34f, 0.18f)),
                dirt = BuildKit.Lit("YardDirt", new Color(0.42f, 0.38f, 0.32f)),
                gravel = BuildKit.Lit("Gravel", new Color(0.50f, 0.49f, 0.46f)),

                concrete = BuildKit.Lit("Concrete", new Color(0.62f, 0.61f, 0.58f)),
                concreteDark = BuildKit.Lit("ConcreteDark", new Color(0.42f, 0.42f, 0.40f)),
                floor = BuildKit.Lit("FloorConcrete", new Color(0.52f, 0.51f, 0.49f)),
                tile = BuildKit.Lit("Tile", new Color(0.74f, 0.77f, 0.74f), 0.45f),

                wood = BuildKit.Lit("Wood", new Color(0.36f, 0.26f, 0.17f)),
                metal = BuildKit.Lit("Metal", new Color(0.30f, 0.31f, 0.33f), 0.55f, 0.85f),
                bars = BuildKit.Lit("Bars", new Color(0.22f, 0.23f, 0.25f), 0.5f, 0.9f),
                steel = BuildKit.Lit("Steel", new Color(0.66f, 0.68f, 0.70f), 0.62f, 0.9f),

                mattress = BuildKit.Lit("Mattress", new Color(0.36f, 0.40f, 0.46f)),
                linen = BuildKit.Lit("Linen", new Color(0.80f, 0.78f, 0.72f)),
                paint = BuildKit.Lit("PaintYellow", new Color(0.78f, 0.65f, 0.18f)),

                prisoner = BuildKit.Lit("Prisoner", new Color(0.82f, 0.42f, 0.12f)),
                guardUniform = BuildKit.Lit("GuardUniform", new Color(0.16f, 0.20f, 0.31f)),
                guardTrim = BuildKit.Lit("GuardTrim", new Color(0.10f, 0.11f, 0.14f)),
                headGuard = BuildKit.Lit("HeadGuard", new Color(0.10f, 0.14f, 0.24f)),
                supervisor = BuildKit.Lit("Supervisor", new Color(0.34f, 0.33f, 0.36f)),
                skin = BuildKit.Lit("Skin", new Color(0.76f, 0.62f, 0.50f)),

                bark = BuildKit.Lit("Bark", new Color(0.27f, 0.20f, 0.14f)),
                foliage = new[]
                {
                    BuildKit.Lit("Foliage_A", new Color(0.16f, 0.31f, 0.15f)),
                    BuildKit.Lit("Foliage_B", new Color(0.21f, 0.36f, 0.17f)),
                    BuildKit.Lit("Foliage_C", new Color(0.13f, 0.26f, 0.14f))
                }
            };
        }
    }
}
