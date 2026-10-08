using UnityEngine;

public class AfterlifeDialogueDatabase : MonoBehaviour
{
    public AfterlifeDialogueSet[] sets;

    public AfterlifeDialogueSet GetSetForDeathCount(int deaths)
    {
        AfterlifeDialogueSet best = sets[0];
        foreach (var s in sets)
            if (deaths >= s.minDeathCount) best = s;
        return best;
    }
}