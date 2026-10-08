using UnityEngine;

public class ItemPopAnimation : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _landPos;
    private float _popHeight;
    private float _popDuration;
    private float _fallDuration;
    private float _timer;
    private bool _falling;

    public void Play(Vector3 startPos, Vector3 landPos, float popHeight, float popDuration, float fallDuration)
    {
        _startPos = startPos;
        _landPos = landPos;
        _popHeight = popHeight;
        _popDuration = popDuration;
        _fallDuration = fallDuration;
        _timer = 0f;
        _falling = false;
        transform.position = startPos;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (!_falling)
        {
            // Arc upward toward peak
            float t = Mathf.Clamp01(_timer / _popDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 2f); // ease out

            Vector3 peak = Vector3.Lerp(_startPos, _landPos, 0.5f) + Vector3.up * _popHeight;
            Vector3 posA = Vector3.Lerp(_startPos, peak, easedT);
            Vector3 posB = Vector3.Lerp(peak, _landPos, easedT);
            transform.position = Vector3.Lerp(posA, posB, easedT);

            if (t >= 1f)
            {
                _falling = true;
                _timer = 0f;
            }
        }
        else
        {
            // Fall from peak to land position
            float t = Mathf.Clamp01(_timer / _fallDuration);
            float easedT = t * t; // ease in (accelerate downward)

            Vector3 peak = Vector3.Lerp(_startPos, _landPos, 0.5f) + Vector3.up * _popHeight;
            transform.position = Vector3.Lerp(peak, _landPos, easedT);

            if (t >= 1f)
            {
                transform.position = _landPos;
                // Re-enable colliders/rigidbody now that animation is done
                foreach (var col in GetComponentsInChildren<Collider2D>())
                    col.enabled = true;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = true;

                Destroy(this); // Remove the animation component, item stays
            }
        }
    }
}