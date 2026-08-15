using System;

namespace SUDOKU.Puzzle.Generator
{
    using Define;

    public static class SudokuSolutionGenerator
    {
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
    }
}
