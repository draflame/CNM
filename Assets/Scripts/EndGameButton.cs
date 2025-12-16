using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameButton : MonoBehaviour
{
    /// <summary>
    /// Nút button gọi hàm này: Xóa save và quay về MainMenu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("🔄 Returning to MainMenu and deleting save...");

        // Xóa save file
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.DeleteSaveFile();
            SaveGameManager.Instance.ResetGameState();
            Debug.Log("✅ Save deleted successfully!");
        }
        else
        {
            Debug.LogWarning("⚠️ SaveGameManager not found!");
        }

        // Load MainMenu scene
        SceneManager.LoadScene("MainMenu");
    }

}
