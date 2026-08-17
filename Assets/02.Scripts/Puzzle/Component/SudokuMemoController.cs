using System;
using System.Collections.Generic;
using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using Define;

    public class SudokuMemoController : MonoBehaviour
    {
        [Tooltip("게임 시작 시 Memo 입력 상태를 활성화할지 여부입니다.")]
        [SerializeField] private bool isMemoActive;

        private readonly Dictionary<int, HashSet<int>> memoValuesByCellIndex = new();

        public event Action<bool> MemoStateChanged;
        public event Action<int> MemoValuesChanged;

        public bool IsMemoActive => isMemoActive;

        /// <summary>
        /// 지정한 셀에 특정 Memo 숫자가 저장되어 있는지 확인합니다.
        /// </summary>
        /// <param name="cellIndex">Memo 데이터를 확인할 셀 인덱스입니다.</param>
        /// <param name="value">저장 여부를 확인할 Memo 숫자입니다.</param>
        /// <returns>지정한 Memo 숫자가 저장되어 있으면 true입니다.</returns>
        public bool HasMemoValue(int cellIndex, int value)
        {
            return memoValuesByCellIndex.TryGetValue(cellIndex, out HashSet<int> memoValues)
                && memoValues.Contains(value);
        }

        /// <summary>
        /// 현재 Memo 입력 상태를 반대로 전환합니다.
        /// </summary>
        public void ToggleMemoState()
        {
            SetMemoState(!isMemoActive);
        }

        /// <summary>
        /// Memo 입력 상태를 지정하고 변경 이벤트를 전달합니다.
        /// </summary>
        /// <param name="isActive">적용할 Memo 활성 상태입니다.</param>
        public void SetMemoState(bool isActive)
        {
            if (isMemoActive == isActive)
            {
                return;
            }

            isMemoActive = isActive;
            MemoStateChanged?.Invoke(isMemoActive);
        }

        /// <summary>
        /// 지정한 셀의 Memo 숫자를 추가하거나 이미 존재하면 제거합니다.
        /// </summary>
        /// <param name="cellIndex">Memo 데이터를 변경할 셀 인덱스입니다.</param>
        /// <param name="value">추가하거나 제거할 1~9 숫자입니다.</param>
        /// <returns>Memo 데이터가 정상적으로 변경되었으면 true입니다.</returns>
        public bool ToggleMemoValue(int cellIndex, int value)
        {
            if (!IsValidCellIndex(cellIndex) || !IsValidMemoValue(value))
            {
                Debug.LogError($"유효하지 않은 Memo 입력입니다. Cell: {cellIndex}, Value: {value}", this);
                return false;
            }

            if (!memoValuesByCellIndex.TryGetValue(cellIndex, out HashSet<int> memoValues))
            {
                memoValues = new HashSet<int>();
                memoValuesByCellIndex.Add(cellIndex, memoValues);
            }

            if (!memoValues.Add(value))
            {
                memoValues.Remove(value);
            }

            if (memoValues.Count == 0)
            {
                memoValuesByCellIndex.Remove(cellIndex);
            }

            MemoValuesChanged?.Invoke(cellIndex);
            return true;
        }

        /// <summary>
        /// 지정한 셀에 저장된 모든 Memo 숫자를 제거합니다.
        /// </summary>
        /// <param name="cellIndex">Memo 데이터를 제거할 셀 인덱스입니다.</param>
        /// <returns>제거할 Memo 데이터가 존재했으면 true입니다.</returns>
        public bool ClearMemo(int cellIndex)
        {
            if (!memoValuesByCellIndex.Remove(cellIndex))
            {
                return false;
            }

            MemoValuesChanged?.Invoke(cellIndex);
            return true;
        }

        /// <summary>
        /// 셀 인덱스가 Sudoku 보드 범위 안에 있는지 확인합니다.
        /// </summary>
        /// <param name="cellIndex">검사할 셀 인덱스입니다.</param>
        /// <returns>유효한 셀 인덱스이면 true입니다.</returns>
        private static bool IsValidCellIndex(int cellIndex)
        {
            return cellIndex >= 0 && cellIndex < SudokuDefine.CellCount;
        }

        /// <summary>
        /// 숫자가 Memo로 저장할 수 있는 범위인지 확인합니다.
        /// </summary>
        /// <param name="value">검사할 Memo 숫자입니다.</param>
        /// <returns>1~9 범위의 숫자이면 true입니다.</returns>
        private static bool IsValidMemoValue(int value)
        {
            return value >= SudokuDefine.MinCellValue
                && value <= SudokuDefine.MaxCellValue;
        }
    }
}
