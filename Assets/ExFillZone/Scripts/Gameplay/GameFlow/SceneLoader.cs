using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExFillZone.Gameplay.GameFlow
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void RestartScene()
        {
            Scene currentScene =
                SceneManager.GetActiveScene();

            SceneManager.LoadScene(
                currentScene.name
            );
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}