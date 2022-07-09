using UnityEngine;
using UnityEngine.UI;

namespace Depravity
{
    [RequireComponent(typeof(UIFader), typeof(AudioSource))]
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button newGame, quit, settings, backButton;

        [SerializeField]
        private GameObject mainMenu, settingsMenu;

        [SerializeField]
        private string startScene = "Level1";

        private UIFader fader;
        private AudioSource music;

        private void NewGame()
        {
            Controller.SceneName = startScene;
        }

        private void Quit()
        {
            Application.Quit();
        }

        private void OpenSettings()
        {
            settingsMenu.SetActive(true);
            mainMenu.SetActive(false);
        }

        private void GoBack()
        {
            if (settingsMenu.activeSelf)
            {
                settingsMenu.SetActive(false);
                mainMenu.SetActive(true);
            }
        }

        private void AdjustVolume(float level)
        {
            music.volume = level;
        }

        private void Awake()
        {
            fader = GetComponent<UIFader>();
            Debug.Log("Fader: " + fader);
            music = GetComponent<AudioSource>();
            fader.OnOpacityChanged += AdjustVolume;
        }

        private void Start()
        {
            newGame.onClick.AddListener(NewGame);
            quit.onClick.AddListener(Quit);
            settings.onClick.AddListener(OpenSettings);
            backButton.onClick.AddListener(GoBack);
        }

        public void Hide(UIFader.OpacityChangeComplete ready = null)
        {
            fader.Hide(ready);
        }

        public void Show(UIFader.OpacityChangeComplete ready = null)
        {
            Debug.Log("Fader: " + fader);
            fader.Show(ready);
        }
    }
}
