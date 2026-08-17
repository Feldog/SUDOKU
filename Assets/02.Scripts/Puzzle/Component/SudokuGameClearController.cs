using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using SUDOKU.Manager;
    using SUDOKU.UI.ClearMenu;

    public class SudokuGameClearController : MonoBehaviour
    {
        [Tooltip("게임 완료 상태를 확인할 Cell Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        [Tooltip("게임 완료 시간을 제공하고 정지할 Timer Controller입니다.")]
        [SerializeField] private TimerController timerController;

        [Tooltip("완료 시간과 Clear 화면을 표시할 UI Controller입니다.")]
        [SerializeField] private ClearMenuController clearMenuController;

        private bool isGameCleared;

        public bool IsGameCleared => isGameCleared;

        #region Unity Callbacks

        private void OnEnable()
        {
            if (cellController != null)
            {
                cellController.CellDataUpdated += OnCellDataUpdated;
            }
        }

        private void Start()
        {
            ValidateReferences();
        }

        private void OnDisable()
        {
            if (cellController != null)
            {
                cellController.CellDataUpdated -= OnCellDataUpdated;
            }
        }

        #endregion

        #region Game Clear

        /// <summary>
        /// Cell 데이터 변경이 완료된 후 전체 Sudoku 완료 여부를 확인합니다.
        /// </summary>
        private void OnCellDataUpdated()
        {
            if (isGameCleared || cellController == null || !cellController.IsBoardCompleted())
            {
                return;
            }

            CompleteGame();
        }

        /// <summary>
        /// 현재 완료 시간을 확정하고 Clear UI를 표시한 뒤 게임 시간을 정지합니다.
        /// </summary>
        private void CompleteGame()
        {
            if (timerController == null || clearMenuController == null)
            {
                Debug.LogError("게임 완료 처리에 Timer Controller와 Clear Menu Controller가 필요합니다.", this);
                return;
            }

            isGameCleared = true;
            timerController.StopTimer();
            clearMenuController.Show(timerController.ElapsedWholeSeconds);
            Time.timeScale = 0f;
        }

        #endregion

        #region Validation

        /// <summary>
        /// 게임 완료 처리에 필요한 Inspector 참조가 연결되었는지 확인합니다.
        /// </summary>
        /// <returns>필요한 참조가 모두 연결되어 있으면 true입니다.</returns>
        private bool ValidateReferences()
        {
            if (cellController != null && timerController != null && clearMenuController != null)
            {
                return true;
            }

            Debug.LogError("Sudoku Game Clear Controller의 참조를 모두 연결해야 합니다.", this);
            return false;
        }

        #endregion
    }
}
