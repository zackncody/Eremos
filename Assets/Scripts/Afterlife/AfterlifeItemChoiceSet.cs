using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Afterlife Item Choice Set")]
public class AfterlifeItemChoiceSet : ScriptableObject
{
    public int deathCountTrigger; // exact death count this fires on, e.g. 3, 10, 25
    public ItemData[] choices;
}