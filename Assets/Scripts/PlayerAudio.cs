using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Footstep Audio")]
    [Tooltip("Add 2-3 footstep clips for random variation")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)]
    public float footstepVolume = 0.6f;
    [Tooltip("Time between each footstep sound while moving")]
    public float footstepInterval = 0.4f;

    [Header("Idle Audio")]
    [Tooltip("A short loopable hum or whistle clip")]
    public AudioClip idleClip;
    [Range(0f, 1f)]
    public float idleVolume = 0.4f;
    [Tooltip("Seconds of stillness before idle sound plays")]
    public float idleDelay = 2f;

    private AudioSource footstepSource;
    private AudioSource idleSource;

    private float footstepTimer = 0f;
    private float idleTimer = 0f;
    private bool isMoving = false;
    private bool idlePlaying = false;
    private int lastFootstepIndex = -1;

    void Start()
    {
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 0f;

        idleSource = gameObject.AddComponent<AudioSource>();
        idleSource.playOnAwake = false;
        idleSource.loop = true;
        idleSource.spatialBlend = 0f;
        idleSource.volume = idleVolume;

        if (idleClip != null)
            idleSource.clip = idleClip;
    }

    void Update()
    {
        // GetAxisRaw snaps to 0 immediately on key release, unlike GetAxis which ramps down
        // This prevents footsteps firing after the player has stopped
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        HandleFootsteps();
        HandleIdleAudio();
    }

    private void HandleFootsteps()
    {
        if (!isMoving)
        {
            footstepTimer = 0f;
            // Stop immediately - don't let the current clip finish
            if (footstepSource.isPlaying)
                footstepSource.Stop();
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            PlayRandomFootstep();
            footstepTimer = footstepInterval;
        }
    }

    private void HandleIdleAudio()
    {
        if (isMoving)
        {
            idleTimer = 0f;
            if (idlePlaying)
            {
                idleSource.Stop();
                idlePlaying = false;
            }
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDelay && !idlePlaying && idleClip != null)
            {
                idleSource.Play();
                idlePlaying = true;
            }
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index = lastFootstepIndex;
        if (footstepClips.Length > 1)
        {
            while (index == lastFootstepIndex)
                index = Random.Range(0, footstepClips.Length);
        }
        else
        {
            index = 0;
        }

        lastFootstepIndex = index;
        if (footstepClips[index] != null)
        {
            // Assign clip and Play instead of PlayOneShot so Stop() works instantly
            footstepSource.clip = footstepClips[index];
            footstepSource.volume = footstepVolume;
            footstepSource.Play();
        }
    }

    public void StopAll()
    {
        footstepSource.Stop();
        idleSource.Stop();
        idlePlaying = false;
        idleTimer = 0f;
    }
}
