using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerJump : MonoBehaviour
{
    public float jumpHeight = 3.2f;
    public int maxJumps = 1;

    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Настройки звуков (Имена групп из библиотеки)")]
    [SerializeField] private string jumpSoundGroup = "Jump";
    [SerializeField] private string landSoundGroup = "Land";
    [SerializeField] private float pitchRandomness = 0.15f;

    // ПЕРЕНЕС СЮДА: Теперь они точно появятся прямо под Pitch Randomness!
    [Range(0f, 1f)]
    [SerializeField] private float landVolume = 0.6f;       // Громкость приземления (тише/громче)
    [SerializeField] private float landBasePitch = 1.2f;     // Базовый питч приземления (выше/ниже)

    Animator animator;
    CharacterController controller;
    TopDownPlayerMovement movement;

    int jumpsLeft;
    float coyoteCounter;
    float jumpBufferCounter;

    private bool previouslyGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<TopDownPlayerMovement>();
        jumpsLeft = maxJumps;
        animator = GetComponentInChildren<Animator>();

        previouslyGrounded = controller.isGrounded;
    }

    void Update()
    {
        if (controller.isGrounded && !previouslyGrounded)
        {
            PlayLandSFX();
        }
        previouslyGrounded = controller.isGrounded;

        if (controller.isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            PerformJump();
            jumpBufferCounter = 0f;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    void PerformJump()
    {
        if (controller.isGrounded)
            jumpsLeft = maxJumps;

        if (jumpsLeft > 0)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * movement.gravity);
            movement.SetVerticalVelocity(jumpVelocity);
            animator.SetTrigger("Jump");

            PlayRandomizedSFX(jumpSoundGroup, 1f);

            jumpsLeft--;
            coyoteCounter = 0f;
        }
    }

    private void PlayRandomizedSFX(string groupName, float basePitch)
    {
        if (!string.IsNullOrEmpty(groupName))
        {
            float randomPitch = basePitch * Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
            SoundEffectManager.PlayVoice(groupName, randomPitch, false);
        }
    }

    private void PlayLandSFX()
    {
        if (!string.IsNullOrEmpty(landSoundGroup))
        {
            float randomPitch = landBasePitch * Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
            SoundEffectManager.PlayVoice(landSoundGroup, randomPitch, false, landVolume);
        }
    }
}