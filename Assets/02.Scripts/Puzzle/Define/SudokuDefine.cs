namespace SUDOKU.Puzzle.Define
{
    public static class SudokuDefine
    {
        public const int BoardSize = 9;
        public const int RegionSize = 3;
        public const int RegionCount = 9;
        public const int CellCount = BoardSize * BoardSize;
        public const int CellsPerRegion = CellCount / RegionCount;
        public const int EmptyCellValue = 0;
        public const int MinCellValue = 1;
        public const int MaxCellValue = BoardSize;

        private static readonly int[] defaultRegionMap = CreateDefaultRegionMap();

        /// <summary>
        /// 기본 3×3 스도쿠 Region Map의 복사본을 반환합니다.
        /// </summary>
        /// <returns>각 셀의 Region ID가 저장된 배열입니다.</returns>
        public static int[] GetDefaultRegionMap()
        {
            return (int[])defaultRegionMap.Clone();
        }

        /// <summary>
        /// 보드 크기와 Region 크기 상수를 사용해 기본 3×3 Region Map을 생성합니다.
        /// </summary>
        /// <returns>기본 Region ID가 행 우선 순서로 저장된 배열입니다.</returns>
        private static int[] CreateDefaultRegionMap()
        {
            int[] regionMap = new int[CellCount];

            for (int row = 0; row < BoardSize; row++)
            {
                for (int column = 0; column < BoardSize; column++)
                {
                    int cellIndex = row * BoardSize + column;
                    int regionRow = row / RegionSize;
                    int regionColumn = column / RegionSize;

                    regionMap[cellIndex] = regionRow * RegionSize + regionColumn;
                }
            }

            return regionMap;
        }
    }
}
