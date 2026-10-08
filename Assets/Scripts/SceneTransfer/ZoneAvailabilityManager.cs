using UnityEngine;

public class ZoneAvailabilityManager : MonoBehaviour
{
    public enum ActiveTime { Day, Night, Both }

    [System.Serializable]
    public class ManagedZone
    {
        public GameObject zoneObject;
        public ActiveTime activeDuring;
    }

    public DayNightCycle dayNightCycle;
    public ManagedZone[] zones;

    void Update()
    {
        if (dayNightCycle == null) return;

        bool isDay = dayNightCycle.IsDay;

        foreach (var zone in zones)
        {
            if (zone.zoneObject == null) continue;

            bool shouldBeActive = zone.activeDuring == ActiveTime.Both
                || (zone.activeDuring == ActiveTime.Day && isDay)
                || (zone.activeDuring == ActiveTime.Night && !isDay);

            if (zone.zoneObject.activeSelf != shouldBeActive)
                zone.zoneObject.SetActive(shouldBeActive);
        }
    }
}