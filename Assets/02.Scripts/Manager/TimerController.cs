using System;
using UnityEngine;

namespace SUDOKU.Manager
{
    public class TimerController : MonoBehaviour
    {
        [Tooltip("TimerController가 처음 활성화될 때 자동으로 시간을 측정할지 여부입니다.")]
        [SerializeField] private bool startAutomatically = true;

        private double elapsedSeconds;
        private int previousElapsedWholeSeconds = -1;
        private bool isRunning;

        public event Action<int> ElapsedSecondChanged;
        public event Action<bool> RunningStateChanged;

        public double ElapsedSeconds => elapsedSeconds;
        public int ElapsedWholeSeconds => Mathf.FloorToInt((float)elapsedSeconds);
        public bool IsRunning => isRunning;

        #region Unity Callbacks

        private void Start()
        {
            if (startAutomatically)
            {
                StartTimer();
            }
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            NotifyElapsedSecondIfChanged();
        }

        #endregion

        /// <summary>
        /// 현재 경과 시간을 유지한 상태로 시간 측정을 시작합니다.
        /// </summary>
        public void StartTimer()
        {
            SetRunningState(true);
            NotifyElapsedSecondIfChanged();
        }

        /// <summary>
        /// 현재 경과 시간을 유지하고 시간 측정을 일시정지합니다.
        /// </summary>
        public void PauseTimer()
        {
            SetRunningState(false);
        }

        /// <summary>
        /// 일시정지된 현재 경과 시간부터 시간 측정을 재개합니다.
        /// </summary>
        public void ResumeTimer()
        {
            SetRunningState(true);
        }

        /// <summary>
        /// 현재 경과 시간을 유지하고 시간 측정을 정지합니다.
        /// </summary>
        public void StopTimer()
        {
            SetRunningState(false);
        }

        /// <summary>
        /// 시간 측정을 정지하고 경과 시간을 0초로 초기화합니다.
        /// </summary>
        public void ResetTimer()
        {
            SetRunningState(false);
            elapsedSeconds = 0d;
            previousElapsedWholeSeconds = -1;
            NotifyElapsedSecondIfChanged();
        }

        /// <summary>
        /// 경과 시간을 0초로 초기화하고 즉시 시간 측정을 다시 시작합니다.
        /// </summary>
        public void RestartTimer()
        {
            elapsedSeconds = 0d;
            previousElapsedWholeSeconds = -1;
            SetRunningState(true);
            NotifyElapsedSecondIfChanged();
        }

        /// <summary>
        /// Timer 실행 상태를 변경하고 상태 변경 이벤트를 전달합니다.
        /// </summary>
        /// <param name="shouldRun">시간을 측정해야 하면 true입니다.</param>
        private void SetRunningState(bool shouldRun)
        {
            if (isRunning == shouldRun)
            {
                return;
            }

            isRunning = shouldRun;
            RunningStateChanged?.Invoke(isRunning);
        }

        /// <summary>
        /// 정수 단위 경과 초가 변경된 경우에만 시간 변경 이벤트를 전달합니다.
        /// </summary>
        private void NotifyElapsedSecondIfChanged()
        {
            int currentWholeSeconds = ElapsedWholeSeconds;

            if (previousElapsedWholeSeconds == currentWholeSeconds)
            {
                return;
            }

            previousElapsedWholeSeconds = currentWholeSeconds;
            ElapsedSecondChanged?.Invoke(currentWholeSeconds);
        }
    }
}
