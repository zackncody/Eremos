using UnityEngine;

[CreateAssetMenu(fileName = "ZoneData", menuName = "RPG/Zone")]
public class ZoneData : ScriptableObject
{
    public string zoneName;
    public AudioClip music;
    [Range(0f, 1f)] public float ambientLight = 1f;
    public Color fogColor;
    public string[] enemyTypes;
}