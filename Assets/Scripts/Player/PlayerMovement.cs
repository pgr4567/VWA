using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    [SerializeField] private CharacterController controller;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform groundCheckSphere;
    [SerializeField] private float maxGroundDistance = 0.1f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float jumpHeight = 2f;

    private Vector2 input = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded = true;

    private void Update () {
        if (!isLocalPlayer || GameManager.instance.isInLobby) {
            return;
        }

        isGrounded = Physics.CheckSphere (groundCheckSphere.position, maxGroundDistance, groundLayerMask);
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }

        //TODO: Neues Input-System? Mobile-Client funktioniert so nicht.
        input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move.Normalize ();
        move *= movementSpeed;

        //TODO: Neues Input-System? Mobile-Client funktioniert so nicht.
        if (isGrounded && Input.GetButtonDown ("Jump")) {
            velocity.y += Mathf.Sqrt (-2f * gravity * jumpHeight);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move ((move + velocity) * Time.deltaTime);
    }
}