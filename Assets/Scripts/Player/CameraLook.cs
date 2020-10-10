﻿using Mirror;
using UnityEngine;

public class CameraLook : NetworkBehaviour {
    [SerializeField] private float maxXRotation = 80f;
    [SerializeField] private float lookSpeed = 10f;
    [SerializeField] private new Transform camera;

    private Vector3 rotation = Vector3.zero;
    private void Update () {
        if (!isLocalPlayer) {
            return;
        }
        //TODO: Neues Input-System? Mobile-Client funktioniert so nicht.
        rotation.x -= Input.GetAxis ("Mouse Y") * lookSpeed;
        rotation.y += Input.GetAxis ("Mouse X") * lookSpeed;
        rotation.x = Mathf.Clamp (rotation.x, -maxXRotation, maxXRotation);

        transform.eulerAngles = new Vector2 (0, rotation.y);
        camera.transform.localRotation = Quaternion.Euler (rotation.x, 0, 0);
    }
}