using UnityEngine;

public class AfterlifeItemChoiceDatabase : MonoBehaviour
{
    public AfterlifeItemChoiceSet[] sets;

    public AfterlifeItemChoiceSet GetSetForDeathCount(int deaths)
    {
        foreach (var s in sets)
            if (s.deathCountTrigger == deaths) return s;
        return null; // no choice on this death
    }
}