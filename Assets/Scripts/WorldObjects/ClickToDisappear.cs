using UnityEngine;
using System.Collections.Generic;

public class ClickToDisappear : MonoBehaviour
{
    [Header("Condition")]
    public ConditionType condition = ConditionType.Always;
    public enum ConditionType { Always, TimeOfDay, BossKilled, EnemyKilled, TimeOfDayAndBossKilled, TimeOfDayAndEnemyKilled }

    [Header("Time of Day (if used)")]
    public float allowedStartTime = 18f;
    public float allowedEndTime = 6f;

    [Header("Boss (if used)")]
    // Uses BossController.IsDead static bool
    [Header("Enemy (if used)")]
    public EnemyHealth requiredEnemy;

    [Header("Save Key (for persistence)")]
    public string saveKey = "ClickToDisappear_Object1";

    private static readonly List<ClickToDisappear> allInstances = new List<ClickToDisappear>();

    private void OnEnable()
    {
        allInstances.Add(this);
    }

    private void OnDisable()
    {
        allInstances.Remove(this);
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
            gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!ConditionMet()) return;
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }

    public static void ResetAll()
    {
        foreach (ClickToDisappear obj in allInstances)
        {
            PlayerPrefs.DeleteKey(obj.saveKey);
            obj.gameObject.SetActive(true);
        }
        PlayerPrefs.Save();
        Debug.Log($"ClickToDisappear: {allInstances.Count} objects reset.");
    }

    private bool ConditionMet()
    {
        switch (condition)
        {
            case ConditionType.Always: return true;
            case ConditionType.TimeOfDay: return IsCorrectTime();
            case ConditionType.BossKilled: return BossController.IsDead;
            case ConditionType.EnemyKilled: return IsEnemyDead();
            case ConditionType.TimeOfDayAndBossKilled: return IsCorrectTime() && BossController.IsDead;
            case ConditionType.TimeOfDayAndEnemyKilled: return IsCorrectTime() && IsEnemyDead();
            default: return true;
        }
    }

    private bool IsEnemyDead()
    {
        if (requiredEnemy == null) return true;
        return !requiredEnemy.gameObject.activeInHierarchy;
    }

    private bool IsCorrectTime()
    {
        if (DayNightCycle.Instance == null) return false;
        float t = DayNightCycle.Instance.currentTime;
        if (allowedStartTime < allowedEndTime)
            return t >= allowedStartTime && t < allowedEndTime;
        else
            return t >= allowedStartTime || t < allowedEndTime;
    }
}