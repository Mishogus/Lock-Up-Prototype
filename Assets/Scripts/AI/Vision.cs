using UnityEngine;

namespace LockUp
{
    /// <summary>Shared line-of-sight test for guards, cameras and searchlights.</summary>
    public static class Vision
    {
        /// <summary>
        /// True when <paramref name="target"/> is inside the cone and nothing solid
        /// is in the way. <paramref name="clarity"/> comes back as 0..1, strongest
        /// when the target is close and dead centre.
        /// </summary>
        public static bool CanSee(Vector3 eye, Vector3 forward, Transform target,
                                  float range, float halfAngle, out float clarity)
        {
            clarity = 0f;
            if (target == null) return false;

            // Aim at the chest rather than the origin at the feet.
            Vector3 chest = target.position + Vector3.up * 1.1f;
            Vector3 delta = chest - eye;
            float distance = delta.magnitude;
            if (distance > range || distance < 0.01f) return false;

            Vector3 direction = delta / distance;
            float angle = Vector3.Angle(forward, direction);
            if (angle > halfAngle) return false;

            if (Physics.Raycast(eye, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                // Anything that is not the player blocks the view.
                if (hit.transform != target && !hit.transform.IsChildOf(target)) return false;
            }

            float byDistance = 1f - Mathf.Clamp01(distance / range);
            float byAngle = 1f - Mathf.Clamp01(angle / halfAngle);
            clarity = Mathf.Clamp01(0.35f + 0.65f * byDistance * byAngle);
            return true;
        }
    }
}
