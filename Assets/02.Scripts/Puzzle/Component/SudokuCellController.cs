using System;
using System.Collections.Generic;
using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using Define;

    public sealed class SudokuCellController : MonoBehaviour
    {
        private readonly int[] cellValues = new int[SudokuDefine.CellCount];
        private readonly HashSet<int>[] cellIndicesByValue = CreateValueIndexCache();

        public event Action<int, int, int> CellValueChanged;

        /// <summary>
        /// 지정한 셀의 현재 값을 반환합니다.
        /// </summary>
        /// <param name="cellIndex">조회할 행 우선 방식의 셀 인덱스입니다.</param>
        /// <returns>셀의 현재 값이며 0은 빈 셀을 의미합니다.</returns>
        public int GetCellValue(int cellIndex)
        {
            if (!IsValidCellIndex(cellIndex))
            {
                Debug.LogError($"유효하지 않은 셀 인덱스입니다: {cellIndex}", this);
                return SudokuDefine.EmptyCellValue;
            }

            return cellValues[cellIndex];
        }

        /// <summary>
        /// 지정한 숫자가 입력된 모든 셀 인덱스 모음을 반환합니다.
        /// </summary>
        /// <param name="value">조회할 1~9 숫자입니다.</param>
        /// <returns>해당 숫자가 입력된 셀 인덱스 모음입니다.</returns>
        public IReadOnlyCollection<int> GetCellIndicesByValue(int value)
        {
            if (value < SudokuDefine.MinCellValue || value > SudokuDefine.MaxCellValue)
            {
                return Array.Empty<int>();
            }

            return cellIndicesByValue[value];
        }

        /// <summary>
        /// 지정한 셀의 데이터를 수정하고 변경 이벤트를 전달합니다.
        /// </summary>
        /// <param name="cellIndex">수정할 행 우선 방식의 셀 인덱스입니다.</param>
        /// <param name="value">적용할 0~9 값이며 0은 빈 셀을 의미합니다.</param>
        /// <returns>셀 값이 정상적으로 변경되었으면 true입니다.</returns>
        public bool SetCellValue(int cellIndex, int value)
        {
            if (!IsValidCellIndex(cellIndex))
            {
                Debug.LogError($"유효하지 않은 셀 인덱스입니다: {cellIndex}", this);
                return false;
            }

            if (value < SudokuDefine.EmptyCellValue || value > SudokuDefine.MaxCellValue)
            {
                Debug.LogError($"셀 값은 {SudokuDefine.EmptyCellValue}~{SudokuDefine.MaxCellValue} 범위여야 합니다: {value}", this);
                return false;
            }

            if (cellValues[cellIndex] == value)
            {
                return false;
            }

            int previousValue = cellValues[cellIndex];

            if (previousValue >= SudokuDefine.MinCellValue)
            {
                cellIndicesByValue[previousValue].Remove(cellIndex);
            }

            cellValues[cellIndex] = value;

            if (value >= SudokuDefine.MinCellValue)
            {
                cellIndicesByValue[value].Add(cellIndex);
            }

            CellValueChanged?.Invoke(cellIndex, previousValue, value);
            return true;
        }

        /// <summary>
        /// 숫자별 셀 인덱스를 저장할 HashSet 배열을 생성합니다.
        /// </summary>
        /// <returns>0~9 값으로 접근할 수 있는 셀 인덱스 캐시입니다.</returns>
        private static HashSet<int>[] CreateValueIndexCache()
        {
            HashSet<int>[] cache = new HashSet<int>[SudokuDefine.MaxCellValue + 1];

            for (int value = SudokuDefine.EmptyCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                cache[value] = new HashSet<int>();
            }

            return cache;
        }

        /// <summary>
        /// 셀 인덱스가 보드 범위 안에 있는지 확인합니다.
        /// </summary>
        /// <param name="cellIndex">검사할 행 우선 방식의 셀 인덱스입니다.</param>
        /// <returns>유효한 셀 인덱스이면 true입니다.</returns>
        private static bool IsValidCellIndex(int cellIndex)
        {
            return cellIndex >= 0 && cellIndex < SudokuDefine.CellCount;
        }
    }
}
