using System.Threading.Tasks;
using UnityEngine;

namespace SUDOKU.Puzzle.Test
{
    using Data;
    using Enum;
    using Manager;
    using Component;

    public class SudokuSolutionTest : MonoBehaviour
    {
        [Tooltip("테스트에서 생성할 스도쿠 문제의 난이도입니다.")]
        [SerializeField] private ESudokuDifficulty difficulty = ESudokuDifficulty.Normal;

        [Tooltip("생성된 스도쿠 문제와 Given 데이터를 관리할 Cell Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        #region Unity Callbacks

        private async void Start()
        {
            await GenerateAndApplyPuzzleAsync();
        }

        #endregion

        /// <summary>
        /// Inspector에서 선택한 난이도의 문제를 비동기로 생성하고 모든 Cell에 적용합니다.
        /// </summary>
        private async Task GenerateAndApplyPuzzleAsync()
        {
            if (cellController == null)
            {
                Debug.LogError("문제를 적용할 Sudoku Cell Controller가 연결되지 않았습니다.", this);
                return;
            }

            SudokuGenerationManager generationManager = SudokuGenerationManager.Instance;

            if (generationManager == null)
            {
                Debug.LogError("씬에서 Sudoku Generation Manager를 찾을 수 없습니다.", this);
                return;
            }

            SudokuPuzzleData puzzleData = await generationManager.GeneratePuzzleAsync(difficulty);

            if (cellController == null)
            {
                return;
            }

            int[] puzzle = puzzleData.GetPuzzle();
            cellController.InitializeGivenCells(puzzle);
        }
    }
}
