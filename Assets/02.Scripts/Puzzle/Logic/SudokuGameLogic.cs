using System;
using System.Collections.Generic;

namespace SUDOKU.Puzzle.Logic
{
    using Data;
    using Define;
    using Enum;

    public static class SudokuGameLogic
    {
        #region Solution Generation

        private static readonly Random random = new Random();
        private static readonly object randomLock = new object();

        /// <summary>
        /// 모든 행, 열, 3×3 Region이 스도쿠 규칙을 만족하는 완성된 정답을 생성합니다.
        /// </summary>
        /// <returns>행 우선 순서로 저장된 81개 셀의 정답 배열입니다.</returns>
        public static int[] CreateSolution()
        {
            int[] rowOrder = CreateGroupedOrder();
            int[] columnOrder = CreateGroupedOrder();
            int[] numberOrder = CreateShuffledSequence(SudokuDefine.BoardSize);
            int[] solution = new int[SudokuDefine.CellCount];

            for (int row = 0; row < SudokuDefine.BoardSize; row++)
            {
                for (int column = 0; column < SudokuDefine.BoardSize; column++)
                {
                    int patternIndex = GetPatternIndex(rowOrder[row], columnOrder[column]);
                    solution[row * SudokuDefine.BoardSize + column] =
                        numberOrder[patternIndex] + SudokuDefine.MinCellValue;
                }
            }

            return solution;
        }

        /// <summary>
        /// 3개 그룹과 각 그룹 내부 순서를 무작위로 섞은 행 또는 열 순서를 생성합니다.
        /// </summary>
        /// <returns>3×3 Region 구조를 유지하는 행 또는 열 인덱스 배열입니다.</returns>
        private static int[] CreateGroupedOrder()
        {
            int[] groupOrder = CreateShuffledSequence(SudokuDefine.RegionSize);
            int[] groupedOrder = new int[SudokuDefine.BoardSize];
            int writeIndex = 0;

            for (int groupIndex = 0; groupIndex < groupOrder.Length; groupIndex++)
            {
                int[] innerOrder = CreateShuffledSequence(SudokuDefine.RegionSize);

                for (int innerIndex = 0; innerIndex < innerOrder.Length; innerIndex++)
                {
                    groupedOrder[writeIndex++] =
                        groupOrder[groupIndex] * SudokuDefine.RegionSize + innerOrder[innerIndex];
                }
            }

            return groupedOrder;
        }

        /// <summary>
        /// 0부터 지정한 개수 직전까지의 정수를 무작위 순서로 생성합니다.
        /// </summary>
        /// <param name="count">생성할 연속 정수의 개수입니다.</param>
        /// <returns>무작위로 섞인 연속 정수 배열입니다.</returns>
        private static int[] CreateShuffledSequence(int count)
        {
            int[] sequence = new int[count];

            for (int index = 0; index < count; index++)
            {
                sequence[index] = index;
            }

            lock (randomLock)
            {
                for (int index = sequence.Length - 1; index > 0; index--)
                {
                    int swapIndex = random.Next(index + 1);
                    int temporaryValue = sequence[index];
                    sequence[index] = sequence[swapIndex];
                    sequence[swapIndex] = temporaryValue;
                }
            }

            return sequence;
        }

        /// <summary>
        /// 기본 스도쿠 패턴에서 지정한 행과 열에 대응하는 숫자 인덱스를 계산합니다.
        /// </summary>
        /// <param name="row">패턴에 사용할 행 인덱스입니다.</param>
        /// <param name="column">패턴에 사용할 열 인덱스입니다.</param>
        /// <returns>숫자 순서 배열에서 사용할 인덱스입니다.</returns>
        private static int GetPatternIndex(int row, int column)
        {
            int rowOffset = row * SudokuDefine.RegionSize + row / SudokuDefine.RegionSize;
            return (rowOffset + column) % SudokuDefine.BoardSize;
        }

        #endregion

        #region Cell Validation

