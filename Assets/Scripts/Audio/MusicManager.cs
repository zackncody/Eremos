using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [Header("Music Tracks")]
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private AudioClip combatMusic;

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float explorationVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float combatVolume = 1f;

    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float combatCooldown = 5f; 

    private AudioSource audioSource;
    private bool isInCombat = false;
    private float combatCooldownTimer = 0f;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        PlayExploration();
    }

    private void Update()
    {
        if (isInCombat)
        {
            combatCooldownTimer -= Time.deltaTime;
            if (combatCooldownTimer <= 0f)
            {
                isInCombat = false;
                TransitionTo(explorationMusic, explorationVolume);
            }
        }
    }

 
    public void EnterCombat()
    {
        combatCooldownTimer = combatCooldown;

        if (!isInCombat)
        {
            isInCombat = true;
            TransitionTo(combatMusic, combatVolume);
        }
        else
        {
          
            combatCooldownTimer = combatCooldown;
        }
    }

    
    public void EnemyDied()
    {
        if (isInCombat)
            combatCooldownTimer = combatCooldown;
    }

    private void PlayExploration()
    {
        if (explorationMusic == null) return;
        audioSource.clip = explorationMusic;
        audioSource.volume = explorationVolume;
        audioSource.Play();
    }

    private void TransitionTo(AudioClip newClip, float targetVolume)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToNewTrack(newClip, targetVolume));
    }

    private IEnumerator FadeToNewTrack(AudioClip newClip, float targetVolume)
    {
       
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }

        
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += targetVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}