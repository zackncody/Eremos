using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Afterlife Set")]
public class AfterlifeDialogueSet : ScriptableObject
{
    public int minDeathCount;
    [TextArea] public string[] lines;
}