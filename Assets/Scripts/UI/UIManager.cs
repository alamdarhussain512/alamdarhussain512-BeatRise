using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public void LoadScene(string sceneName) {
        if(!Application.isPlaying) return;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
