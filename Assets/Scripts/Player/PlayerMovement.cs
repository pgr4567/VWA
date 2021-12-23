using System;
using General;
using Mirror;
using UnityEngine;

namespace Player {
    public class PlayerMovement : NetworkBehaviour {
        [SerializeField] private CharacterController controller;
        public float movementSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheckSphere;
        [SerializeField] private float maxGroundDistance = 0.1f;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float jumpHeight = 2f;

        private Vector2 _input = Vector2.zero;
        private bool _isGrounded = true;
        private Vector3 _velocity = Vector3.zero;

        public static float standardMovementSpeed = 10f;

        private void Update () {
            if (!isLocalPlayer || GameManager.instance.isInGUI) {
                return;
            }

            _isGrounded = Physics.CheckSphere (groundCheckSphere.position, maxGroundDistance, groundLayerMask);
            if (_isGrounded && _velocity.y < 0) {
                _velocity.y = -2f;
            }

            //TODO: Neues Input-System? Mobile-Client funktioniert so nicht.
            _input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

            Vector3 move = transform.right * _input.x + transform.forward * _input.y;
            move.Normalize ();
            move *= movementSpeed;

            //TODO: Neues Input-System? Mobile-Client funktioniert so nicht.
            if (_isGrounded && Input.GetButtonDown ("Jump")) {
                _velocity.y += Mathf.Sqrt (-2f * gravity * jumpHeight);
            }

            _velocity.y += gravity * Time.deltaTime;
            controller.Move ((move + _velocity) * Time.deltaTime);
        }

        [ClientRpc (excludeOwner = true)]
        public void RpcSetVisible (bool visible) {
            // Is local player inGame? If so, dont disable player.
            //TODO: Could lead to cases where player was playing, then left and now sees visible players that should be invisible.
            if (GameManager.instance.isInGame) {
                return;
            }

            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer> ()) {
                mr.enabled = visible;
            }
        }
    }
}