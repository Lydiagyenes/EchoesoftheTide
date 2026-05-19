using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu_Scene";

    void Start()
    {
        // Feliratkozunk az eseményre: ha vége a videónak, fusson le az EndReached
        videoPlayer.loopPointReached += EndReached;
          if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }

    void Update()
    {
        // Ha megnyomod az ESC-et, azonnal továbbugrunk
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadNextScene();
        }
    }

    // Ez fut le magától, ha vége a videónak
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}