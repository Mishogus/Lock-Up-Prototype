using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Minimal on-screen readout: the timetable, where you are, and how close the
    /// prison is to noticing you. Drawn with IMGUI so there is no canvas to wire up.
    /// </summary>
    public class PrisonHUD : MonoBehaviour
    {
        LevelMap map;
        GUIStyle label, small, banner;
        Texture2D white;
        float caughtFlashUntil;
        int lastCaughtCount;

        void Awake()
        {
            map = Object.FindAnyObjectByType<LevelMap>();
            white = Texture2D.whiteTexture;
        }

        void Update()
        {
            AlertState alert = AlertState.Instance;
            if (alert == null) return;

            if (alert.timesCaught != lastCaughtCount)
            {
                lastCaughtCount = alert.timesCaught;
                caughtFlashUntil = Time.time + 2.5f;
            }
        }

        void OnGUI()
        {
            EnsureStyles();

            AlertState alert = AlertState.Instance;
            if (alert == null) return;

            const float pad = 14f;
            float w = 260f;

            // Timetable.
            DaySchedule schedule = DaySchedule.Instance;
            if (schedule != null)
            {
                string phase = DaySchedule.Label(schedule.current);
                GUI.Label(new Rect(pad, pad, w, 24f), phase, label);
                GUI.Label(new Rect(pad, pad + 22f, w, 20f),
                          Mathf.CeilToInt(schedule.timeLeft) + "s until " + DaySchedule.Label(DaySchedule.Next(schedule.current)),
                          small);
            }

            // Where you are, and whether you should be.
            Transform player = alert.Player;
            if (player != null && map != null)
                GUI.Label(new Rect(pad, pad + 46f, w * 1.6f, 20f), PrisonRules.Explain(player.position, map), small);

            // Suspicion bar.
            float barY = pad + 74f;
            GUI.Label(new Rect(pad, barY, w, 20f), alert.alarm ? "ALARM" : "Suspicion", small);
            Bar(new Rect(pad, barY + 20f, w, 12f), alert.suspicion,
                alert.alarm ? new Color(0.9f, 0.2f, 0.2f) : Color.Lerp(new Color(0.5f, 0.75f, 0.4f), new Color(0.9f, 0.6f, 0.15f), alert.suspicion));

            if (alert.timesCaught > 0)
                GUI.Label(new Rect(pad, barY + 38f, w, 20f), "Caught " + alert.timesCaught + "x", small);

            // Controls hint.
            GUI.Label(new Rect(pad, Screen.height - 26f, 700f, 20f),
                      "WASD move   Shift run   Ctrl crouch   Space jump   Esc free cursor", small);

            if (Time.time < caughtFlashUntil)
            {
                var r = new Rect(0f, Screen.height * 0.4f, Screen.width, 60f);
                GUI.Label(r, "CAUGHT - back to your cell", banner);
            }
        }

        void Bar(Rect r, float fill, Color colour)
        {
            Color old = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(r, white);

            GUI.color = colour;
            GUI.DrawTexture(new Rect(r.x + 1f, r.y + 1f, (r.width - 2f) * Mathf.Clamp01(fill), r.height - 2f), white);

            GUI.color = old;
        }

        void EnsureStyles()
        {
            if (label != null) return;

            label = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            label.normal.textColor = Color.white;

            small = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            small.normal.textColor = new Color(0.88f, 0.88f, 0.9f);

            banner = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            banner.normal.textColor = new Color(1f, 0.4f, 0.35f);
        }
    }
}