        /// <summary>
        /// 지정한 숫자를 셀에 입력했을 때 행, 열, Region 규칙을 만족하는지 확인합니다.
        /// </summary>
        /// <param name="cellValues">행 우선 방식으로 정렬된 현재 셀 값 목록입니다.</param>
        /// <param name="regionMap">각 셀이 속한 Region ID 목록입니다.</param>
        /// <param name="cellIndex">숫자를 입력할 셀 인덱스입니다.</param>
        /// <param name="value">입력 가능 여부를 검사할 0~9 값입니다.</param>
        /// <returns>같은 행, 열, Region에 중복 숫자가 없으면 true입니다.</returns>
        public static bool CanPlaceValue(
            IReadOnlyList<int> cellValues,
            IReadOnlyList<int> regionMap,
            int cellIndex,
            int value)
        {
            if (!IsValidInput(cellValues, regionMap, cellIndex, value))
            {
                return false;
            }

            if (value == SudokuDefine.EmptyCellValue)
            {
                return true;
            }

            int targetRow = cellIndex / SudokuDefine.BoardSize;
            int targetColumn = cellIndex % SudokuDefine.BoardSize;
            int targetRegionId = regionMap[cellIndex];

            for (int compareIndex = 0; compareIndex < SudokuDefine.CellCount; compareIndex++)
            {
                if (compareIndex == cellIndex || cellValues[compareIndex] != value)
                {
                    continue;
                }

                int compareRow = compareIndex / SudokuDefine.BoardSize;
                int compareColumn = compareIndex % SudokuDefine.BoardSize;
                bool isSameRow = compareRow == targetRow;
                bool isSameColumn = compareColumn == targetColumn;
                bool isSameRegion = regionMap[compareIndex] == targetRegionId;

                if (isSameRow || isSameColumn || isSameRegion)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 현재 셀에 등록된 숫자가 행, 열, Region 규칙을 만족하는지 확인합니다.
        /// </summary>
        /// <param name="cellValues">행 우선 방식으로 정렬된 현재 셀 값 목록입니다.</param>
        /// <param name="regionMap">각 셀이 속한 Region ID 목록입니다.</param>
        /// <param name="cellIndex">현재 값을 검사할 셀 인덱스입니다.</param>
        /// <returns>등록된 숫자가 유효하거나 빈 셀이면 true입니다.</returns>
        public static bool IsCellValueValid(
            IReadOnlyList<int> cellValues,
            IReadOnlyList<int> regionMap,
            int cellIndex)
        {
            if (cellValues == null
                || cellIndex < 0
                || cellIndex >= SudokuDefine.CellCount)
            {
                return false;
            }

            return CanPlaceValue(
                cellValues,
                regionMap,
                cellIndex,
                cellValues[cellIndex]);
        }

        /// <summary>
        /// 숫자 배치 검사에 필요한 배열 길이, 셀 인덱스, 값과 Region ID를 검증합니다.
        /// </summary>
        /// <param name="cellValues">검사할 현재 셀 값 목록입니다.</param>
        /// <param name="regionMap">검사할 셀별 Region ID 목록입니다.</param>
        /// <param name="cellIndex">검사 대상 셀 인덱스입니다.</param>
        /// <param name="value">검사 대상 셀 값입니다.</param>
        /// <returns>입력 데이터가 검사 가능한 상태이면 true입니다.</returns>
        private static bool IsValidInput(
            IReadOnlyList<int> cellValues,
            IReadOnlyList<int> regionMap,
            int cellIndex,
            int value)
        {
            if (cellValues == null
                || regionMap == null
                || cellValues.Count != SudokuDefine.CellCount
                || regionMap.Count != SudokuDefine.CellCount
                || cellIndex < 0
                || cellIndex >= SudokuDefine.CellCount
                || value < SudokuDefine.EmptyCellValue
                || value > SudokuDefine.MaxCellValue)
            {
                return false;
            }

            for (int regionIndex = 0; regionIndex < regionMap.Count; regionIndex++)
            {
                int regionId = regionMap[regionIndex];

                if (regionId < 0 || regionId >= SudokuDefine.RegionCount)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Solution Counting

        private const int DefaultSolutionLimit = 2;
        private const int SolverAllNumberMask = (1 << (SudokuDefine.MaxCellValue + 1)) - 2;

        /// <summary>
        /// 주어진 문제의 해답 개수를 지정한 제한까지 계산합니다.
        /// </summary>
        /// <param name="puzzle">빈 셀이 0으로 저장된 문제 배열입니다.</param>
        /// <param name="regionMap">각 셀이 속한 Region ID 목록입니다.</param>
        /// <param name="solutionLimit">탐색을 중단할 해답 개수입니다.</param>
        /// <returns>제한 범위 안에서 발견한 해답 개수입니다.</returns>
        public static int CountSolutions(
            IReadOnlyList<int> puzzle,
            IReadOnlyList<int> regionMap,
            int solutionLimit = DefaultSolutionLimit)
        {
            if (puzzle == null
                || regionMap == null
                || puzzle.Count != SudokuDefine.CellCount
                || regionMap.Count != SudokuDefine.CellCount
                || solutionLimit <= 0)
            {
                return 0;
            }

            int[] board = new int[SudokuDefine.CellCount];
            int[] rowMasks = new int[SudokuDefine.BoardSize];
            int[] columnMasks = new int[SudokuDefine.BoardSize];
            int[] regionMasks = new int[SudokuDefine.RegionCount];

            if (!TryInitializeBoard(puzzle, regionMap, board, rowMasks, columnMasks, regionMasks))
            {
                return 0;
            }

            return CountRecursive(
                board,
                regionMap,
                rowMasks,
                columnMasks,
                regionMasks,
                solutionLimit);
        }

        /// <summary>
        /// 문제 데이터를 작업 배열과 행, 열, Region 사용 숫자 Mask로 변환합니다.
        /// </summary>
        /// <param name="puzzle">원본 문제 배열입니다.</param>
        /// <param name="regionMap">셀별 Region ID 목록입니다.</param>
        /// <param name="board">재귀 탐색에 사용할 작업 배열입니다.</param>
        /// <param name="rowMasks">행별 사용 숫자 Mask입니다.</param>
        /// <param name="columnMasks">열별 사용 숫자 Mask입니다.</param>
        /// <param name="regionMasks">Region별 사용 숫자 Mask입니다.</param>
        /// <returns>초기 문제가 스도쿠 규칙에 맞으면 true입니다.</returns>
        private static bool TryInitializeBoard(
            IReadOnlyList<int> puzzle,
            IReadOnlyList<int> regionMap,
            int[] board,
            int[] rowMasks,
            int[] columnMasks,
            int[] regionMasks)
        {
            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                int value = puzzle[cellIndex];
                int regionId = regionMap[cellIndex];

                if (regionId < 0 || regionId >= SudokuDefine.RegionCount)
                {
                    return false;
                }

                board[cellIndex] = value;

                if (value == SudokuDefine.EmptyCellValue)
                {
                    continue;
                }

                if (value < SudokuDefine.MinCellValue || value > SudokuDefine.MaxCellValue)
                {
                    return false;
                }

                int row = cellIndex / SudokuDefine.BoardSize;
                int column = cellIndex % SudokuDefine.BoardSize;
                int valueMask = 1 << value;

                if ((rowMasks[row] & valueMask) != 0
                    || (columnMasks[column] & valueMask) != 0
                    || (regionMasks[regionId] & valueMask) != 0)
                {
                    return false;
                }

                rowMasks[row] |= valueMask;
                columnMasks[column] |= valueMask;
                regionMasks[regionId] |= valueMask;
            }

            return true;
        }

        /// <summary>
        /// 후보 수가 가장 적은 빈 셀부터 선택해 해답 개수를 재귀적으로 계산합니다.
        /// </summary>
        /// <param name="board">현재 탐색 중인 작업 배열입니다.</param>
        /// <param name="regionMap">셀별 Region ID 목록입니다.</param>
        /// <param name="rowMasks">행별 사용 숫자 Mask입니다.</param>
        /// <param name="columnMasks">열별 사용 숫자 Mask입니다.</param>
        /// <param name="regionMasks">Region별 사용 숫자 Mask입니다.</param>
        /// <param name="remainingLimit">현재 호출에서 탐색할 수 있는 남은 해답 수입니다.</param>
        /// <returns>남은 제한 범위 안에서 발견한 해답 개수입니다.</returns>
        private static int CountRecursive(
            int[] board,
            IReadOnlyList<int> regionMap,
            int[] rowMasks,
            int[] columnMasks,
            int[] regionMasks,
            int remainingLimit)
        {
            int targetCellIndex = -1;
            int targetCandidateMask = 0;
            int minimumCandidateCount = SudokuDefine.MaxCellValue + 1;

            for (int cellIndex = 0; cellIndex < board.Length; cellIndex++)
            {
                if (board[cellIndex] != SudokuDefine.EmptyCellValue)
                {
                    continue;
                }

                int row = cellIndex / SudokuDefine.BoardSize;
                int column = cellIndex % SudokuDefine.BoardSize;
                int regionId = regionMap[cellIndex];
                int usedMask = rowMasks[row] | columnMasks[column] | regionMasks[regionId];
                int candidateMask = SolverAllNumberMask & ~usedMask;
                int candidateCount = CountSolverBits(candidateMask);

                if (candidateCount == 0)
                {
                    return 0;
                }

                if (candidateCount < minimumCandidateCount)
                {
                    targetCellIndex = cellIndex;
                    targetCandidateMask = candidateMask;
                    minimumCandidateCount = candidateCount;

                    if (candidateCount == 1)
                    {
                        break;
                    }
                }
            }

            if (targetCellIndex < 0)
            {
                return 1;
            }

            int targetRow = targetCellIndex / SudokuDefine.BoardSize;
            int targetColumn = targetCellIndex % SudokuDefine.BoardSize;
            int targetRegionId = regionMap[targetCellIndex];
            int solutionCount = 0;

            for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                int valueMask = 1 << value;

                if ((targetCandidateMask & valueMask) == 0)
                {
                    continue;
                }

                board[targetCellIndex] = value;
                rowMasks[targetRow] |= valueMask;
                columnMasks[targetColumn] |= valueMask;
                regionMasks[targetRegionId] |= valueMask;

                solutionCount += CountRecursive(
                    board,
                    regionMap,
                    rowMasks,
                    columnMasks,
                    regionMasks,
                    remainingLimit - solutionCount);

                board[targetCellIndex] = SudokuDefine.EmptyCellValue;
                rowMasks[targetRow] &= ~valueMask;
                columnMasks[targetColumn] &= ~valueMask;
                regionMasks[targetRegionId] &= ~valueMask;

                if (solutionCount >= remainingLimit)
                {
                    break;
                }
            }

            return solutionCount;
        }

        /// <summary>
        /// 정수 Mask에 설정된 Bit 개수를 계산합니다.
        /// </summary>
        /// <param name="value">설정된 Bit를 계산할 정수입니다.</param>
        /// <returns>설정된 Bit의 개수입니다.</returns>
        private static int CountSolverBits(int value)
        {
            int count = 0;

            while (value != 0)
            {
                value &= value - 1;
                count++;
            }

            return count;
        }

        #endregion

        #region Difficulty Evaluation

        private const int DifficultyAllNumberMask = (1 << (SudokuDefine.MaxCellValue + 1)) - 2;

        /// <summary>
        /// 사람식 풀이 기법을 쉬운 순서대로 적용해 문제 난이도를 평가합니다.
        /// </summary>
        /// <param name="puzzle">빈 셀이 0으로 저장된 문제 배열입니다.</param>
        /// <param name="regionMap">각 셀이 속한 Region ID 목록입니다.</param>
        /// <returns>난이도, 최고 기법, 점수와 논리적 해결 여부입니다.</returns>
        public static SudokuDifficultyResult Evaluate(
            IReadOnlyList<int> puzzle,
            IReadOnlyList<int> regionMap)
        {
            if (!TryCreateState(puzzle, regionMap, out EvaluationState state))
            {
                return CreateGuessingResult(SudokuDifficultyDefine.GuessingScore);
            }

            int score = 0;
            ESudokuSolveTechnique hardestTechnique = ESudokuSolveTechnique.None;

            while (state.EmptyCellCount > 0)
            {
                if (TryApplyNakedSingle(state))
                {
                    score += SudokuDifficultyDefine.NakedSingleScore;
                    UpdateHardestTechnique(ref hardestTechnique, ESudokuSolveTechnique.NakedSingle);
                    continue;
                }

                if (TryApplyHiddenSingle(state))
                {
                    score += SudokuDifficultyDefine.HiddenSingleScore;
                    UpdateHardestTechnique(ref hardestTechnique, ESudokuSolveTechnique.HiddenSingle);
                    continue;
                }

                int lockedCandidateEliminations = ApplyLockedCandidate(state);

                if (lockedCandidateEliminations > 0)
                {
                    score += lockedCandidateEliminations * SudokuDifficultyDefine.LockedCandidateScore;
                    UpdateHardestTechnique(ref hardestTechnique, ESudokuSolveTechnique.LockedCandidate);
                    continue;
                }

                int nakedPairEliminations = ApplyNakedPair(state);

                if (nakedPairEliminations > 0)
                {
                    score += nakedPairEliminations * SudokuDifficultyDefine.NakedPairScore;
                    UpdateHardestTechnique(ref hardestTechnique, ESudokuSolveTechnique.NakedPair);
                    continue;
                }

                return CreateGuessingResult(score + SudokuDifficultyDefine.GuessingScore);
            }

            ESudokuDifficulty difficulty = ClassifyDifficulty(hardestTechnique, score);
            return new SudokuDifficultyResult(difficulty, hardestTechnique, score, true);
        }

        /// <summary>
        /// 문제 배열을 사람식 풀이 평가에 필요한 Board와 Candidate Mask 상태로 변환합니다.
        /// </summary>
        /// <param name="puzzle">평가할 문제 배열입니다.</param>
        /// <param name="regionMap">셀별 Region ID 목록입니다.</param>
        /// <param name="state">생성된 평가 상태입니다.</param>
        /// <returns>문제와 Region 데이터가 유효하면 true입니다.</returns>
        private static bool TryCreateState(
            IReadOnlyList<int> puzzle,
            IReadOnlyList<int> regionMap,
            out EvaluationState state)
        {
            state = null;

            if (puzzle == null
                || regionMap == null
                || puzzle.Count != SudokuDefine.CellCount
                || regionMap.Count != SudokuDefine.CellCount)
            {
                return false;
            }

            int[] board = new int[SudokuDefine.CellCount];
            int[] candidates = new int[SudokuDefine.CellCount];
            int[] rowMasks = new int[SudokuDefine.BoardSize];
            int[] columnMasks = new int[SudokuDefine.BoardSize];
            int[] regionMasks = new int[SudokuDefine.RegionCount];
            int emptyCellCount = 0;

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                int value = puzzle[cellIndex];
                int regionId = regionMap[cellIndex];

                if (regionId < 0 || regionId >= SudokuDefine.RegionCount)
                {
                    return false;
                }

                board[cellIndex] = value;

                if (value == SudokuDefine.EmptyCellValue)
                {
                    emptyCellCount++;
                    continue;
                }

                if (value < SudokuDefine.MinCellValue || value > SudokuDefine.MaxCellValue)
                {
                    return false;
                }

                int row = cellIndex / SudokuDefine.BoardSize;
                int column = cellIndex % SudokuDefine.BoardSize;
                int valueMask = 1 << value;

                if ((rowMasks[row] & valueMask) != 0
                    || (columnMasks[column] & valueMask) != 0
                    || (regionMasks[regionId] & valueMask) != 0)
                {
                    return false;
                }

                rowMasks[row] |= valueMask;
                columnMasks[column] |= valueMask;
                regionMasks[regionId] |= valueMask;
            }

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                if (board[cellIndex] != SudokuDefine.EmptyCellValue)
                {
                    continue;
                }

                int row = cellIndex / SudokuDefine.BoardSize;
                int column = cellIndex % SudokuDefine.BoardSize;
                int regionId = regionMap[cellIndex];
                int usedMask = rowMasks[row] | columnMasks[column] | regionMasks[regionId];
                candidates[cellIndex] = DifficultyAllNumberMask & ~usedMask;

                if (candidates[cellIndex] == 0)
                {
                    return false;
                }
            }

            state = new EvaluationState(
                board,
                candidates,
                CopyRegionMap(regionMap),
                emptyCellCount);

            return true;
        }

