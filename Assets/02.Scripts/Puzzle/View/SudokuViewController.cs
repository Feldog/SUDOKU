using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.View
{
    using Component;
    using Define;

    public sealed class SudokuViewController : MonoBehaviour
    {
        private const string FocusedCellClass = "focused";

        [Tooltip("셀 선택과 숫자 표시를 처리할 게임 보드 UI Document입니다.")]
        [SerializeField] private UIDocument gameBoardDocument;

        [Tooltip("숫자 및 지우기 버튼을 포함한 플레이어 컨트롤 UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        [Tooltip("셀 데이터를 조회하고 수정할 Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        private readonly VisualElement[] cells = new VisualElement[SudokuDefine.CellCount];
        private readonly Label[] cellLabels = new Label[SudokuDefine.CellCount];
        private readonly VisualElement[] valueButtons = new VisualElement[SudokuDefine.MaxCellValue + 1];

        private int focusedCellIndex = -1;
        private bool callbacksRegistered;
        private bool hasStarted;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            RefreshCellView();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
                RefreshCellView();
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
            if (callbacksRegistered || !CacheVisualElements() || cellController == null)
            {
                if (cellController == null)
                {
                    Debug.LogError("Sudoku Cell Controller가 연결되지 않았습니다.", this);
                }

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

            cellController.CellValueChanged += OnCellValueChanged;
            callbacksRegistered = true;
        }

        /// <summary>
        /// 등록한 UI 이벤트와 셀 데이터 변경 이벤트를 해제합니다.
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

            if (cellController != null)
            {
                cellController.CellValueChanged -= OnCellValueChanged;
            }

            callbacksRegistered = false;
        }

        /// <summary>
        /// 현재 Cell 데이터를 게임 보드 View에 반영합니다.
        /// </summary>
        private void RefreshCellView()
        {
            if (!CacheVisualElements())
            {
                return;
            }

            if (cellController != null)
            {
                for (int cellIndex = 0; cellIndex < cellLabels.Length; cellIndex++)
                {
                    ApplyCellValue(cellIndex, cellController.GetCellValue(cellIndex));
                }
            }

        }

        /// <summary>
        /// 두 UI Document에서 셀, 셀 Label, 숫자 버튼과 지우기 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 UI 요소를 찾았으면 true입니다.</returns>
        private bool CacheVisualElements()
        {
            if (gameBoardDocument == null || playerControlDocument == null)
            {
                Debug.LogError("게임 보드와 플레이어 컨트롤 UI Document를 모두 연결해야 합니다.", this);
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
                    cellLabels[cellIndex] = boardRoot.Q<Label>($"cell-label-{row}-{column}");

                    if (cells[cellIndex] == null || cellLabels[cellIndex] == null)
                    {
                        Debug.LogError($"게임 보드에서 cell-{row}-{column} 또는 Label을 찾을 수 없습니다.", this);
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
                    Debug.LogError($"플레이어 컨트롤 UI에서 값 {value}에 대응하는 버튼을 찾을 수 없습니다.", this);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 클릭한 셀을 현재 입력 대상으로 선택하고 Focus 스타일을 적용합니다.
        /// </summary>
        /// <param name="clickEvent">클릭된 셀 정보를 포함한 UI Toolkit 이벤트입니다.</param>
        private void OnCellClicked(ClickEvent clickEvent)
        {
            if (clickEvent.currentTarget is not VisualElement selectedCell
                || selectedCell.userData is not int selectedCellIndex)
            {
                return;
            }

            if (focusedCellIndex >= 0)
            {
                cells[focusedCellIndex].RemoveFromClassList(FocusedCellClass);
            }

            focusedCellIndex = selectedCellIndex;
            selectedCell.AddToClassList(FocusedCellClass);
            selectedCell.Focus();
        }

        /// <summary>
        /// 클릭한 숫자 또는 지우기 버튼의 값을 Cell Controller에 전달합니다.
        /// </summary>
        /// <param name="clickEvent">클릭된 입력 버튼 정보를 포함한 UI Toolkit 이벤트입니다.</param>
        private void OnValueButtonClicked(ClickEvent clickEvent)
        {
            if (focusedCellIndex < 0
                || clickEvent.currentTarget is not VisualElement valueButton
                || valueButton.userData is not int value)
            {
                return;
            }

            cellController.SetCellValue(focusedCellIndex, value);
        }

        /// <summary>
        /// Cell Controller의 값 변경 이벤트를 받아 셀 Label을 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">변경된 셀 인덱스입니다.</param>
        /// <param name="value">변경된 셀 값입니다.</param>
        private void OnCellValueChanged(int cellIndex, int previousValue, int currentValue)
        {
            ApplyCellValue(cellIndex, currentValue);
        }

        /// <summary>
        /// 지정한 셀 값을 게임 보드 Label에 표시합니다.
        /// </summary>
        /// <param name="cellIndex">표시를 갱신할 셀 인덱스입니다.</param>
        /// <param name="value">표시할 셀 값이며 0은 빈 문자열로 표시합니다.</param>
        private void ApplyCellValue(int cellIndex, int value)
        {
            cellLabels[cellIndex].text = value == SudokuDefine.EmptyCellValue
                ? string.Empty
                : value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
