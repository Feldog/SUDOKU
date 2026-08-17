using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.ClearMenu
{
    using Manager;

    public class ClearMenuController : MonoBehaviour
    {
        private const string CompletionTimeLabelName = "completion-time-label";
        private const string RestartButtonName = "restart-button";
        private const string MainMenuButtonName = "mainmenu-button";

        [Tooltip("완료 시간과 Clear 버튼을 포함한 UI Document입니다.")]
        [SerializeField] private UIDocument clearMenuDocument;

        private Label completionTimeLabel;
        private Button restartButton;
        private Button mainMenuButton;
        private bool callbacksRegistered;
        private bool hasStarted;

        public event Action RestartRequested;
        public event Action MainMenuRequested;

        public int CompletionTimeSeconds { get; private set; }

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            Hide();
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
        }

        #endregion

        #region Visibility

        /// <summary>
        /// 완료 시간을 저장하고 Clear UI를 표시합니다.
        /// </summary>
        /// <param name="completionTimeSeconds">게임 시작부터 완료까지 경과한 전체 초입니다.</param>
        public void Show(int completionTimeSeconds)
        {
            if (!CacheVisualElements())
            {
                return;
            }

            CompletionTimeSeconds = Mathf.Max(0, completionTimeSeconds);
            ApplyCompletionTime(CompletionTimeSeconds);
            clearMenuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Clear UI를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (clearMenuDocument != null)
            {
                clearMenuDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region UI Callbacks

        /// <summary>
        /// Clear UI 요소를 캐싱하고 버튼 이벤트를 등록합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheVisualElements())
            {
                return;
            }

            restartButton.clicked += OnRestartButtonClicked;
            mainMenuButton.clicked += OnMainMenuButtonClicked;
            callbacksRegistered = true;
        }

        /// <summary>
        /// Clear UI 버튼에 등록한 이벤트를 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            restartButton.clicked -= OnRestartButtonClicked;
            mainMenuButton.clicked -= OnMainMenuButtonClicked;
            callbacksRegistered = false;
        }

        /// <summary>
        /// Restart 버튼 입력을 외부 게임 흐름 Controller에 전달합니다.
        /// </summary>
        private void OnRestartButtonClicked()
        {
            RestartRequested?.Invoke();
            RequestSceneChange(true);
        }

        /// <summary>
        /// Mainmenu 버튼 입력을 외부 게임 흐름 Controller에 전달합니다.
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            MainMenuRequested?.Invoke();
            RequestSceneChange(false);
        }

        #endregion

        #region Scene Transition

        /// <summary>
        /// 정지된 게임 시간을 복구하고 GameManager에 Scene 전환을 요청합니다.
        /// </summary>
        /// <param name="shouldRestart">게임 Scene을 다시 불러오려면 true, Main Menu로 이동하려면 false입니다.</param>
        private void RequestSceneChange(bool shouldRestart)
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                Debug.LogError("Clear Menu의 Scene 전환을 요청할 GameManager를 찾을 수 없습니다.", this);
                return;
            }

            Time.timeScale = 1f;

            bool sceneChangeRequested = shouldRestart
                ? gameManager.RestartGame()
                : gameManager.ReturnToMainMenu();

            if (!sceneChangeRequested)
            {
                Time.timeScale = 0f;
            }
        }

        #endregion

        #region View

        /// <summary>
        /// Clear UI Document에서 완료 시간 Label과 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 UI 요소를 모두 찾았으면 true입니다.</returns>
        private bool CacheVisualElements()
        {
            if (clearMenuDocument == null)
            {
                Debug.LogError("Clear Menu UI Document를 연결해야 합니다.", this);
                return false;
            }

            VisualElement root = clearMenuDocument.rootVisualElement;
            completionTimeLabel ??= root.Q<Label>(CompletionTimeLabelName);
            restartButton ??= root.Q<Button>(RestartButtonName);
            mainMenuButton ??= root.Q<Button>(MainMenuButtonName);

            if (completionTimeLabel != null && restartButton != null && mainMenuButton != null)
            {
                return true;
            }

            Debug.LogError("Clear Menu UI에서 완료 시간 Label 또는 버튼을 찾을 수 없습니다.", this);
            return false;
        }

        /// <summary>
        /// 완료 시간을 MM:SS 형식으로 Clear UI Label에 표시합니다.
        /// </summary>
        /// <param name="completionTimeSeconds">표시할 전체 경과 초입니다.</param>
        private void ApplyCompletionTime(int completionTimeSeconds)
        {
            int minutes = completionTimeSeconds / 60;
            int seconds = completionTimeSeconds % 60;

            completionTimeLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}",
                minutes,
                seconds);
        }

        #endregion
    }
}
