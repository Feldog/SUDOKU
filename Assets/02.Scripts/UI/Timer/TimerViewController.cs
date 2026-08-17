using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.Timer
{
    using Manager;

    public class TimerViewController : MonoBehaviour
    {
        private const string TimerLabelName = "timer-label";

        [Tooltip("Timer Label을 포함한 플레이어 컨트롤 UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        [Tooltip("게임 경과 시간을 제공할 Timer Controller입니다.")]
        [SerializeField] private TimerController timerController;

        private Label timerLabel;
        private bool callbackRegistered;
        private bool hasStarted;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            RefreshTimerLabel();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
                RefreshTimerLabel();
            }
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        #endregion

        /// <summary>
        /// UI Document의 Timer Label을 찾고 경과 시간 변경 이벤트를 연결합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbackRegistered)
            {
                return;
            }

            if (playerControlDocument == null || timerController == null)
            {
                Debug.LogError("Timer View에 UI Document와 Timer Controller를 모두 연결해야 합니다.", this);
                return;
            }

            timerLabel = playerControlDocument.rootVisualElement.Q<Label>(TimerLabelName);

            if (timerLabel == null)
            {
                Debug.LogError($"플레이어 컨트롤 UI에서 {TimerLabelName}을 찾을 수 없습니다.", this);
                return;
            }

            timerController.ElapsedSecondChanged += OnElapsedSecondChanged;
            callbackRegistered = true;
        }

        /// <summary>
        /// Timer Controller에 등록한 경과 시간 변경 이벤트를 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbackRegistered)
            {
                return;
            }

            if (timerController != null)
            {
                timerController.ElapsedSecondChanged -= OnElapsedSecondChanged;
            }

            callbackRegistered = false;
        }

        /// <summary>
        /// 현재 Timer Controller의 경과 시간을 Label에 즉시 반영합니다.
        /// </summary>
        private void RefreshTimerLabel()
        {
            if (timerLabel == null || timerController == null)
            {
                return;
            }

            ApplyElapsedTime(timerController.ElapsedWholeSeconds);
        }

        /// <summary>
        /// 초 단위 경과 시간 변경 이벤트를 받아 Timer Label을 갱신합니다.
        /// </summary>
        /// <param name="elapsedSeconds">게임 시작 후 경과한 전체 초입니다.</param>
        private void OnElapsedSecondChanged(int elapsedSeconds)
        {
            ApplyElapsedTime(elapsedSeconds);
        }

        /// <summary>
        /// 전체 경과 초를 MM:SS 문자열로 변환해 Timer Label에 표시합니다.
        /// </summary>
        /// <param name="elapsedSeconds">표시할 전체 경과 초입니다.</param>
        private void ApplyElapsedTime(int elapsedSeconds)
        {
            int minutes = elapsedSeconds / 60;
            int seconds = elapsedSeconds % 60;

            timerLabel.text = string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}",
                minutes,
                seconds);
        }
    }
}
