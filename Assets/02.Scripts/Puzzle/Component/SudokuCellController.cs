using System;
using System.Collections.Generic;
using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using Define;
    using Logic;

    public class SudokuCellController : MonoBehaviour
    {
        #region Fields

        private readonly int[] cellValues = new int[SudokuDefine.CellCount];
        private readonly HashSet<int>[] cellIndicesByValue = CreateValueIndexCache();
        private readonly bool[] cellValidationStates = CreateInitialValidationStates();
        private readonly bool[] givenCellStates = new bool[SudokuDefine.CellCount];
        private readonly List<int> invalidCellIndices = new();

        [Tooltip("셀 값의 행, 열, Region 규칙을 검사할 Region Controller입니다.")]
        [SerializeField] private SudokuRegionController regionController;

        [Tooltip("Cell에 숫자가 입력될 때 해당 Cell의 Memo를 제거할 Controller입니다.")]
        [SerializeField] private SudokuMemoController memoController;

        #endregion

        #region Events And Properties

        public event Action<int, int, int> CellValueChanged;
        public event Action<int, bool> CellValidationChanged;
        public event Action<int, bool> GivenCellStateChanged;
        public event Action CellDataUpdated;

        public bool IsInitializingGivenCells { get; private set; }

        #endregion

        #region Cell Query

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
        /// 지정한 숫자가 현재 보드에 입력된 셀 개수를 반환합니다.
        /// </summary>
        /// <param name="value">개수를 조회할 1~9 숫자입니다.</param>
        /// <returns>Given 셀과 플레이어 입력 셀을 모두 포함한 숫자별 셀 개수입니다.</returns>
        public int GetCellValueCount(int value)
        {
            if (value < SudokuDefine.MinCellValue || value > SudokuDefine.MaxCellValue)
            {
                return 0;
            }

            return cellIndicesByValue[value].Count;
        }

        /// <summary>
        /// 모든 Cell이 채워졌으며 현재 검증 상태가 유효한지 확인합니다.
        /// </summary>
        /// <returns>Sudoku 보드가 정상적으로 완성되었으면 true입니다.</returns>
        public bool IsBoardCompleted()
        {
            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                if (cellValues[cellIndex] == SudokuDefine.EmptyCellValue
                    || !cellValidationStates[cellIndex])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Validation Query

        /// <summary>
        /// 지정한 셀의 현재 숫자가 행, 열, Region 규칙을 만족하는지 반환합니다.
        /// </summary>
        /// <param name="cellIndex">검증 상태를 조회할 셀 인덱스입니다.</param>
        /// <returns>셀이 비어 있거나 숫자 배치가 유효하면 true입니다.</returns>
        public bool IsCellValueValid(int cellIndex)
        {
            if (!IsValidCellIndex(cellIndex))
            {
                return false;
            }

            return cellValues[cellIndex] == SudokuDefine.EmptyCellValue
                || cellValidationStates[cellIndex];
        }

        #endregion

        #region Given Cell

        /// <summary>
        /// 지정한 셀이 게임 시작 시 제공된 Given 셀인지 반환합니다.
        /// </summary>
        /// <param name="cellIndex">Given 상태를 조회할 셀 인덱스입니다.</param>
        /// <returns>플레이어가 수정할 수 없는 Given 셀이면 true입니다.</returns>
        public bool IsGivenCell(int cellIndex)
        {
            return IsValidCellIndex(cellIndex) && givenCellStates[cellIndex];
        }

        /// <summary>
        /// 생성된 Puzzle 값을 초기 셀 데이터로 등록하고 0이 아닌 셀을 Given으로 설정합니다.
        /// </summary>
        /// <param name="puzzle">빈 셀이 0으로 저장된 81개 Puzzle 값입니다.</param>
        /// <returns>초기 Given 데이터가 정상적으로 등록되었으면 true입니다.</returns>
        public bool InitializeGivenCells(IReadOnlyList<int> puzzle)
        {
            if (puzzle == null || puzzle.Count != SudokuDefine.CellCount)
            {
                Debug.LogError("Given Cell을 초기화하려면 81개의 Puzzle 값이 필요합니다.", this);
                return false;
            }

            IsInitializingGivenCells = true;

            try
            {
                ResetCellData();

                for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
                {
                    int value = puzzle[cellIndex];

                    if (value < SudokuDefine.EmptyCellValue || value > SudokuDefine.MaxCellValue)
                    {
                        Debug.LogError($"Given Cell 값은 0~{SudokuDefine.MaxCellValue} 범위여야 합니다.", this);
                        ResetCellData();
                        return false;
                    }

                    int previousValue = cellValues[cellIndex];
                    bool isGiven = value != SudokuDefine.EmptyCellValue;

                    cellValues[cellIndex] = value;
                    givenCellStates[cellIndex] = isGiven;

                    if (isGiven)
                    {
                        cellIndicesByValue[value].Add(cellIndex);
                    }

                    CellValueChanged?.Invoke(cellIndex, previousValue, value);
                    GivenCellStateChanged?.Invoke(cellIndex, isGiven);
                }
            }
            finally
            {
                IsInitializingGivenCells = false;
            }

            CellDataUpdated?.Invoke();
            return true;
        }

        #endregion

        #region Cell Mutation

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

            if (givenCellStates[cellIndex])
            {
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
                memoController?.ClearMemo(cellIndex);
            }

            CellValueChanged?.Invoke(cellIndex, previousValue, value);
            RefreshValidationStates(cellIndex);
            CellDataUpdated?.Invoke();
            return true;
        }

        /// <summary>
        /// 현재 셀 값, Given 상태, 숫자별 Index와 오류 상태를 초기화합니다.
        /// </summary>
        private void ResetCellData()
        {
            invalidCellIndices.Clear();

            for (int value = SudokuDefine.EmptyCellValue; value <= SudokuDefine.MaxCellValue; value++)
            {
                cellIndicesByValue[value].Clear();
            }

            for (int cellIndex = 0; cellIndex < SudokuDefine.CellCount; cellIndex++)
            {
                cellValues[cellIndex] = SudokuDefine.EmptyCellValue;
                givenCellStates[cellIndex] = false;
                cellValidationStates[cellIndex] = true;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// 새로 입력된 셀과 기존 오류 셀만 다시 검사하고 변경된 검증 상태를 전달합니다.
        /// </summary>
        /// <param name="changedCellIndex">이번 입력으로 값이 변경된 셀 인덱스입니다.</param>
        private void RefreshValidationStates(int changedCellIndex)
        {
            if (regionController == null || regionController.RegionData == null)
            {
                Debug.LogError("셀 값 검증에 사용할 Region Controller가 준비되지 않았습니다.", this);
                return;
            }

            ApplyValidationState(changedCellIndex);

            for (int invalidIndex = invalidCellIndices.Count - 1; invalidIndex >= 0; invalidIndex--)
            {
                int invalidCellIndex = invalidCellIndices[invalidIndex];

                if (invalidCellIndex != changedCellIndex)
                {
                    ApplyValidationState(invalidCellIndex);
                }
            }
        }

        /// <summary>
        /// 지정한 셀 하나의 Sudoku 규칙을 검사하고 오류 Index와 이벤트 상태를 갱신합니다.
        /// </summary>
        /// <param name="cellIndex">검증할 셀 인덱스입니다.</param>
        private void ApplyValidationState(int cellIndex)
        {
            bool isValid = SudokuGameLogic.IsCellValueValid(
                cellValues,
                regionController.RegionData.RegionMap,
                cellIndex);

            if (isValid)
            {
                invalidCellIndices.Remove(cellIndex);
            }
            else if (!invalidCellIndices.Contains(cellIndex))
            {
                invalidCellIndices.Add(cellIndex);
            }

            if (cellValidationStates[cellIndex] == isValid)
            {
                return;
            }

            cellValidationStates[cellIndex] = isValid;
            CellValidationChanged?.Invoke(cellIndex, isValid);
        }

        #endregion

        #region Cache

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
        /// 모든 셀이 정상인 상태로 시작하는 초기 검증 상태 배열을 생성합니다.
        /// </summary>
        /// <returns>모든 값이 true로 설정된 셀 검증 상태 배열입니다.</returns>
        private static bool[] CreateInitialValidationStates()
        {
            bool[] validationStates = new bool[SudokuDefine.CellCount];

            for (int cellIndex = 0; cellIndex < validationStates.Length; cellIndex++)
            {
                validationStates[cellIndex] = true;
            }

            return validationStates;
        }

        #endregion

        #region Util

        /// <summary>
        /// 셀 인덱스가 보드 범위 안에 있는지 확인합니다.
        /// </summary>
        /// <param name="cellIndex">검사할 행 우선 방식의 셀 인덱스입니다.</param>
        /// <returns>유효한 셀 인덱스이면 true입니다.</returns>
        private static bool IsValidCellIndex(int cellIndex)
        {
            return cellIndex >= 0 && cellIndex < SudokuDefine.CellCount;
        }

        #endregion
    }
}
