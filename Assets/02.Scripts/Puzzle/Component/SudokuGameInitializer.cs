using System.Threading.Tasks;
using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using Data;
    using Enum;
    using Manager;
    using SUDOKU.Manager;

    public class SudokuGameInitializer : MonoBehaviour
    {
        [Tooltip("생성된 Sudoku 문제의 Given 값을 적용할 Cell Controller입니다.")]
        [SerializeField] private SudokuCellController cellController;

        [Tooltip("생성된 Sudoku 정답을 전달받아 관리할 Hint Controller입니다.")]
        [SerializeField] private SudokuHintController hintController;

        private bool isInitializing;

        #region Unity Callbacks

        private async void Start()
        {
            await InitializeGameAsync();
        }

        #endregion

        #region Game Initialization

        /// <summary>
        /// GameManager의 난이도로 Sudoku 문제를 비동기 생성하고 Cell에 적용합니다.
        /// </summary>
        private async Task InitializeGameAsync()
        {
            if (isInitializing)
            {
                return;
            }

            if (cellController == null || hintController == null)
            {
                Debug.LogError("문제와 정답을 전달할 Cell Controller와 Hint Controller가 연결되지 않았습니다.", this);
                return;
            }

            SudokuGenerationManager generationManager = SudokuGenerationManager.Instance;

            if (generationManager == null)
            {
                Debug.LogError("Sudoku 문제를 생성할 Sudoku Generation Manager를 찾을 수 없습니다.", this);
                return;
            }

            isInitializing = true;

            try
            {
                ESudokuDifficulty difficulty = ResolveDifficulty();
                SudokuPuzzleData puzzleData = await generationManager.GeneratePuzzleAsync(difficulty);

                if (this == null || cellController == null || hintController == null)
                {
                    return;
                }

                if (puzzleData == null
                    || !cellController.InitializeGivenCells(puzzleData.GetPuzzle())
                    || !hintController.InitializeSolution(puzzleData.GetSolution()))
                {
                    Debug.LogError("생성된 Sudoku 문제 또는 정답을 Controller에 적용하지 못했습니다.", this);
                }
            }
            finally
            {
                if (this != null)
                {
                    isInitializing = false;
                }
            }
        }

        /// <summary>
        /// 현재 Scene에서 GameManager를 찾아 저장된 난이도를 반환하고, 없으면 Normal을 반환합니다.
        /// </summary>
        /// <returns>새 Sudoku 문제 생성에 사용할 난이도입니다.</returns>
        private static ESudokuDifficulty ResolveDifficulty()
        {
            GameManager gameManager = GameManager.Instance;

            return gameManager != null
                ? gameManager.Difficulty
                : ESudokuDifficulty.Normal;
        }

        #endregion
    }
}
