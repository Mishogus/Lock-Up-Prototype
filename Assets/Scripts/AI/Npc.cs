using UnityEngine;

namespace LockUp
{
    /// <summary>
    /// Shared legwork for everyone who walks around the prison. Movement is
    /// straight-line towards a target: the routes are laid along the corridor and
    /// the open yard, so nobody needs to path around a corner.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Npc : MonoBehaviour
    {
        public float walkSpeed = 2.1f;
        public float runSpeed = 4.6f;
        public float turnSpeed = 420f;
        public float arriveDistance = 0.7f;

        protected CharacterController cc;

        protected virtual void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        /// <summary>Walk towards a point. Returns true once it has been reached.</summary>
        protected bool MoveTowards(Vector3 target, float speed)
        {
            Vector3 flat = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z);
            float distance = flat.magnitude;

            if (distance <= arriveDistance)
            {
                cc.SimpleMove(Vector3.zero);
                return true;
            }

            Vector3 direction = flat / distance;
            FaceDirection(direction);
            cc.SimpleMove(direction * speed);
            return false;
        }

        protected void Idle()
        {
            cc.SimpleMove(Vector3.zero);
        }

        protected void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion want = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z).normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
        }

        protected Vector3 EyePosition
        {
            // Nudged forward so the ray does not start inside the NPC's own capsule.
            get { return transform.position + Vector3.up * 1.62f + transform.forward * 0.42f; }
        }
    }
}
