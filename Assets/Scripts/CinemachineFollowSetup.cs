using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Re-assigns this Virtual Camera's Follow/LookAt target at runtime,
/// since the player persists via DontDestroyOnLoad and can't be
/// dragged into the Inspector for scenes it doesn't originally exist in.
/// Attach this to the CinemachineCamera object in each scene.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineFollowSetup : MonoBehaviour
{
    private CinemachineCamera vcam;

    private void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        AssignTarget();
    }

    private void AssignTarget()
    {
        if (GameServices.Player == null)
        {
            Debug.LogWarning("CinemachineFollowSetup: GameServices.Player is null, cannot assign follow target.");
            return;
        }

        vcam.Follow = GameServices.Player.transform;
        vcam.LookAt = GameServices.Player.transform;
    }
}