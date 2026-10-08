using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayerCode : MonoBehaviour
{
    private Vector2 lookDir;
    public float speed = 10f;
    private Rigidbody2D rb;
    private Vector2 input;
    [SerializeField] private Animator animator;
    private bool isKnockedBack = false;
    private Camera mainCam;

    private void Awake()
    {
        if (GameServices.Player != null && GameServices.Player != this)
        {
            Destroy(gameObject);
            return;
        }

        GameServices.RegisterPlayer(this);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCam = Camera.main;
    }

    private void OnDestroy()
    {
        if (GameServices.Player == this)
            GameServices.ClearPlayer();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();

        if (input != Vector2.zero)
            animator.SetBool("IsRunning", true);
        else
            animator.SetBool("IsRunning", false);

        if (mainCam == null) return;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        lookDir = ((Vector2)(mouseWorld - transform.position)).normalized;
        animator.SetFloat("LookX", lookDir.x);
        animator.SetFloat("LookY", lookDir.y);
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) return; // Don't override knockback velocity

        float speedMult = GameServices.Rage != null ? GameServices.Rage.SpeedMultiplier : 1f;
        rb.linearVelocity = input * speed * speedMult;
    }

    public IEnumerator ApplyKnockback(Vector2 force, float duration = 0.2f)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MiniGrass"))
        {
            animator.SetBool("IsInGrass", true);
            Debug.Log("Entered grass");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("MiniGrass"))
        {
            animator.SetBool("IsInGrass", false);
            Debug.Log("Left grass");
        }
    }
}