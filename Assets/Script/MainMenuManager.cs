using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("หน้าต่างตั้งค่า")]
    public GameObject settingsPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Debug.Log("ออกเกมแล้ว!");
        Application.Quit();
    }
}