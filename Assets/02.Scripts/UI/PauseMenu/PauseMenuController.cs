using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.PauseMenu
{
    using Manager;
    using OptionMenu;

    public class PauseMenuController : MonoBehaviour
    {
        [Tooltip("Resume, Option, Mainmenu 버튼을 포함한 Pause Menu UI Document입니다.")]
        [SerializeField] private UIDocument pauseMenuDocument;

        [Tooltip("Pause Menu에서 열 Option Menu Controller입니다.")]
        [SerializeField] private OptionMenuController optionMenuController;

        [Tooltip("Pause 상태의 배경 Alpha와 입력 차단을 적용할 Controller입니다.")]
        [SerializeField] private PauseBackgroundController pauseBackgroundController;

        private Button resumeButton;
        private Button optionButton;
        private Button mainMenuButton;
        private float previousTimeScale = 1f;
        private bool isPaused;
        private bool callbacksRegistered;
        private bool hasStarted;

        public bool IsPaused => isPaused;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            HidePauseMenu();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
            }
        }

        private void OnDisable()
        {
            UnregisterCallbacks();

            if (isPaused)
            {
                RestoreGameTime();
            }
        }

        #endregion

        /// <summary>
        /// 게임 진행을 일시정지하고 Pause Menu를 표시합니다.
        /// </summary>
        public void PauseGame()
        {
            if (isPaused)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
            pauseBackgroundController.SetPauseState(true);
            optionMenuController?.Hide();
            ShowPauseMenu();
        }

        /// <summary>
        /// Pause Menu를 닫고 게임 진행과 Timer를 재개합니다.
        /// </summary>
        public void ResumeGame()
        {
            if (!isPaused)
            {
                return;
            }

            HidePauseMenu();
            optionMenuController?.Hide();
            RestoreGameTime();
        }

        /// <summary>
        /// Pause Menu 버튼과 Option Return 이벤트를 연결합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheButtons())
            {
                return;
            }

            resumeButton.clicked += ResumeGame;
            optionButton.clicked += ShowOptionMenu;
            mainMenuButton.clicked += ReturnToMainMenu;
            optionMenuController.ReturnRequested += ReturnFromOptionMenu;
            callbacksRegistered = true;
        }

        /// <summary>
        /// Pause Menu 버튼과 Option Return 이벤트 연결을 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            resumeButton.clicked -= ResumeGame;
            optionButton.clicked -= ShowOptionMenu;
            mainMenuButton.clicked -= ReturnToMainMenu;
            optionMenuController.ReturnRequested -= ReturnFromOptionMenu;
            callbacksRegistered = false;
        }

        /// <summary>
        /// Pause Menu UI Document에서 기능에 필요한 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 참조와 버튼을 모두 찾았으면 true입니다.</returns>
        private bool CacheButtons()
        {
            if (pauseBackgroundController == null)
            {
                pauseBackgroundController = FindFirstObjectByType<PauseBackgroundController>();
            }

            if (pauseMenuDocument == null || optionMenuController == null || pauseBackgroundController == null)
            {
                Debug.LogError("Pause Menu Controller에 UI Document, Option Controller와 Background Controller를 연결해야 합니다.", this);
                return false;
            }

            VisualElement pauseRoot = pauseMenuDocument.rootVisualElement;
            resumeButton = pauseRoot.Q<Button>("resume-button");
            optionButton = pauseRoot.Q<Button>("option-button");
            mainMenuButton = pauseRoot.Q<Button>("mainmenu-button");

            if (resumeButton == null || optionButton == null || mainMenuButton == null)
            {
                Debug.LogError("Pause 기능에 필요한 버튼 중 일부를 찾을 수 없습니다.", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pause Menu를 숨기고 Option Menu를 표시합니다.
        /// </summary>
        private void ShowOptionMenu()
        {
            HidePauseMenu();
            optionMenuController.Show();
        }

        /// <summary>
        /// Option Menu를 숨기고 Pause Menu로 돌아갑니다.
        /// </summary>
        private void ReturnFromOptionMenu()
        {
            optionMenuController.Hide();
            ShowPauseMenu();
        }

        /// <summary>
        /// Time Scale을 복구한 뒤 GameManager에 Main Menu 전환을 요청합니다.
        /// </summary>
        private void ReturnToMainMenu()
        {
            RestoreGameTime();

            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                Debug.LogError("Main Menu 전환을 요청할 GameManager를 찾을 수 없습니다.", this);
                return;
            }

            gameManager.ReturnToMainMenu();
        }

        /// <summary>
        /// Pause Menu UI Document를 표시합니다.
        /// </summary>
        private void ShowPauseMenu()
        {
            SetPauseMenuVisibility(true);
        }

        /// <summary>
        /// Pause Menu UI Document를 숨깁니다.
        /// </summary>
        private void HidePauseMenu()
        {
            SetPauseMenuVisibility(false);
        }

        /// <summary>
        /// Pause 이전 Time Scale을 복구하고 Pause 상태를 해제합니다.
        /// </summary>
        private void RestoreGameTime()
        {
            bool wasPaused = isPaused;
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
            isPaused = false;

            if (wasPaused)
            {
                pauseBackgroundController.SetPauseState(false);
            }
        }

        /// <summary>
        /// Pause Menu UI Document의 표시 상태를 변경합니다.
        /// </summary>
        /// <param name="shouldShow">Pause Menu를 표시해야 하면 true입니다.</param>
        private void SetPauseMenuVisibility(bool shouldShow)
        {
            if (pauseMenuDocument == null)
            {
                return;
            }

            pauseMenuDocument.rootVisualElement.style.display = shouldShow
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
