using UnityEngine;

[CreateAssetMenu (fileName = "New Minigame", menuName = "Custom/New Minigame")]
public class Minigame : ScriptableObject {
    public new string name;
    public GameObject world;
    public int maxInstances;
}