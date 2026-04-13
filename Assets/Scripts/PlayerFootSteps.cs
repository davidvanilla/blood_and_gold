using UnityEngine;

public class PlayerFootSteps : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource footstepSource; 
    public AudioClip[] footstepClips;

    [Header("Timing")]
    public float walkStepInterval = 0.8f;
    public float sprintStepInterval = 0.35f;

    private Player player;                
    private float stepTimer;
    private bool wasMovingLastFrame;



    void Start()
    {
        // Find the Player component on the same GameObject
        player = GetComponent<Player>();

        // If no separate AudioSource was assigned, try to add one
        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 1.0f;  // 3D sound
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (player != null && player.GetIsDead())
        //{
        //    stepTimer = 0f;
        //    return;
        //}

        //bool isMoving = false;
        //bool isSprinting = false;

        //Animator anim = GetComponent<Animator>();
        //if (anim != null)
        //{
        //    float speedFloat = anim.GetFloat("Walk");
        //    bool sprintBool = anim.GetBool("Sprint");
        //    isMoving = speedFloat > 0.1f;
        //    isSprinting = sprintBool;
        //}

        //if (!isMoving)
        //{
        //    stepTimer = 0f;
        //    wasMovingLastFrame = false;
        //    return;
        //}

        //float interval = isSprinting ? sprintStepInterval : walkStepInterval;
        //stepTimer -= Time.deltaTime;
        //if (stepTimer <= 0f)
        //{
        //    PlayFootstep();
        //    stepTimer = interval;
        //}

        //wasMovingLastFrame = true;

    }
    void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.PlayOneShot(clip);
    }

    public void OnPlayFootStep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.PlayOneShot(clip);
    }
}
