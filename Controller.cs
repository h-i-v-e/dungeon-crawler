using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace Depravity
{
    public class Controller : MonoBehaviour
    {
        private static Controller instance;

        private readonly List<GameObject> rootBuffer = new List<GameObject>();
        private readonly Dictionary<string, int> seeds = new Dictionary<string, int>();

        private enum State
        {
            onMenu, playing
        };

        [SerializeField]
        private Material interactableMaterial;

        [SerializeField]
        private Monster player;

        [SerializeField]
        private GameObject ui;

        [SerializeField]
        private MainMenu menu;

        [SerializeField]
        private UIFader loadingScreen;

        [SerializeField]
        private TextMeshProUGUI message;

        [SerializeField]
        private TextMeshProUGUI conversationText;

        [SerializeField]
        private GameObject loadingCamera;

        [SerializeField, Header("Collider Settings")]
        private int weaponLayer;

        [SerializeField]
        private int shieldLayer;

        [SerializeField]
        private int damageLayer;

        [SerializeField]
        private int monsterLayer;

        private Scene currentScene, controllerScene;
        private State state = State.onMenu;
        private string nextScene;
        private PlayerController playerController;
        private bool enterAtExit = false;
        private Dungeon dungeon;

        private void Awake()
        {
            instance = this;
            Physics.IgnoreLayerCollision(monsterLayer, shieldLayer, true);
            Physics.IgnoreLayerCollision(monsterLayer, weaponLayer, true);
            Physics.IgnoreLayerCollision(monsterLayer, damageLayer, true);
            Physics.IgnoreLayerCollision(shieldLayer, damageLayer, true);
            Physics.IgnoreLayerCollision(damageLayer, 0, true);
            controllerScene = SceneManager.GetActiveScene();
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
            playerController = player.GetComponent<PlayerController>();
        }

        private void Start()
        {
            LoadHomeScreen();
        }

        public static string ConversationText
        {
            get
            {
                return instance.conversationText.text;
            }
            set
            {
                if (value == null)
                {
                    instance.conversationText.gameObject.SetActive(false);
                }
                else
                {
                    var ct = instance.conversationText;
                    ct.text = value;
                    ct.gameObject.SetActive(true);
                }
            }
        }

        private void SetMenuActive(bool value, UIFader.OpacityChangeComplete complete = null)
        {
            if (menu == null)
            {
                return;
            }
            if (value)
            {
                Time.timeScale = 0.0f;
                CursorEnabled = true;
                playerController.enabled = false;
                menu.Show(complete);
            }
            else
            {
                Time.timeScale = 1.0f;
                CursorEnabled = false;
                playerController.enabled = state == State.playing;
                menu.Hide(complete);
            }
        }

        public static bool MenuActive
        {
            get
            {
                return instance.menu.gameObject.activeSelf;
            }
            set
            {
                instance.SetMenuActive(value);
            }
        }

        public static void LoadHomeScreen()
        {
            instance.state = State.onMenu;
            instance.LoadScene("Menu");
        }

        private void ResetPlayer()
        {
            var trans = player.gameObject.transform;
            trans.SetParent(null, false);
            trans.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void MovePlayerToNewScene(Scene newScene)
        {
            ResetPlayer();
            SceneManager.MoveGameObjectToScene(player.gameObject, newScene);
        }

        private int GetDungeonSeed(Scene scene)
        {
            var name = scene.name;
            if (!seeds.TryGetValue(name, out int seed))
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
                seeds.Add(name, seed);
            }
            return seed;
        }

        private void SetRootObjectsActive(bool active)
        {
            for (int i = 0, j = rootBuffer.Count; i != j; rootBuffer[i++].SetActive(active)) ;
        }

        private Dungeon FindDungeon()
        {
            for (int i = 0, j = rootBuffer.Count; i != j; ++i)
            {
                var dungeon = rootBuffer[i].GetComponent<Dungeon>();
                if (dungeon != null)
                {
                    return dungeon;
                }
            }
            return null;
        }

        private void SetDungeonSeed(Scene scene)
        {
            if (dungeon != null)
            {
                dungeon.seed = GetDungeonSeed(scene);
            }
        }

        private void PlacePlayerAtExit()
        {
            var exit = dungeon.FindExitBlock();
            Debug.Assert(exit != null, "Dungeon has no exit");
            player.transform.SetPositionAndRotation(exit.transform.position, Quaternion.LookRotation(Vector3.back));
        }

        private void SceneLoaded(Scene newScene, LoadSceneMode loadMode)
        {
            if (nextScene == null || nextScene != newScene.name)
            {
                return;
            }
            if (state != State.onMenu)
            {
                MovePlayerToNewScene(newScene);
            }
            newScene.GetRootGameObjects(rootBuffer);
            dungeon = FindDungeon();
            SetDungeonSeed(newScene);
            SceneManager.SetActiveScene(newScene);
            SetRootObjectsActive(true);
            if (ui != null)
            {
                ui.SetActive(state == State.playing);
            }
            if (loadingCamera != null)
            {
                loadingCamera.SetActive(false);
            }
            if (state == State.onMenu)
            {
                SetMenuActive(true);
            }
            else
            {
                player.gameObject.SetActive(true);
                var ac = player.AnimationManager;
                player.AddToLevel();
                ac.SetActivity(state == State.playing ? MonsterAnimationManager.Activity.ATTACKING : MonsterAnimationManager.Activity.SHOPPING);
                ac.SetState(0.0f, 0.0f, 0.0f);
            }
            currentScene = newScene;
            if (enterAtExit)
            {
                PlacePlayerAtExit();
            }
            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }
        }

        private void LoadNewScene()
        {
            SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        }

        private void SceneUnloaded(Scene oldScene)
        {
            LoadNewScene();
        }

        private void LoadingScreenShowing()
        {
            player.gameObject.transform.parent = null;
            currentScene.GetRootGameObjects(rootBuffer);
            SetRootObjectsActive(false);
            loadingCamera.SetActive(true);
            SceneManager.MoveGameObjectToScene(player.gameObject, controllerScene);
            SceneManager.SetActiveScene(controllerScene);
            SceneManager.UnloadSceneAsync(currentScene);
        }

        private void LoadScene(string sceneName)
        {
            if (state != State.onMenu)
            {
                SetMenuActive(false);
            }
            nextScene = sceneName;
            if (currentScene.name != null)
            {
                loadingScreen.Show(LoadingScreenShowing);
            }
            else
            {
                LoadNewScene();
            }
        }

        public static string SceneName
        {
            get
            {
                return instance.currentScene.name;
            }
            set
            {
                instance.state = State.playing;
                instance.LoadScene(value);
            }
        }

        public static bool StartPlayerAtExit
        {
            set
            {
                instance.enterAtExit = value;
            }
            get
            {
                return instance.enterAtExit;
            }
        }

        public static Material InteractableMaterial
        {
            get
            {
                return instance.interactableMaterial;
            }
        }

        public static Monster Player
        {
            get
            {
                return instance.player;
            }
        }

        public static void ShowMessage(string message)
        {
            instance.message.text = message;
            instance.message.gameObject.SetActive(true);
        }

        public static void HideMessage()
        {
            instance.message.gameObject.SetActive(false);
        }

        public static bool CursorEnabled
        {
            get
            {
                return Cursor.visible;
            }
            set
            {
                Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = value;
            }
        }

        public static int WeaponLayer
        {
            get
            {
                return instance.weaponLayer;
            }
        }

        public static int DamageLayer
        {
            get
            {
                return instance.damageLayer;
            }
        }

        public static int ShieldLayer
        {
            get
            {
                return instance.shieldLayer;
            }
        }

        public static int MonsterLayer
        {
            get
            {
                return instance.monsterLayer;
            }
        }

        /*private static void SetPlayerControlled(bool controlled)
        {
            var player = instance.player;
            var go = player.gameObject;
            go.GetComponent<PlayerController>().enabled = controlled;
            go.GetComponentInChildren<Camera>().enabled = controlled;
            go.GetComponentInChildren<AudioListener>().enabled = controlled;
            go.GetComponent<Animator>().applyRootMotion = controlled;
            go.GetComponent<Rigidbody>().isKinematic = !controlled;
        }

        public static void EnterShop(string scene)
        {
            instance.state = State.shopping;
            SetPlayerControlled(false);
            instance.LoadScene(scene);
        }

        public static void ExitShop(string scene)
        {
            instance.state = State.playing;
            SetPlayerControlled(true);
            instance.player.gameObject.SetActive(false);
            instance.LoadScene(scene);
        }*/

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                MenuActive = !MenuActive;
            }
        }
    }
}
