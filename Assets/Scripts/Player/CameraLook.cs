using General;
using Mirror;
using UnityEngine;

namespace Player {
    public class CameraLook : NetworkBehaviour {
        [SerializeField] private float maxXRotation = 80f;
        public float lookSpeed = 5f;
        [SerializeField] private GameObject cameraPrefab;

        private Transform _camera;
        private Vector3 _rotation = Vector3.zero;

        public static float standardLookSpeed = 5f;

        private void Start () {
            if (isLocalPlayer) {
                _camera = Instantiate (cameraPrefab, new Vector3 (0, 1f, 0), Quaternion.identity, transform)
                    .transform;
                gameObject.name = "LocalPlayer";
            } else {
                gameObject.name = "RemotePlayer";
                Destroy (GetComponent<CharacterController>());
                Destroy (GetComponent<CameraLook>());
                Destroy (GetComponent<MultiplayerFunctionReceiver>());
                Destroy (GetComponent<PlayerInteract>());
            }
        }

        private void Update () {
            if (!isLocalPlayer || GameManager.instance.isInGUI) {
                return;
            }

            //TODO: Neues Input-System? Mobile-Client funktioniert so nicht.
            _rotation.x -= Input.GetAxis ("Mouse Y") * lookSpeed;
            _rotation.y += Input.GetAxis ("Mouse X") * lookSpeed;
            _rotation.x =  Mathf.Clamp (_rotation.x, -maxXRotation, maxXRotation);

            transform.eulerAngles           = new Vector2 (0, _rotation.y);
            _camera.transform.localRotation = Quaternion.Euler (_rotation.x, 0, 0);
        }
    }
}