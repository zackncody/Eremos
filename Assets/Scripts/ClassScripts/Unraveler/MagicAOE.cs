using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicAOE : MonoBehaviour
{
    [SerializeField] private float expandDuration = 0.25f;
    [SerializeField] private float lingerAfterExpand = 0.15f;

    public void Init(int damage, float radius)
    {
        StartCoroutine(ExpandAndDamage(damage, radius));
    }

    private IEnumerator ExpandAndDamage(int damage, float radius)
    {
        // Whatever scale you set on the prefab in the Inspector is treated as the
        // "fully expanded" size. It grows from 0 up to that over expandDuration.
        Vector3 fullScale = transform.localScale;
        transform.localScale = Vector3.zero;

        HashSet<GameObject> alreadyHit = new HashSet<GameObject>();
        float t = 0f;

        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / expandDuration);

            transform.localScale = fullScale * pct;
            DamageWithin(radius * pct, damage, alreadyHit);

            yield return null;
        }

        transform.localScale = fullScale;

        // Final pass in case anything was missed between frame steps.
        DamageWithin(radius, damage, alreadyHit);

        Destroy(gameObject, lingerAfterExpand);
    }

    private void DamageWithin(float r, int damage, HashSet<GameObject> alreadyHit)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, r);
        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root.CompareTag("Player")) continue;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null) enemy = hit.GetComponentInChildren<EnemyHealth>();

            BossController boss = null;
            if (enemy == null)
            {
                boss = hit.GetComponentInParent<BossController>();
                if (boss == null) boss = hit.GetComponentInChildren<BossController>();
            }

            // One hit per enemy, however many colliders it has.
            GameObject key = enemy != null ? enemy.gameObject
                           : boss != null ? boss.gameObject
                           : hit.gameObject;
            if (!alreadyHit.Add(key)) continue;

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                GameServices.ReportPlayerDamage(enemy, damage);
            }
            else if (boss != null)
            {
                boss.TakeDamage(damage);
                GameServices.ReportPlayerDamage(boss, damage);
            }
            else
            {
                // Anything else with a TakeDamage method (breakables, etc.)
                hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}