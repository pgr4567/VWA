using UnityEngine;

namespace Minigames.Labyrinth.Maze {
    public class MazeDoor : MazePassage {
        private static Quaternion
            normalRotation = Quaternion.Euler(0f, -90f, 0f),
            mirroredRotation = Quaternion.Euler(0f, 90f, 0f);

        public Transform hinge;

        private bool isMirrored;

        private MazeDoor OtherSideOfDoor {
            get {
                return otherCell.GetEdge(direction.GetOpposite()) as MazeDoor;
            }
        }
        
        public override void Initialize (MazeCell primary, MazeCell other, MazeDirection direction) {
            base.Initialize(primary, other, direction);
            if (OtherSideOfDoor != null) {
                isMirrored = true;
                hinge.localScale = new Vector3(-1f, 1f, 1f);
                Vector3 p = hinge.localPosition;
                p.x = -p.x;
                hinge.localPosition = p;
            }
        }
        private bool isOpen = false;
        private void OnTriggerEnter(Collider other) {
            Open();
        }
        public void Interact() {
            if (isOpen) {
                Close();
            }
            else {
                Open();
            }
        }
        //TODO: HACKIEST SHIT I HAVE EVER SEEN
        private void Open() {
            isOpen = true;
            //OtherSideOfDoor.hinge.localRotation = hinge.localRotation =
            //    isMirrored ? mirroredRotation : normalRotation;
            if (OtherSideOfDoor.hinge) {
                Destroy(OtherSideOfDoor.hinge.gameObject);
            }
        }
        private void Close() {
            isOpen = false;
            OtherSideOfDoor.hinge.localRotation = hinge.localRotation = Quaternion.identity;
        }
    }
}