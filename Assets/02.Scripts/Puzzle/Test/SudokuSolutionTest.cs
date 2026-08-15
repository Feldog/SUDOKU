using UnityEngine;

namespace SUDOKU.Puzzle.Test
{
    using Component;
    using Generator;

    public sealed class SudokuSolutionTest : MonoBehaviour
    {
        [Tooltip("생성된 스도쿠 정답을 적용할 Cell Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        #region Unity Callbacks

        private void Start()
        {
            GenerateAndApplySolution();
        }

        #endregion

        /// <summary>
        /// 완성된 스도쿠 정답을 새로 생성하고 모든 Cell에 적용합니다.
        /// </summary>
        [ContextMenu("정답 생성 및 적용")]
        public void GenerateAndApplySolution()
        {
            if (cellController == null)
            {
                Debug.LogError("정답을 적용할 Sudoku Cell Controller가 연결되지 않았습니다.", this);
                return;
            }

            int[] solution = SudokuSolutionGenerator.CreateSolution();

            for (int cellIndex = 0; cellIndex < solution.Length; cellIndex++)
            {
                cellController.SetCellValue(cellIndex, solution[cellIndex]);
            }
        }
    }
}
