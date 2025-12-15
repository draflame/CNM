using UnityEngine;

/// <summary>
/// Script cho Checkpoint - điểm hồi sinh
/// Khi player tương tác: Save game + hồi máu/stamina + set làm respawn point
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private string checkpointID = ""; // ID duy nhất của checkpoint
    [SerializeField] private bool isActivated = false; // Đã kích hoạt chưa
    
    [Header("World-Space Interaction UI")]
    [SerializeField] private GameObject worldUI; // UI hiển thị "Press E"
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject activatedEffect; // Effect khi đã activate (optional)
    [SerializeField] private ParticleSystem activateParticle; // Particle khi activate
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activateClip;
    
    private Animator animator;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-generate ID nếu chưa có
        if (string.IsNullOrEmpty(checkpointID))
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            checkpointID = $"{sceneName}_Checkpoint_{transform.position.x:F1}_{transform.position.y:F1}";
            Debug.Log($"🆔 Auto-generated checkpoint ID: {checkpointID}");
        }
        
        if (worldUI != null)
            worldUI.SetActive(false);
        
        // Check xem checkpoint này đã được activate chưa
        if (CheckpointManager.Instance != null)
        {
            isActivated = CheckpointManager.Instance.IsCheckpointActivated(checkpointID);
            UpdateVisuals();
        }
    }
    
    void Update()
    {
        // Chỉ cho phép interact nếu chưa activate hoặc muốn save lại
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateCheckpoint();
        }
        
        // UI luôn hướng về Camera (2D)
        if (worldUI != null && worldUI.activeSelf)
        {
            worldUI.transform.rotation = Quaternion.identity;
        }
    }
    
    private void ActivateCheckpoint()
    {
        if (CheckpointManager.Instance == null)
        {
            Debug.LogError("❌ CheckpointManager not found!");
            return;
        }
        
        // Kích hoạt checkpoint
        CheckpointManager.Instance.ActivateCheckpoint(checkpointID, transform.position);
        isActivated = true;
        
        // Visual feedback
        UpdateVisuals();
        
        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("Activate");
        }
        
        // Play particle effect
        if (activateParticle != null)
        {
            activateParticle.Play();
        }
        
        // Play sound
        if (audioSource != null && activateClip != null)
        {
            audioSource.PlayOneShot(activateClip);
        }
        
        // Ẩn UI
        if (worldUI != null)
        {
            worldUI.SetActive(false);
        }
        
        Debug.Log($"✅ Checkpoint activated: {checkpointID}");
    }
    
    private void UpdateVisuals()
    {
        // Thay đổi visual khi đã activate (optional)
        if (activatedEffect != null)
        {
            activatedEffect.SetActive(isActivated);
        }
        
        // Thay đổi màu sprite (optional)
        if (spriteRenderer != null && isActivated)
        {
            spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f); // Màu xanh nhạt
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Hiển thị UI (luôn hiển thị, cho phép save lại)
            if (worldUI != null)
            {
                worldUI.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (worldUI != null)
            {
                worldUI.SetActive(false);
            }
        }
    }
    
    // Public getter
    public string GetCheckpointID()
    {
        return checkpointID;
    }
    
    public bool IsActivated()
    {
        return isActivated;
    }
}
