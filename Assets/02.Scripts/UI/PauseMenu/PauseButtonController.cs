using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.PauseMenu
{
    public class PauseButtonController : MonoBehaviour
    {
        private const string PauseButtonName = "pause-button";

        [Tooltip("Pause 버튼을 포함한 Player Sudoku Control UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        [Tooltip("Pause 버튼 입력을 전달할 Pause Menu Controller입니다.")]
        [SerializeField] private PauseMenuController pauseMenuController;

        private Button pauseButton;
        private bool callbackRegistered;
        private bool hasStarted;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallback();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallback();
            }
        }

        private void OnDisable()
        {
            UnregisterCallback();
        }

        #endregion

        /// <summary>
        /// Player Sudoku Control UI에서 Pause 버튼을 찾아 클릭 이벤트를 연결합니다.
        /// </summary>
        private void RegisterCallback()
        {
            if (callbackRegistered)
            {
                return;
            }

            if (playerControlDocument == null || pauseMenuController == null)
            {
                Debug.LogError("Pause Button Controller에 UI Document와 Pause Menu Controller를 연결해야 합니다.", this);
                return;
            }

            pauseButton = playerControlDocument.rootVisualElement.Q<Button>(PauseButtonName);

            if (pauseButton == null)
            {
                Debug.LogError($"Player Sudoku Control UI에서 {PauseButtonName}을 찾을 수 없습니다.", this);
                return;
            }

            pauseButton.clicked += OnPauseButtonClicked;
            callbackRegistered = true;
        }

        /// <summary>
        /// Pause 버튼에 등록한 클릭 이벤트를 해제합니다.
        /// </summary>
        private void UnregisterCallback()
        {
            if (!callbackRegistered)
            {
                return;
            }

            pauseButton.clicked -= OnPauseButtonClicked;
            callbackRegistered = false;
        }

        /// <summary>
        /// Pause 버튼 입력을 Pause Menu Controller에 전달합니다.
        /// </summary>
        private void OnPauseButtonClicked()
        {
            pauseMenuController.PauseGame();
        }
    }
}