        /// <summary>
        /// 후보가 하나뿐인 셀 하나를 찾아 값을 확정합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <returns>Naked Single을 적용했으면 true입니다.</returns>
        private static bool TryApplyNakedSingle(EvaluationState state)
        {
            for (int cellIndex = 0; cellIndex < state.Candidates.Length; cellIndex++)
            {
                int candidateMask = state.Candidates[cellIndex];

                if (CountCandidateBits(candidateMask) == 1)
                {
                    PlaceValue(state, cellIndex, GetSingleValue(candidateMask));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 행, 열 또는 Region 안에서 특정 후보가 한 셀에만 존재하는 경우 값을 확정합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <returns>Hidden Single을 적용했으면 true입니다.</returns>
        private static bool TryApplyHiddenSingle(EvaluationState state)
        {
            for (int unitIndex = 0; unitIndex < SudokuDefine.BoardSize; unitIndex++)
            {
                if (TryApplyHiddenSingleInUnit(state, CreateRowUnit(unitIndex))
                    || TryApplyHiddenSingleInUnit(state, CreateColumnUnit(unitIndex))
                    || TryApplyHiddenSingleInUnit(state, CreateRegionUnit(state.RegionMap, unitIndex)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 한 Unit에서 한 번만 등장하는 후보 숫자를 찾아 값을 확정합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <param name="unitCells">검사할 행, 열 또는 Region의 셀 인덱스입니다.</param>
        /// <returns>Hidden Single을 적용했으면 true입니다.</returns>
        private static bool TryApplyHiddenSingleInUnit(EvaluationState state, int[] unitCells)
        {
            for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                int valueMask = 1 << value;
                int matchingCellIndex = -1;
                int matchingCount = 0;

                for (int index = 0; index < unitCells.Length; index++)
                {
                    int cellIndex = unitCells[index];

                    if ((state.Candidates[cellIndex] & valueMask) != 0)
                    {
                        matchingCellIndex = cellIndex;
                        matchingCount++;
                    }
                }

                if (matchingCount == 1)
                {
                    PlaceValue(state, matchingCellIndex, value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Region 안의 후보가 한 행 또는 열에만 모인 경우 바깥 셀에서 해당 후보를 제거합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <returns>제거한 Candidate Bit의 개수입니다.</returns>
        private static int ApplyLockedCandidate(EvaluationState state)
        {
            for (int regionId = 0; regionId < SudokuDefine.RegionCount; regionId++)
            {
                int[] regionCells = CreateRegionUnit(state.RegionMap, regionId);

                for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
                {
                    int valueMask = 1 << value;
                    int sharedRow = -1;
                    int sharedColumn = -1;
                    int candidateCount = 0;
                    bool sameRow = true;
                    bool sameColumn = true;

                    for (int index = 0; index < regionCells.Length; index++)
                    {
                        int cellIndex = regionCells[index];

                        if ((state.Candidates[cellIndex] & valueMask) == 0)
                        {
                            continue;
                        }

                        int row = cellIndex / SudokuDefine.BoardSize;
                        int column = cellIndex % SudokuDefine.BoardSize;

                        if (candidateCount == 0)
                        {
                            sharedRow = row;
                            sharedColumn = column;
                        }
                        else
                        {
                            sameRow &= sharedRow == row;
                            sameColumn &= sharedColumn == column;
                        }

                        candidateCount++;
                    }

                    if (candidateCount < 2)
                    {
                        continue;
                    }

                    int eliminationCount = 0;

                    if (sameRow)
                    {
                        eliminationCount += RemoveCandidateOutsideRegion(
                            state,
                            CreateRowUnit(sharedRow),
                            regionId,
                            valueMask);
                    }

                    if (sameColumn)
                    {
                        eliminationCount += RemoveCandidateOutsideRegion(
                            state,
                            CreateColumnUnit(sharedColumn),
                            regionId,
                            valueMask);
                    }

                    if (eliminationCount > 0)
                    {
                        return eliminationCount;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 동일한 두 후보를 가진 셀 두 개를 찾아 같은 Unit의 다른 셀에서 후보를 제거합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <returns>제거한 Candidate Bit의 개수입니다.</returns>
        private static int ApplyNakedPair(EvaluationState state)
        {
            for (int unitIndex = 0; unitIndex < SudokuDefine.BoardSize; unitIndex++)
            {
                int eliminations = ApplyNakedPairInUnit(state, CreateRowUnit(unitIndex));

                if (eliminations == 0)
                {
                    eliminations = ApplyNakedPairInUnit(state, CreateColumnUnit(unitIndex));
                }

                if (eliminations == 0)
                {
                    eliminations = ApplyNakedPairInUnit(
                        state,
                        CreateRegionUnit(state.RegionMap, unitIndex));
                }

                if (eliminations > 0)
                {
                    return eliminations;
                }
            }

            return 0;
        }

        /// <summary>
        /// 한 Unit에서 Naked Pair를 찾아 다른 셀의 후보를 제거합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <param name="unitCells">검사할 Unit의 셀 인덱스입니다.</param>
        /// <returns>제거한 Candidate Bit의 개수입니다.</returns>
        private static int ApplyNakedPairInUnit(EvaluationState state, int[] unitCells)
        {
            for (int firstIndex = 0; firstIndex < unitCells.Length - 1; firstIndex++)
            {
                int firstCellIndex = unitCells[firstIndex];
                int pairMask = state.Candidates[firstCellIndex];

                if (CountCandidateBits(pairMask) != 2)
                {
                    continue;
                }

                for (int secondIndex = firstIndex + 1; secondIndex < unitCells.Length; secondIndex++)
                {
                    int secondCellIndex = unitCells[secondIndex];

                    if (state.Candidates[secondCellIndex] != pairMask)
                    {
                        continue;
                    }

                    int eliminationCount = 0;

                    for (int index = 0; index < unitCells.Length; index++)
                    {
                        int cellIndex = unitCells[index];

                        if (cellIndex == firstCellIndex || cellIndex == secondCellIndex)
                        {
                            continue;
                        }

                        eliminationCount += RemoveCandidateMask(state, cellIndex, pairMask);
                    }

                    if (eliminationCount > 0)
                    {
                        return eliminationCount;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 확정된 값을 Board에 기록하고 같은 행, 열, Region의 후보에서 제거합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <param name="cellIndex">값을 확정할 셀 인덱스입니다.</param>
        /// <param name="value">확정할 숫자입니다.</param>
        private static void PlaceValue(EvaluationState state, int cellIndex, int value)
        {
            state.Board[cellIndex] = value;
            state.Candidates[cellIndex] = 0;
            state.EmptyCellCount--;

            int row = cellIndex / SudokuDefine.BoardSize;
            int column = cellIndex % SudokuDefine.BoardSize;
            int regionId = state.RegionMap[cellIndex];
            int valueMask = 1 << value;

            for (int targetCellIndex = 0; targetCellIndex < SudokuDefine.CellCount; targetCellIndex++)
            {
                if (state.Candidates[targetCellIndex] == 0)
                {
                    continue;
                }

                int targetRow = targetCellIndex / SudokuDefine.BoardSize;
                int targetColumn = targetCellIndex % SudokuDefine.BoardSize;
                int targetRegionId = state.RegionMap[targetCellIndex];

                if (targetRow == row || targetColumn == column || targetRegionId == regionId)
                {
                    state.Candidates[targetCellIndex] &= ~valueMask;
                }
            }
        }

        /// <summary>
        /// 지정한 Unit에서 특정 Region 바깥 셀의 후보 숫자를 제거합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <param name="unitCells">후보를 제거할 행 또는 열의 셀 인덱스입니다.</param>
        /// <param name="excludedRegionId">후보를 유지할 Region ID입니다.</param>
        /// <param name="valueMask">제거할 후보 숫자 Mask입니다.</param>
        /// <returns>제거한 Candidate Bit의 개수입니다.</returns>
        private static int RemoveCandidateOutsideRegion(
            EvaluationState state,
            int[] unitCells,
            int excludedRegionId,
            int valueMask)
        {
            int eliminationCount = 0;

            for (int index = 0; index < unitCells.Length; index++)
            {
                int cellIndex = unitCells[index];

                if (state.RegionMap[cellIndex] != excludedRegionId)
                {
                    eliminationCount += RemoveCandidateMask(state, cellIndex, valueMask);
                }
            }

            return eliminationCount;
        }

        /// <summary>
        /// 한 셀의 Candidate Mask에서 지정한 Bit를 제거합니다.
        /// </summary>
        /// <param name="state">현재 사람식 풀이 평가 상태입니다.</param>
        /// <param name="cellIndex">후보를 제거할 셀 인덱스입니다.</param>
        /// <param name="removeMask">제거할 Candidate Bit Mask입니다.</param>
        /// <returns>실제로 제거된 Candidate Bit의 개수입니다.</returns>
        private static int RemoveCandidateMask(
            EvaluationState state,
            int cellIndex,
            int removeMask)
        {
            int previousMask = state.Candidates[cellIndex];

            if (previousMask == 0)
            {
                return 0;
            }

            int removedMask = previousMask & removeMask;
            state.Candidates[cellIndex] = previousMask & ~removeMask;
            return CountCandidateBits(removedMask);
        }

        /// <summary>
        /// 행에 포함된 셀 인덱스를 생성합니다.
        /// </summary>
        /// <param name="row">생성할 행 인덱스입니다.</param>
        /// <returns>행 우선 순서의 셀 인덱스 배열입니다.</returns>
        private static int[] CreateRowUnit(int row)
        {
            int[] cells = new int[SudokuDefine.BoardSize];

            for (int column = 0; column < SudokuDefine.BoardSize; column++)
            {
                cells[column] = row * SudokuDefine.BoardSize + column;
            }

            return cells;
        }

        /// <summary>
        /// 열에 포함된 셀 인덱스를 생성합니다.
        /// </summary>
        /// <param name="column">생성할 열 인덱스입니다.</param>
        /// <returns>위에서 아래 순서의 셀 인덱스 배열입니다.</returns>
        private static int[] CreateColumnUnit(int column)
        {
            int[] cells = new int[SudokuDefine.BoardSize];

            for (int row = 0; row < SudokuDefine.BoardSize; row++)
            {
                cells[row] = row * SudokuDefine.BoardSize + column;
            }

            return cells;
        }

        /// <summary>
        /// Region에 포함된 셀 인덱스를 생성합니다.
        /// </summary>
        /// <param name="regionMap">셀별 Region ID 목록입니다.</param>
        /// <param name="regionId">조회할 Region ID입니다.</param>
        /// <returns>Region에 속한 셀 인덱스 배열입니다.</returns>
        private static int[] CreateRegionUnit(IReadOnlyList<int> regionMap, int regionId)
        {
            int[] cells = new int[SudokuDefine.CellsPerRegion];
            int writeIndex = 0;

            for (int cellIndex = 0; cellIndex < regionMap.Count; cellIndex++)
            {
                if (regionMap[cellIndex] == regionId && writeIndex < cells.Length)
                {
                    cells[writeIndex++] = cellIndex;
                }
            }

            return cells;
        }

        /// <summary>
        /// Region Map을 평가 전용 배열로 복사합니다.
        /// </summary>
        /// <param name="regionMap">복사할 셀별 Region ID 목록입니다.</param>
        /// <returns>복사된 Region Map입니다.</returns>
        private static int[] CopyRegionMap(IReadOnlyList<int> regionMap)
        {
            int[] copiedRegionMap = new int[SudokuDefine.CellCount];

            for (int cellIndex = 0; cellIndex < copiedRegionMap.Length; cellIndex++)
            {
                copiedRegionMap[cellIndex] = regionMap[cellIndex];
            }

            return copiedRegionMap;
        }

        /// <summary>
        /// 사용한 최고 기법과 점수로 최종 난이도를 분류합니다.
        /// </summary>
        /// <param name="hardestTechnique">풀이 중 사용된 가장 어려운 기법입니다.</param>
        /// <param name="score">풀이 과정의 누적 점수입니다.</param>
        /// <returns>평가된 스도쿠 난이도입니다.</returns>
        private static ESudokuDifficulty ClassifyDifficulty(
            ESudokuSolveTechnique hardestTechnique,
            int score)
        {
            if (hardestTechnique <= ESudokuSolveTechnique.NakedSingle
                && score <= SudokuDifficultyDefine.EasyMaxScore)
            {
                return ESudokuDifficulty.Easy;
            }

            if (hardestTechnique <= ESudokuSolveTechnique.HiddenSingle
                && score <= SudokuDifficultyDefine.NormalMaxScore)
            {
                return ESudokuDifficulty.Normal;
            }

            return ESudokuDifficulty.Hard;
        }

        /// <summary>
        /// 지원하는 논리 기법만으로 풀 수 없는 문제의 평가 결과를 생성합니다.
        /// </summary>
        /// <param name="score">논리 풀이 중 누적된 점수와 추측 가중치의 합입니다.</param>
        /// <returns>Extreme으로 분류된 평가 결과입니다.</returns>
        private static SudokuDifficultyResult CreateGuessingResult(int score)
        {
            return new SudokuDifficultyResult(
                ESudokuDifficulty.Extreme,
                ESudokuSolveTechnique.Guessing,
                score,
                false);
        }

        /// <summary>
        /// 현재까지 사용한 가장 어려운 풀이 기법을 갱신합니다.
        /// </summary>
        /// <param name="currentTechnique">현재 기록된 최고 기법입니다.</param>
        /// <param name="usedTechnique">이번 단계에서 사용한 기법입니다.</param>
        private static void UpdateHardestTechnique(
            ref ESudokuSolveTechnique currentTechnique,
            ESudokuSolveTechnique usedTechnique)
        {
            if (usedTechnique > currentTechnique)
            {
                currentTechnique = usedTechnique;
            }
        }

        /// <summary>
        /// Candidate Mask에 설정된 Bit 개수를 계산합니다.
        /// </summary>
        /// <param name="value">설정된 Bit를 계산할 Mask입니다.</param>
        /// <returns>설정된 Bit의 개수입니다.</returns>
        private static int CountCandidateBits(int value)
        {
            int count = 0;

            while (value != 0)
            {
                value &= value - 1;
                count++;
            }

            return count;
        }

        /// <summary>
        /// 하나의 Bit만 설정된 Candidate Mask에서 숫자를 반환합니다.
        /// </summary>
        /// <param name="candidateMask">하나의 후보만 포함한 Mask입니다.</param>
        /// <returns>Mask에 포함된 숫자입니다.</returns>
        private static int GetSingleValue(int candidateMask)
        {
            for (int value = SudokuDefine.MinCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                if ((candidateMask & (1 << value)) != 0)
                {
                    return value;
                }
            }

            return SudokuDefine.EmptyCellValue;
        }

        private class EvaluationState
        {
            public int[] Board { get; }
            public int[] Candidates { get; }
            public int[] RegionMap { get; }
            public int EmptyCellCount { get; set; }

            /// <summary>
            /// 사람식 풀이 과정에서 변경할 평가 상태를 생성합니다.
            /// </summary>
            /// <param name="board">현재 셀 값 배열입니다.</param>
            /// <param name="candidates">셀별 Candidate Mask 배열입니다.</param>
            /// <param name="regionMap">셀별 Region ID 배열입니다.</param>
            /// <param name="emptyCellCount">현재 빈 셀 개수입니다.</param>
            public EvaluationState(
                int[] board,
                int[] candidates,
                int[] regionMap,
                int emptyCellCount)
            {
                Board = board;
                Candidates = candidates;
                RegionMap = regionMap;
                EmptyCellCount = emptyCellCount;
            }
        }

        #endregion
    }
}

