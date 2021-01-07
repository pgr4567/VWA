using Mirror;
using UnityEngine;

namespace Player {
    public class PlayerAnimator : MonoBehaviour {
        [SerializeField] private Transform walkingBall;
        [SerializeField] private float animationSpeed = 5;
        private Vector3 _lastPos = Vector3.zero;

        private void Update () {
            Vector3 position = transform.position;
            Animate (position - _lastPos);
            _lastPos = position;
        }

        private void Animate (Vector3 direction) {
            Vector3 angles = walkingBall.localEulerAngles;
            angles += new Vector3 (direction.z, 0, direction.x) * animationSpeed * Time.deltaTime * 1000;
            if (angles.x > 36000f) {
                angles.x -= 36000f;
            }

            if (angles.y > 36000f) {
                angles.y -= 36000f;
            }

            if (angles.z > 36000f) {
                angles.z -= 36000f;
            }

            walkingBall.transform.localEulerAngles = angles;
        }
    }
}