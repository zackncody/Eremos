using UnityEngine;
public class TongueHitbox : MonoBehaviour
{
    public RangedMeleeAttack owner;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[TongueHitbox] Trigger hit: {other.gameObject.name}, tag: {other.tag}");
        owner.OnTongueHit(other);
    }
}