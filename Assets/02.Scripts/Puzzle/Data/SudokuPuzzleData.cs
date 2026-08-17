namespace SUDOKU.Puzzle.Data
{
    using Define;
    using Enum;

    public class SudokuPuzzleData
    {
        private readonly int[] puzzle;
        private readonly int[] solution;

        public ESudokuDifficulty Difficulty { get; }
        public ESudokuDifficulty EvaluatedDifficulty { get; }
        public ESudokuSolveTechnique HardestTechnique { get; }
        public int DifficultyScore { get; }
        public bool IsSolvedLogically { get; }
        public int ClueCount { get; }

        /// <summary>
        /// 생성된 스도쿠 문제와 정답 데이터를 보관합니다.
        /// </summary>
        /// <param name="puzzle">빈 셀이 0으로 저장된 문제 배열입니다.</param>
        /// <param name="solution">모든 셀이 채워진 정답 배열입니다.</param>
        /// <param name="difficulty">요청된 스도쿠 난이도입니다.</param>
        /// <param name="difficultyResult">사람식 풀이 과정으로 계산한 난이도 결과입니다.</param>
        public SudokuPuzzleData(
            int[] puzzle,
            int[] solution,
            ESudokuDifficulty difficulty,
            SudokuDifficultyResult difficultyResult)
        {
            this.puzzle = (int[])puzzle.Clone();
            this.solution = (int[])solution.Clone();
            Difficulty = difficulty;
            EvaluatedDifficulty = difficultyResult.Difficulty;
            HardestTechnique = difficultyResult.HardestTechnique;
            DifficultyScore = difficultyResult.Score;
            IsSolvedLogically = difficultyResult.IsSolvedLogically;
            ClueCount = CountClues(this.puzzle);
        }

        /// <summary>
        /// 외부 수정으로부터 보호된 문제 배열의 복사본을 반환합니다.
        /// </summary>
        /// <returns>빈 셀이 0으로 저장된 문제 배열입니다.</returns>
        public int[] GetPuzzle()
        {
            return (int[])puzzle.Clone();
        }

        /// <summary>
        /// 외부 수정으로부터 보호된 정답 배열의 복사본을 반환합니다.
        /// </summary>
        /// <returns>모든 셀이 채워진 정답 배열입니다.</returns>
        public int[] GetSolution()
        {
            return (int[])solution.Clone();
        }

        /// <summary>
        /// 문제 배열에 남아 있는 단서 수를 계산합니다.
        /// </summary>
        /// <param name="board">단서 수를 계산할 스도쿠 배열입니다.</param>
        /// <returns>0이 아닌 셀의 개수입니다.</returns>
        private static int CountClues(int[] board)
        {
            int clueCount = 0;

            for (int cellIndex = 0; cellIndex < board.Length; cellIndex++)
            {
                if (board[cellIndex] != SudokuDefine.EmptyCellValue)
                {
                    clueCount++;
                }
            }

            return clueCount;
        }
    }
}
