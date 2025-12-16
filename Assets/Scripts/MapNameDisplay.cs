using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hiển thị tên map với hiệu ứng fade in/out khi chuyển scene
/// Đặt vào Canvas trong scene Persistent hoặc trong mỗi map
/// </summary>
public class MapNameDisplay : MonoBehaviour
{
    public static MapNameDisplay Instance;

    [Header("UI References")]
    [Tooltip("Text component để hiển thị tên map (UI Text hoặc TextMeshPro)")]
    [SerializeField] private Text mapNameText;
    [SerializeField] private TextMeshProUGUI mapNameTextTMP;

    [Tooltip("CanvasGroup để làm fade effect")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [Tooltip("Thời gian fade in")]
    [SerializeField] private float fadeInDuration = 1f;

    [Tooltip("Thời gian hiển thị tên map")]
    [SerializeField] private float displayDuration = 3f;

    [Tooltip("Thời gian fade out")]
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Map Name Mapping")]
    [Tooltip("Tên đẹp cho từng map")]
    [SerializeField]
    private MapNameMapping[] mapNames = new MapNameMapping[]
    {
        new MapNameMapping { sceneInternalName = "RuinedCastle", displayName = "Ruined Castle" },
        new MapNameMapping { sceneInternalName = "HuntedRoom", displayName = "Hunted Room" },
        new MapNameMapping { sceneInternalName = "BossRoom", displayName = "Boss Room" }
    };

    private Coroutine currentAnimation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ẩn UI ban đầu
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Hiển thị tên map với hiệu ứng fade
    /// </summary>
    public void ShowMapName(string sceneInternalName)
    {
        // Tìm display name
        string displayName = GetDisplayName(sceneInternalName);

        if (string.IsNullOrEmpty(displayName))
        {
            Debug.LogWarning($"⚠️ MapNameDisplay: Không tìm thấy display name cho '{sceneInternalName}'");
            return;
        }

        // Dừng animation cũ nếu đang chạy
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Bắt đầu animation mới
        currentAnimation = StartCoroutine(DisplayMapNameRoutine(displayName));
    }

    private IEnumerator DisplayMapNameRoutine(string displayName)
    {
        // Set text
        if (mapNameText != null)
        {
            mapNameText.text = displayName;
        }
        if (mapNameTextTMP != null)
        {
            mapNameTextTMP.text = displayName;
        }

        // Fade In
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeInDuration));

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeOutDuration));

        currentAnimation = null;
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private string GetDisplayName(string sceneInternalName)
    {
        foreach (var mapping in mapNames)
        {
            if (mapping.sceneInternalName == sceneInternalName)
            {
                return mapping.displayName;
            }
        }
        return sceneInternalName; // Fallback: dùng tên scene gốc
    }
}

[System.Serializable]
public class MapNameMapping
{
    [Tooltip("Tên scene trong Unity (ví dụ: RuinedCastle)")]
    public string sceneInternalName;

    [Tooltip("Tên hiển thị cho người chơi (ví dụ: Ruined Castle)")]
    public string displayName;
}
