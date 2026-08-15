using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.Component
{
    using Define;

    public sealed class SudokuHelpController : MonoBehaviour
    {
        private const string HelpHighlightedClass = "help-highlighted";
        private const string SameValueHighlightedClass = "same-value-highlighted";

        [Tooltip("선택한 셀과 같은 행, 열, Region을 강조할지 여부입니다.")]
        [SerializeField] private bool isHelpEnabled = true;

        [Tooltip("선택한 셀과 같은 숫자를 가진 다른 셀을 강조할지 여부입니다.")]
        [SerializeField] private bool isSameValueHelpEnabled = true;

        [Tooltip("도움 강조 표시를 적용할 게임 보드 UI Document입니다.")]
        [SerializeField] private UIDocument gameBoardDocument;

        [Tooltip("선택한 셀의 Region 데이터를 제공할 Controller입니다.")]
        [SerializeField] private SudokuRegionController regionController;

        [Tooltip("선택 셀의 값과 숫자별 셀 인덱스를 제공할 Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        private readonly VisualElement[] cells = new VisualElement[SudokuDefine.CellCount];

        private int focusedCellIndex = -1;
        private bool callbacksRegistered;
        private bool hasStarted;

        public bool IsHelpEnabled => isHelpEnabled;
        public bool IsSameValueHelpEnabled => isSameValueHelpEnabled;

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
            ClearHighlights();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                RefreshHighlights();
            }
        }

        #endregion

        /// <summary>
        /// 도움 강조 기능을 활성화하거나 비활성화합니다.
        /// </summary>
        /// <param name="isEnabled">도움 강조 기능을 사용할지 여부입니다.</param>
        public void SetHelpEnabled(bool isEnabled)
        {
            isHelpEnabled = isEnabled;

            if (!isHelpEnabled)
            {
                RefreshHighlights();
            }
        }

        /// <summary>
        /// 선택 셀과 동일한 숫자의 강조 기능을 활성화하거나 비활성화합니다.
        /// </summary>
        /// <param name="isEnabled">동일 숫자 강조 기능을 사용할지 여부입니다.</param>
        public void SetSameValueHelpEnabled(bool isEnabled)
        {
            isSameValueHelpEnabled = isEnabled;
            RefreshHighlights();
        }

        /// <summary>
        /// 게임 보드 셀의 클릭 이벤트를 등록합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheCells())
            {
                return;
            }

            if (cellController == null)
            {
                Debug.LogError("동일 숫자 도움 기능에 사용할 Cell Controller가 연결되지 않았습니다.", this);
                return;
            }

            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                cells[cellIndex].userData = cellIndex;
                cells[cellIndex].RegisterCallback<ClickEvent>(OnCellClicked);
            }

            cellController.CellValueChanged += OnCellValueChanged;
            callbacksRegistered = true;
        }

        /// <summary>
        /// 게임 보드 셀에 등록한 클릭 이벤트를 해제합니다.
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

            if (cellController != null)
            {
                cellController.CellValueChanged -= OnCellValueChanged;
            }

            callbacksRegistered = false;
        }

        /// <summary>
        /// UI Document에서 81개 게임 보드 셀을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>모든 셀을 찾았으면 true입니다.</returns>
        private bool CacheCells()
        {
            if (gameBoardDocument == null)
            {
                Debug.LogError("도움 기능에 사용할 게임 보드 UI Document가 연결되지 않았습니다.", this);
                return false;
            }

            VisualElement root = gameBoardDocument.rootVisualElement;

            for (int row = 0; row < SudokuDefine.BoardSize; row++)
            {
                for (int column = 0; column < SudokuDefine.BoardSize; column++)
                {
                    int cellIndex = row * SudokuDefine.BoardSize + column;
                    cells[cellIndex] = root.Q<VisualElement>($"cell-{row}-{column}");

                    if (cells[cellIndex] == null)
                    {
                        Debug.LogError($"도움 기능에서 cell-{row}-{column} 요소를 찾을 수 없습니다.", this);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 선택한 셀과 같은 행, 열, Region에 도움 강조 표시를 적용합니다.
        /// </summary>
        /// <param name="clickEvent">선택된 셀 정보를 포함한 UI Toolkit 이벤트입니다.</param>
        private void OnCellClicked(ClickEvent clickEvent)
        {
            if (clickEvent.currentTarget is not VisualElement selectedCell
                || selectedCell.userData is not int selectedCellIndex)
            {
                return;
            }

            focusedCellIndex = selectedCellIndex;
            RefreshHighlights();
        }

        /// <summary>
        /// 셀 값이 변경될 때 현재 Focus 셀을 기준으로 도움 강조를 다시 계산합니다.
        /// </summary>
        /// <param name="cellIndex">값이 변경된 셀 인덱스입니다.</param>
        /// <param name="previousValue">변경 전 셀 값입니다.</param>
        /// <param name="currentValue">변경 후 셀 값입니다.</param>
        private void OnCellValueChanged(int cellIndex, int previousValue, int currentValue)
        {
            RefreshHighlights();
        }

        /// <summary>
        /// 현재 Focus 셀을 기준으로 행, 열, Region과 동일 숫자 강조를 갱신합니다.
        /// </summary>
        private void RefreshHighlights()
        {
            ClearHighlights();

            if (focusedCellIndex < 0)
            {
                return;
            }

            int selectedRow = focusedCellIndex / SudokuDefine.BoardSize;
            int selectedColumn = focusedCellIndex % SudokuDefine.BoardSize;

            if (isHelpEnabled && regionController != null && regionController.RegionData != null)
            {
                int selectedRegionId = regionController.RegionData.GetRegionId(focusedCellIndex);

                for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                {
                    if (cellIndex == focusedCellIndex)
                    {
                        continue;
                    }

                    int row = cellIndex / SudokuDefine.BoardSize;
                    int column = cellIndex % SudokuDefine.BoardSize;
                    bool isSameRow = row == selectedRow;
                    bool isSameColumn = column == selectedColumn;
                    bool isSameRegion = regionController.RegionData.GetRegionId(cellIndex) == selectedRegionId;

                    if (isSameRow || isSameColumn || isSameRegion)
                    {
                        cells[cellIndex].AddToClassList(HelpHighlightedClass);
                    }
                }
            }

            if (!isSameValueHelpEnabled || cellController == null)
            {
                return;
            }

            int focusedCellValue = cellController.GetCellValue(focusedCellIndex);

            if (focusedCellValue == SudokuDefine.EmptyCellValue)
            {
                return;
            }

            foreach (int cellIndex in cellController.GetCellIndicesByValue(focusedCellValue))
            {
                if (cellIndex != focusedCellIndex)
                {
                    cells[cellIndex].AddToClassList(SameValueHighlightedClass);
                }
            }
        }

        /// <summary>
        /// 모든 셀에서 도움 강조 표시를 제거합니다.
        /// </summary>
        private void ClearHighlights()
        {
            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                cells[cellIndex]?.RemoveFromClassList(HelpHighlightedClass);
                cells[cellIndex]?.RemoveFromClassList(SameValueHighlightedClass);
            }
        }
    }
}
