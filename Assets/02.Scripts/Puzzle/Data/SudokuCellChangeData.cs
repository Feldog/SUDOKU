namespace SUDOKU.Puzzle.Data
{
    public readonly struct SudokuCellChangeData
    {
        public int CellIndex { get; }
        public int PreviousValue { get; }
        public int CurrentValue { get; }

        /// <summary>
        /// 한 번의 셀 값 변경 기록을 생성합니다.
        /// </summary>
        /// <param name="cellIndex">값이 변경된 셀 인덱스입니다.</param>
        /// <param name="previousValue">변경 전 셀 값입니다.</param>
        /// <param name="currentValue">변경 후 셀 값입니다.</param>
        public SudokuCellChangeData(int cellIndex, int previousValue, int currentValue)
        {
            CellIndex = cellIndex;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }
    }
}
