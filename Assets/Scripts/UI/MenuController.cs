using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button stuckButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Start() {
        if (resumeButton) {
            resumeButton.onClick.AddListener(Resume);
        }

        if (stuckButton) {
            stuckButton.onClick.AddListener(Stuck);
        }

        if (mainMenuButton) {
            mainMenuButton.onClick.AddListener(MainMenu);
        }

        if (quitButton) {
            quitButton.onClick.AddListener(Quit);
        }
    }

    private void Resume() {
        CanvasManager.Instance.CanCloseMenu(MenuType.Inventory);
    }

    private void Stuck() {
        PlayerHUDEvents.OnUnstuckPlayer.Invoke();
    }

    private void MainMenu() {
        //SceneManager.LoadScene("MainMenu");
    }
    
    private void Quit() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        
        Application.Quit();
    }
}
