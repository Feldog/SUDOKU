using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.Component
{
    using Define;

    public class SudokuController : MonoBehaviour
    {
        [Tooltip("셀 선택 입력을 받을 게임 보드 UI Document입니다.")]
        [SerializeField] private UIDocument gameBoardDocument;

        [Tooltip("숫자, 지우기, Memo 버튼 입력을 받을 플레이어 컨트롤 UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        [Tooltip("일반 셀 값을 관리할 Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        [Tooltip("Memo 상태와 셀별 Memo 값을 관리할 Controller입니다.")]
        [SerializeField] private SudokuMemoController memoController;

        private readonly VisualElement[] cells = new VisualElement[SudokuDefine.CellCount];
        private readonly VisualElement[] valueButtons = new VisualElement[SudokuDefine.MaxCellValue + 1];

        private Button memoButton;
        private int selectedCellIndex = -1;
        private bool callbacksRegistered;
        private bool hasStarted;

        public event Action<int> CellSelected;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
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

        /// <summary>
        /// 게임 보드 셀과 플레이어 입력 버튼의 이벤트를 등록합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheVisualElements())
            {
                return;
            }

            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                cells[cellIndex].userData = cellIndex;
                cells[cellIndex].focusable = true;
                cells[cellIndex].RegisterCallback<ClickEvent>(OnCellClicked);
            }

            for (int value = SudokuDefine.EmptyCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                valueButtons[value].userData = value;
                valueButtons[value].RegisterCallback<ClickEvent>(OnValueButtonClicked);
            }

            memoButton.clicked += OnMemoButtonClicked;
            cellController.CellValueChanged += OnCellValueChanged;
            callbacksRegistered = true;
            RefreshNumberButtonStates();
        }

        /// <summary>
        /// 게임 보드 셀과 플레이어 입력 버튼의 이벤트 연결을 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                cells[cellIndex]?.UnregisterCallback<ClickEvent>(OnCellClicked);
            }

            for (int value = SudokuDefine.EmptyCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                valueButtons[value]?.UnregisterCallback<ClickEvent>(OnValueButtonClicked);
            }

            memoButton.clicked -= OnMemoButtonClicked;

            if (cellController != null)
            {
                cellController.CellValueChanged -= OnCellValueChanged;
            }

            callbacksRegistered = false;
        }

        /// <summary>
        /// 두 UI Document에서 셀과 플레이어 입력 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>입력 처리에 필요한 모든 요소를 찾았으면 true입니다.</returns>
        private bool CacheVisualElements()
        {
            if (gameBoardDocument == null || playerControlDocument == null
                || cellController == null || memoController == null)
            {
                Debug.LogError("Sudoku 입력 처리에 필요한 UI Document와 Controller를 모두 연결해야 합니다.", this);
                return false;
            }

            VisualElement boardRoot = gameBoardDocument.rootVisualElement;
            VisualElement controlRoot = playerControlDocument.rootVisualElement;

            for (int row = 0; row < SudokuDefine.BoardSize; row++)
            {
                for (int column = 0; column < SudokuDefine.BoardSize; column++)
                {
                    int cellIndex = row * SudokuDefine.BoardSize + column;
                    cells[cellIndex] = boardRoot.Q<VisualElement>($"cell-{row}-{column}");

                    if (cells[cellIndex] == null)
                    {
                        Debug.LogError($"게임 보드에서 cell-{row}-{column}을 찾을 수 없습니다.", this);
                        return false;
                    }
                }
            }

            valueButtons[SudokuDefine.EmptyCellValue] = controlRoot.Q<VisualElement>("erase-button");

            for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                valueButtons[value] = controlRoot.Q<VisualElement>($"number-button-{value}");
            }

            for (int value = SudokuDefine.EmptyCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                if (valueButtons[value] == null)
                {
                    Debug.LogError($"플레이어 컨트롤 UI에서 값 {value} 입력 버튼을 찾을 수 없습니다.", this);
                    return false;
                }
            }

            memoButton = controlRoot.Q<Button>("memo-button");

            if (memoButton == null)
            {
                Debug.LogError("플레이어 컨트롤 UI에서 memo-button을 찾을 수 없습니다.", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 클릭한 셀을 현재 플레이어 입력 대상으로 지정합니다.
        /// </summary>
        /// <param name="clickEvent">선택한 셀 정보를 포함한 클릭 이벤트입니다.</param>
        private void OnCellClicked(ClickEvent clickEvent)
        {
            if (clickEvent.currentTarget is not VisualElement selectedCell
                || selectedCell.userData is not int cellIndex)
            {
                return;
            }

            selectedCellIndex = cellIndex;
            selectedCell.Focus();
            CellSelected?.Invoke(selectedCellIndex);
        }

        /// <summary>
        /// Memo 활성 상태에 따라 숫자 입력을 Cell 또는 Memo Controller에 전달합니다.
        /// </summary>
        /// <param name="clickEvent">숫자 또는 지우기 값을 포함한 클릭 이벤트입니다.</param>
        private void OnValueButtonClicked(ClickEvent clickEvent)
        {
            if (selectedCellIndex < 0
                || clickEvent.currentTarget is not VisualElement valueButton
                || valueButton.userData is not int value)
            {
                return;
            }

            if (memoController.IsMemoActive)
            {
                if (value == SudokuDefine.EmptyCellValue)
                {
                    memoController.ClearMemo(selectedCellIndex);
                    return;
                }

                if (cellController.GetCellValue(selectedCellIndex) == SudokuDefine.EmptyCellValue)
                {
                    memoController.ToggleMemoValue(selectedCellIndex, value);
                }

                return;
            }

            if (cellController.SetCellValue(selectedCellIndex, value)
                && value != SudokuDefine.EmptyCellValue)
            {
                memoController.ClearMemo(selectedCellIndex);
            }
        }

        /// <summary>
        /// Memo 버튼 입력을 Memo Controller의 상태 전환 요청으로 전달합니다.
        /// </summary>
        private void OnMemoButtonClicked()
        {
            memoController.ToggleMemoState();
        }

        /// <summary>
        /// Cell 값이 변경될 때 숫자별 입력 버튼의 활성 상태를 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">값이 변경된 셀 인덱스입니다.</param>
        /// <param name="previousValue">변경 전 셀 값입니다.</param>
        /// <param name="currentValue">변경 후 셀 값입니다.</param>
        private void OnCellValueChanged(int cellIndex, int previousValue, int currentValue)
        {
            RefreshNumberButtonStates();
        }

        /// <summary>
        /// Given과 플레이어 입력을 포함해 9개가 배치된 숫자의 입력 버튼을 비활성화합니다.
        /// </summary>
        private void RefreshNumberButtonStates()
        {
            if (cellController == null)
            {
                return;
            }

            for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                VisualElement valueButton = valueButtons[value];

                if (valueButton != null)
                {
                    bool canEnterValue = cellController.GetCellValueCount(value) < SudokuDefine.BoardSize;
                    valueButton.SetEnabled(canEnterValue);
                }
            }
        }
    }
}
