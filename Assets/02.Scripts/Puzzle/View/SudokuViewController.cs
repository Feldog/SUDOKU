using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.View
{
    using Component;
    using Define;

    public class SudokuViewController : MonoBehaviour
    {
        private const string FocusedCellClass = "focused";
        private const string InvalidCellLabelClass = "invalid";
        private const string GivenCellLabelClass = "given";

        [Tooltip("셀 선택과 숫자 표시를 처리할 게임 보드 UI Document입니다.")]
        [SerializeField] private UIDocument gameBoardDocument;

        [Tooltip("셀 데이터를 조회하고 수정할 Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        [Tooltip("선택된 셀 정보를 제공할 Sudoku Controller입니다.")]
        [SerializeField] private SudokuController sudokuController;

        private readonly VisualElement[] cells = new VisualElement[SudokuDefine.CellCount];
        private readonly Label[] cellLabels = new Label[SudokuDefine.CellCount];
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
        /// Cell 데이터와 Sudoku 선택 상태 변경 이벤트를 등록합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheVisualElements()
                || cellController == null || sudokuController == null)
            {
                if (cellController == null || sudokuController == null)
                {
                    Debug.LogError("Sudoku View에 Cell Controller와 Sudoku Controller를 연결해야 합니다.", this);
                }

                return;
            }

            sudokuController.CellSelected += OnCellSelected;
            cellController.CellValueChanged += OnCellValueChanged;
            cellController.CellValidationChanged += OnCellValidationChanged;
            cellController.GivenCellStateChanged += OnGivenCellStateChanged;
            callbacksRegistered = true;
        }

        /// <summary>
        /// 등록한 Cell 데이터와 Sudoku 선택 상태 변경 이벤트를 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            if (cellController != null)
            {
                cellController.CellValueChanged -= OnCellValueChanged;
                cellController.CellValidationChanged -= OnCellValidationChanged;
                cellController.GivenCellStateChanged -= OnGivenCellStateChanged;
            }

            if (sudokuController != null)
            {
                sudokuController.CellSelected -= OnCellSelected;
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
                    ApplyCellValidation(cellIndex, cellController.IsCellValueValid(cellIndex));
                    ApplyGivenCellState(cellIndex, cellController.IsGivenCell(cellIndex));
                }
            }

        }

        /// <summary>
        /// 게임 보드 UI Document에서 셀과 셀 Label을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 UI 요소를 찾았으면 true입니다.</returns>
        private bool CacheVisualElements()
        {
            if (gameBoardDocument == null)
            {
                Debug.LogError("게임 보드 UI Document를 연결해야 합니다.", this);
                return false;
            }

            VisualElement boardRoot = gameBoardDocument.rootVisualElement;

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

            return true;
        }

        /// <summary>
        /// Sudoku Controller가 선택한 셀에 Focus 스타일을 적용합니다.
        /// </summary>
        /// <param name="selectedCellIndex">새로 선택된 셀 인덱스입니다.</param>
        private void OnCellSelected(int selectedCellIndex)
        {
            if (focusedCellIndex >= 0)
            {
                cells[focusedCellIndex].RemoveFromClassList(FocusedCellClass);
            }

            focusedCellIndex = selectedCellIndex;
            cells[focusedCellIndex].AddToClassList(FocusedCellClass);
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
        /// Cell Controller의 검증 상태 변경 이벤트를 받아 셀 글자색을 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">검증 상태가 변경된 셀 인덱스입니다.</param>
        /// <param name="isValid">현재 셀 값이 Sudoku 규칙을 만족하는지 여부입니다.</param>
        private void OnCellValidationChanged(int cellIndex, bool isValid)
        {
            ApplyCellValidation(cellIndex, isValid);
        }

        /// <summary>
        /// Cell Controller의 Given 상태 변경 이벤트를 받아 셀 글꼴을 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">Given 상태가 변경된 셀 인덱스입니다.</param>
        /// <param name="isGiven">플레이어가 수정할 수 없는 Given 셀인지 여부입니다.</param>
        private void OnGivenCellStateChanged(int cellIndex, bool isGiven)
        {
            ApplyGivenCellState(cellIndex, isGiven);
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

        /// <summary>
        /// 셀 검증 결과에 따라 Label의 잘못된 숫자 표시 클래스를 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">글자색을 갱신할 셀 인덱스입니다.</param>
        /// <param name="isValid">셀 값이 유효한지 여부입니다.</param>
        private void ApplyCellValidation(int cellIndex, bool isValid)
        {
            if (isValid)
            {
                cellLabels[cellIndex].RemoveFromClassList(InvalidCellLabelClass);
                return;
            }

            cellLabels[cellIndex].AddToClassList(InvalidCellLabelClass);
        }

        /// <summary>
        /// Given 상태에 따라 셀 Label의 굵은 글꼴 클래스를 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">글꼴을 갱신할 셀 인덱스입니다.</param>
        /// <param name="isGiven">Given 셀인지 여부입니다.</param>
        private void ApplyGivenCellState(int cellIndex, bool isGiven)
        {
            if (isGiven)
            {
                cellLabels[cellIndex].AddToClassList(GivenCellLabelClass);
                return;
            }

            cellLabels[cellIndex].RemoveFromClassList(GivenCellLabelClass);
        }
    }
}
