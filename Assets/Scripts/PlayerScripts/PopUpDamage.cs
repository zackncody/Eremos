using UnityEngine;

public class PopUpDamage : MonoBehaviour
{
    public Vector2 InitialVelocity;
    public Rigidbody2D rb;
    public float lifetime = 1.5f;
    public bool hitFromRight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (hitFromRight)
            InitialVelocity.x *= -1;

        rb.linearVelocity = InitialVelocity;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
