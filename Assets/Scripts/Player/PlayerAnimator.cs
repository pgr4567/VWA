using UnityEngine;

namespace Player {
    public class PlayerAnimator : MonoBehaviour {
        [SerializeField] private Transform walkingBall;
        [SerializeField] private float animationSpeed = 5;
        private Vector3 _lastPos = Vector3.zero;

        private void Update () {
            Vector3 position = transform.localPosition;
            if ((position - _lastPos).magnitude > 0.1f) {
                Animate (position - _lastPos);
                _lastPos = position;
            }
        }

        private void Animate (Vector3 direction) {
            walkingBall.Rotate (Vector3.Cross (Vector3.up, direction).normalized * animationSpeed * Time.deltaTime * 100, Space.World);
        }
    }
}