using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.Component
{
    using Define;

    public class SudokuHintController : MonoBehaviour
    {
        private const string HintButtonName = "hint-button";

        [Tooltip("Hint 버튼을 포함한 플레이어 컨트롤 UI Document입니다.")]
        [SerializeField] private UIDocument playerControlDocument;

        [Tooltip("저장된 정답을 사용해 Hint 값을 적용할 Cell Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        private readonly int[] solutionValues = new int[SudokuDefine.CellCount];
        private Button hintButton;
        private bool hasSolution;
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

        #region Hint Input

        /// <summary>
        /// 생성된 Sudoku 정답을 Hint 전용 데이터로 복사해 저장합니다.
        /// </summary>
        /// <param name="solution">모든 Cell이 채워진 81개 정답 값입니다.</param>
        /// <returns>정답이 유효하고 정상적으로 저장되었으면 true입니다.</returns>
        public bool InitializeSolution(IReadOnlyList<int> solution)
        {
            if (!IsValidSolution(solution))
            {
                Debug.LogError("Hint에 저장할 Solution은 1~9로 구성된 81개 값이어야 합니다.", this);
                return false;
            }

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                solutionValues[cellIndex] = solution[cellIndex];
            }

            hasSolution = true;
            return true;
        }

        /// <summary>
        /// Hint 버튼을 찾아 클릭 이벤트를 등록합니다.
        /// </summary>
        private void RegisterCallback()
        {
            if (callbackRegistered || !CacheHintButton())
            {
                return;
            }

            hintButton.clicked += OnHintButtonClicked;
            callbackRegistered = true;
        }

        /// <summary>
        /// Hint 버튼에 등록한 클릭 이벤트를 해제합니다.
        /// </summary>
        private void UnregisterCallback()
        {
            if (!callbackRegistered)
            {
                return;
            }

            hintButton.clicked -= OnHintButtonClicked;
            callbackRegistered = false;
        }

        /// <summary>
        /// 빈 Cell 하나에 저장된 정답을 입력하고 해당 Cell의 Memo를 제거합니다.
        /// </summary>
        private void OnHintButtonClicked()
        {
            TryApplyRandomHint();
        }

        /// <summary>
        /// 현재 값이 0인 Cell 하나를 무작위로 선택해 저장된 정답을 적용합니다.
        /// </summary>
        /// <returns>빈 Cell에 정답이 적용되었으면 true입니다.</returns>
        private bool TryApplyRandomHint()
        {
            if (!hasSolution)
            {
                Debug.LogError("Hint에 사용할 Solution이 아직 전달되지 않았습니다.", this);
                return false;
            }

            int emptyCellCount = 0;
            int hintedCellIndex = -1;

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                if (cellController.GetCellValue(cellIndex) != SudokuDefine.EmptyCellValue)
                {
                    continue;
                }

                emptyCellCount++;

                if (Random.Range(0, emptyCellCount) == 0)
                {
                    hintedCellIndex = cellIndex;
                }
            }

            if (hintedCellIndex >= 0
                && cellController.SetCellValue(hintedCellIndex, solutionValues[hintedCellIndex]))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 플레이어 컨트롤 UI에서 Hint 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>Hint 처리에 필요한 참조와 버튼을 찾았으면 true입니다.</returns>
        private bool CacheHintButton()
        {
            if (playerControlDocument == null || cellController == null)
            {
                Debug.LogError("Sudoku Hint Controller에 UI Document와 Cell Controller를 연결해야 합니다.", this);
                return false;
            }

            hintButton = playerControlDocument.rootVisualElement.Q<Button>(HintButtonName);

            if (hintButton != null)
            {
                return true;
            }

            Debug.LogError($"플레이어 컨트롤 UI에서 {HintButtonName}을 찾을 수 없습니다.", this);
            return false;
        }

        /// <summary>
        /// Solution이 81개의 1~9 값으로 구성되어 있는지 확인합니다.
        /// </summary>
        /// <param name="solution">검사할 Sudoku 정답 값입니다.</param>
        /// <returns>Hint에 사용할 수 있는 정답이면 true입니다.</returns>
        private static bool IsValidSolution(IReadOnlyList<int> solution)
        {
            if (solution == null || solution.Count != SudokuDefine.CellCount)
            {
                return false;
            }

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                if (solution[cellIndex] < SudokuDefine.MinCellValue
                    || solution[cellIndex] > SudokuDefine.MaxCellValue)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
