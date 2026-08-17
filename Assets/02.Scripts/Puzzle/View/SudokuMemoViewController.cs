using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.View
{
    using Component;
    using Define;

    public class SudokuMemoViewController : MonoBehaviour
    {
        private const string ActiveMemoButtonClass = "memo-active";
        private const string VisibleMemoLabelClass = "memo-visible";

        [Tooltip("Memo 버튼을 포함한 플레이어 컨트롤 UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        [Tooltip("셀별 Memo Label을 포함한 게임 보드 UI Document입니다.")]
        [SerializeField] private UIDocument gameBoardDocument;

        [Tooltip("Memo 상태와 셀별 Memo 데이터를 관리할 Controller입니다.")]
        [SerializeField] private SudokuMemoController memoController;

        private readonly Label[,] memoLabels = new Label[SudokuDefine.CellCount, SudokuDefine.MaxCellValue];

        private Button memoButton;
        private bool callbacksRegistered;
        private bool hasStarted;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            RefreshView();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
                RefreshView();
            }
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        #endregion

        /// <summary>
        /// Memo 데이터와 상태 변경 이벤트를 View에 연결합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheVisualElements())
            {
                return;
            }

            memoController.MemoStateChanged += OnMemoStateChanged;
            memoController.MemoValuesChanged += OnMemoValuesChanged;
            callbacksRegistered = true;
        }

        /// <summary>
        /// Memo 데이터와 상태 변경 이벤트 연결을 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            memoController.MemoStateChanged -= OnMemoStateChanged;
            memoController.MemoValuesChanged -= OnMemoValuesChanged;
            callbacksRegistered = false;
        }

        /// <summary>
        /// UI Document에서 Memo 버튼과 모든 셀의 1~9 Memo Label을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 UI 요소를 찾았으면 true입니다.</returns>
        private bool CacheVisualElements()
        {
            if (playerControlDocument == null || gameBoardDocument == null || memoController == null)
            {
                Debug.LogError("Memo View에 두 UI Document와 Memo Controller를 모두 연결해야 합니다.", this);
                return false;
            }

            memoButton = playerControlDocument.rootVisualElement.Q<Button>("memo-button");

            if (memoButton == null)
            {
                Debug.LogError("플레이어 컨트롤 UI에서 memo-button을 찾을 수 없습니다.", this);
                return false;
            }

            VisualElement boardRoot = gameBoardDocument.rootVisualElement;

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
                {
                    Label memoLabel = boardRoot.Q<Label>($"memo-label-{cellIndex}-{value}");

                    if (memoLabel == null)
                    {
                        Debug.LogError($"게임 보드에서 memo-label-{cellIndex}-{value}를 찾을 수 없습니다.", this);
                        return false;
                    }

                    memoLabels[cellIndex, value - SudokuDefine.MinCellValue] = memoLabel;
                }
            }

            return true;
        }

        /// <summary>
        /// Memo 버튼 상태와 모든 셀의 Memo 표시를 현재 데이터로 갱신합니다.
        /// </summary>
        private void RefreshView()
        {
            if (memoController == null || memoButton == null)
            {
                return;
            }

            ApplyMemoButtonState(memoController.IsMemoActive);

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                RefreshCellMemo(cellIndex);
            }
        }

        /// <summary>
        /// 변경된 Memo 상태에 맞춰 버튼 표시를 갱신합니다.
        /// </summary>
        /// <param name="isActive">현재 Memo 활성 상태입니다.</param>
        private void OnMemoStateChanged(bool isActive)
        {
            ApplyMemoButtonState(isActive);
        }

        /// <summary>
        /// Memo 데이터가 변경된 셀의 3×3 Memo 표시만 다시 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">Memo 데이터가 변경된 셀 인덱스입니다.</param>
        private void OnMemoValuesChanged(int cellIndex)
        {
            RefreshCellMemo(cellIndex);
        }

        /// <summary>
        /// 지정한 셀의 Memo 데이터와 1~9 Label 활성 상태를 일치시킵니다.
        /// </summary>
        /// <param name="cellIndex">Memo 표시를 갱신할 셀 인덱스입니다.</param>
        private void RefreshCellMemo(int cellIndex)
        {
            for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                memoLabels[cellIndex, value - SudokuDefine.MinCellValue]
                    .EnableInClassList(VisibleMemoLabelClass, memoController.HasMemoValue(cellIndex, value));
            }
        }

        /// <summary>
        /// Memo 활성 상태에 따라 버튼의 전용 스타일 클래스를 갱신합니다.
        /// </summary>
        /// <param name="isActive">Memo 버튼을 활성 상태로 표시할지 여부입니다.</param>
        private void ApplyMemoButtonState(bool isActive)
        {
            memoButton.EnableInClassList(ActiveMemoButtonClass, isActive);
        }
    }
}
