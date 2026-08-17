using System;

namespace SUDOKU.Puzzle.Generator
{
    using Data;
    using Define;
    using Enum;
    using Logic;

    public class SudokuPuzzleGenerator
    {
        private readonly Random random = new(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// 요청한 난이도의 목표 단서 수까지 유일해를 유지하며 문제를 생성합니다.
        /// </summary>
        /// <param name="difficulty">생성할 스도쿠 난이도입니다.</param>
        /// <returns>문제, 정답, 난이도를 포함한 생성 결과입니다.</returns>
        public SudokuPuzzleData Generate(ESudokuDifficulty difficulty)
        {
            SudokuPuzzleData closestPuzzle = null;
            int closestDifficultyDistance = int.MaxValue;

            for (int attempt = 0; attempt < SudokuDifficultyDefine.MaxGenerationAttempts; attempt++)
            {
                SudokuPuzzleData candidatePuzzle = GenerateCandidate(difficulty);

                if (candidatePuzzle.EvaluatedDifficulty == difficulty)
                {
                    return candidatePuzzle;
                }

                int difficultyDistance = GetDifficultyDistance(
                    difficulty,
                    candidatePuzzle.EvaluatedDifficulty);

                if (difficultyDistance < closestDifficultyDistance)
                {
                    closestPuzzle = candidatePuzzle;
                    closestDifficultyDistance = difficultyDistance;
                }
            }

            return closestPuzzle;
        }

        /// <summary>
        /// 완성 Solution 하나에서 유일해를 유지한 문제 후보를 만들고 사람식 난이도를 평가합니다.
        /// </summary>
        /// <param name="difficulty">생성 과정의 목표 난이도입니다.</param>
        /// <returns>사람식 난이도 평가가 포함된 문제 후보입니다.</returns>
        private SudokuPuzzleData GenerateCandidate(ESudokuDifficulty difficulty)
        {
            int[] solution = SudokuGameLogic.CreateSolution();
            int[] puzzle = (int[])solution.Clone();
            int[] regionMap = SudokuDefine.GetDefaultRegionMap();
            int targetClueCount = GetTargetClueCount(difficulty);
            int currentClueCount = SudokuDefine.CellCount;
            int[] removalOrder = CreateShuffledCellOrder();

            for (int orderIndex = 0;
                 orderIndex < removalOrder.Length && currentClueCount > targetClueCount;
                 orderIndex++)
            {
                int cellIndex = removalOrder[orderIndex];
                int previousValue = puzzle[cellIndex];
                puzzle[cellIndex] = SudokuDefine.EmptyCellValue;

                if (SudokuGameLogic.CountSolutions(puzzle, regionMap) == 1)
                {
                    currentClueCount--;
                    continue;
                }

                puzzle[cellIndex] = previousValue;
            }

            SudokuDifficultyResult difficultyResult =
                SudokuGameLogic.Evaluate(puzzle, regionMap);

            return new SudokuPuzzleData(puzzle, solution, difficulty, difficultyResult);
        }

        /// <summary>
        /// 요청 난이도와 평가 난이도의 단계 차이를 계산합니다.
        /// </summary>
        /// <param name="requestedDifficulty">생성을 요청한 난이도입니다.</param>
        /// <param name="evaluatedDifficulty">사람식 Solver가 평가한 난이도입니다.</param>
        /// <returns>두 난이도 사이의 단계 차이입니다.</returns>
        private static int GetDifficultyDistance(
            ESudokuDifficulty requestedDifficulty,
            ESudokuDifficulty evaluatedDifficulty)
        {
            return Math.Abs((int)requestedDifficulty - (int)evaluatedDifficulty);
        }

        /// <summary>
        /// 난이도별 초기 생성 목표 단서 수를 반환합니다.
        /// </summary>
        /// <param name="difficulty">목표 단서 수를 조회할 난이도입니다.</param>
        /// <returns>유일해를 유지하며 도달을 시도할 단서 수입니다.</returns>
        private static int GetTargetClueCount(ESudokuDifficulty difficulty)
        {
            return difficulty switch
            {
                ESudokuDifficulty.Easy => SudokuDifficultyDefine.EasyTargetClueCount,
                ESudokuDifficulty.Normal => SudokuDifficultyDefine.NormalTargetClueCount,
                ESudokuDifficulty.Hard => SudokuDifficultyDefine.HardTargetClueCount,
                ESudokuDifficulty.Extreme => SudokuDifficultyDefine.ExtremeTargetClueCount,
                _ => SudokuDifficultyDefine.NormalTargetClueCount
            };
        }

        /// <summary>
        /// 전체 셀 인덱스를 무작위 삭제 순서로 생성합니다.
        /// </summary>
        /// <returns>0부터 CellCount 직전까지 무작위로 섞인 배열입니다.</returns>
        private int[] CreateShuffledCellOrder()
        {
            int[] order = new int[SudokuDefine.CellCount];

            for (int cellIndex = 0; cellIndex < order.Length; cellIndex++)
            {
                order[cellIndex] = cellIndex;
            }

            for (int index = order.Length - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                int temporaryValue = order[index];
                order[index] = order[swapIndex];
                order[swapIndex] = temporaryValue;
            }

            return order;
        }
    }
}
