using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.Component
{
    using Data;

    public sealed class SudokuCellHistoryController : MonoBehaviour
    {
        [Tooltip("값 변경 기록을 저장하고 복구할 Cell Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        [Tooltip("Undo와 Do 버튼을 포함한 플레이어 컨트롤 UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        private readonly Stack<SudokuCellChangeData> undoStack = new();
        private readonly Stack<SudokuCellChangeData> redoStack = new();

        private VisualElement undoButton;
        private VisualElement doButton;
        private bool isApplyingHistory;
        private bool buttonCallbacksRegistered;
        private bool hasStarted;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterButtonCallbacks();
        }

        private void OnEnable()
        {
            if (cellController != null)
            {
                cellController.CellValueChanged += OnCellValueChanged;
            }

            if (hasStarted)
            {
                RegisterButtonCallbacks();
            }
        }

        private void OnDisable()
        {
            if (cellController != null)
            {
                cellController.CellValueChanged -= OnCellValueChanged;
            }

            UnregisterButtonCallbacks();
        }

        #endregion

        /// <summary>
        /// 가장 최근 셀 변경을 이전 값으로 복구하고 Redo Stack에 저장합니다.
        /// </summary>
        /// <returns>복구할 기록이 존재하고 값이 변경되었으면 true입니다.</returns>
        public bool Undo()
        {
            if (cellController == null || undoStack.Count == 0)
            {
                return false;
            }

            SudokuCellChangeData changeData = undoStack.Pop();
            redoStack.Push(changeData);

            if (ApplyHistoryValue(changeData.CellIndex, changeData.PreviousValue))
            {
                return true;
            }

            redoStack.Pop();
            undoStack.Push(changeData);
            return false;
        }

        /// <summary>
        /// 가장 최근 Undo 기록의 새 값을 다시 적용하고 Undo Stack에 저장합니다.
        /// </summary>
        /// <returns>다시 적용할 기록이 존재하고 값이 변경되었으면 true입니다.</returns>
        public bool Redo()
        {
            if (cellController == null || redoStack.Count == 0)
            {
                return false;
            }

            SudokuCellChangeData changeData = redoStack.Pop();
            undoStack.Push(changeData);

            if (ApplyHistoryValue(changeData.CellIndex, changeData.CurrentValue))
            {
                return true;
            }

            undoStack.Pop();
            redoStack.Push(changeData);
            return false;
        }

        /// <summary>
        /// 저장된 Undo와 Redo 기록을 모두 제거합니다.
        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        /// <summary>
        /// 플레이어 컨트롤 UI Document에서 Undo와 Do 버튼을 찾아 이벤트를 등록합니다.
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            if (buttonCallbacksRegistered)
            {
                return;
            }

            if (playerControlDocument == null)
            {
                Debug.LogError("Undo와 Do 버튼을 연결할 플레이어 컨트롤 UI Document가 없습니다.", this);
                return;
            }

            VisualElement root = playerControlDocument.rootVisualElement;
            undoButton = root.Q<VisualElement>("undo-button");
            doButton = root.Q<VisualElement>("do-button");

            if (undoButton == null || doButton == null)
            {
                Debug.LogError("플레이어 컨트롤 UI에서 Undo 또는 Do 버튼을 찾을 수 없습니다.", this);
                return;
            }

            undoButton.RegisterCallback<ClickEvent>(OnUndoButtonClicked);
            doButton.RegisterCallback<ClickEvent>(OnDoButtonClicked);
            buttonCallbacksRegistered = true;
        }

        /// <summary>
        /// Undo와 Do 버튼에 등록한 이벤트를 해제합니다.
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            if (!buttonCallbacksRegistered)
            {
                return;
            }

            undoButton?.UnregisterCallback<ClickEvent>(OnUndoButtonClicked);
            doButton?.UnregisterCallback<ClickEvent>(OnDoButtonClicked);
            buttonCallbacksRegistered = false;
        }

        /// <summary>
        /// Undo 버튼 입력을 받아 가장 최근 셀 변경을 복구합니다.
        /// </summary>
        /// <param name="clickEvent">Undo 버튼의 UI Toolkit 클릭 이벤트입니다.</param>
        private void OnUndoButtonClicked(ClickEvent clickEvent)
        {
            Undo();
        }

        /// <summary>
        /// Do 버튼 입력을 받아 가장 최근 Undo 기록을 다시 적용합니다.
        /// </summary>
        /// <param name="clickEvent">Do 버튼의 UI Toolkit 클릭 이벤트입니다.</param>
        private void OnDoButtonClicked(ClickEvent clickEvent)
        {
            Redo();
        }

        /// <summary>
        /// 일반 셀 입력을 Undo Stack에 저장하고 기존 Redo Stack을 제거합니다.
        /// </summary>
        /// <param name="cellIndex">값이 변경된 셀 인덱스입니다.</param>
        /// <param name="previousValue">변경 전 셀 값입니다.</param>
        /// <param name="currentValue">변경 후 셀 값입니다.</param>
        private void OnCellValueChanged(int cellIndex, int previousValue, int currentValue)
        {
            if (isApplyingHistory)
            {
                return;
            }

            undoStack.Push(new SudokuCellChangeData(cellIndex, previousValue, currentValue));

            if (redoStack.Count > 0)
            {
                redoStack.Clear();
            }
        }

        /// <summary>
        /// Undo 또는 Redo 값을 일반 입력 기록 없이 Cell Controller에 적용합니다.
        /// </summary>
        /// <param name="cellIndex">값을 복구할 셀 인덱스입니다.</param>
        /// <param name="value">복구할 셀 값입니다.</param>
        /// <returns>셀 값이 변경되었으면 true입니다.</returns>
        private bool ApplyHistoryValue(int cellIndex, int value)
        {
            isApplyingHistory = true;

            try
            {
                return cellController.SetCellValue(cellIndex, value);
            }
            finally
            {
                isApplyingHistory = false;
            }
        }
    }
}
