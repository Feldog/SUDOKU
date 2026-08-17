namespace SUDOKU.Puzzle.Data
{
    using Enum;

    public class SudokuDifficultyResult
    {
        public ESudokuDifficulty Difficulty { get; }
        public ESudokuSolveTechnique HardestTechnique { get; }
        public int Score { get; }
        public bool IsSolvedLogically { get; }

        /// <summary>
        /// 사람식 풀이 과정에서 계산된 난이도 평가 결과를 생성합니다.
        /// </summary>
        /// <param name="difficulty">풀이 결과로 판정된 난이도입니다.</param>
        /// <param name="hardestTechnique">풀이 중 사용된 가장 어려운 기법입니다.</param>
        /// <param name="score">사용한 기법과 횟수로 계산한 누적 점수입니다.</param>
        /// <param name="isSolvedLogically">추측 없이 지원 기법만으로 완성했는지 여부입니다.</param>
        public SudokuDifficultyResult(
            ESudokuDifficulty difficulty,
            ESudokuSolveTechnique hardestTechnique,
            int score,
            bool isSolvedLogically)
        {
            Difficulty = difficulty;
            HardestTechnique = hardestTechnique;
            Score = score;
            IsSolvedLogically = isSolvedLogically;
        }
    }
}
