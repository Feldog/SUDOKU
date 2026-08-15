using System.Collections.Generic;

namespace SUDOKU.Puzzle.Data
{
    using Define;

    public sealed class SudokuRegionData
    {
        private readonly int[] regionMap;
        private readonly List<int>[] regionCells;

        public IReadOnlyList<int> RegionMap => regionMap;
        public bool UsesDefaultRegion { get; }

        /// <summary>
        /// 직쏘 여부와 Region Map을 기준으로 런타임 Region 데이터를 생성합니다.
        /// </summary>
        /// <param name="isJigsawRegion">직쏘 Region Map을 사용할지 여부입니다.</param>
        /// <param name="jigsawRegionMap">직쏘 퍼즐의 셀별 Region ID 목록입니다.</param>
        public SudokuRegionData(bool isJigsawRegion, IReadOnlyList<int> jigsawRegionMap)
        {
            bool canUseJigsawRegion = isJigsawRegion && IsValidRegionMap(jigsawRegionMap);

            regionMap = canUseJigsawRegion
                ? CopyRegionMap(jigsawRegionMap)
                : SudokuDefine.GetDefaultRegionMap();

            UsesDefaultRegion = !canUseJigsawRegion;
            regionCells = BuildRegionCache(regionMap);
        }

        /// <summary>
        /// 지정한 셀이 속한 Region ID를 반환합니다.
        /// </summary>
        /// <param name="cellIndex">행 우선 방식의 셀 인덱스입니다.</param>
        /// <returns>셀이 속한 Region ID입니다.</returns>
        public int GetRegionId(int cellIndex)
        {
            return regionMap[cellIndex];
        }

        /// <summary>
        /// 지정한 Region에 속한 셀 인덱스 목록을 반환합니다.
        /// </summary>
        /// <param name="regionId">조회할 Region ID입니다.</param>
        /// <returns>Region에 속한 셀 인덱스 목록입니다.</returns>
        public IReadOnlyList<int> GetRegionCells(int regionId)
        {
            return regionCells[regionId];
        }

        /// <summary>
        /// Region Map의 길이, ID 범위, Region별 셀 개수를 검증합니다.
        /// </summary>
        /// <param name="candidateRegionMap">검증할 셀별 Region ID 목록입니다.</param>
        /// <returns>직쏘 Region Map으로 사용할 수 있으면 true입니다.</returns>
        private static bool IsValidRegionMap(IReadOnlyList<int> candidateRegionMap)
        {
            if (candidateRegionMap == null || candidateRegionMap.Count != SudokuDefine.CellCount)
            {
                return false;
            }

            int[] regionCellCounts = new int[SudokuDefine.RegionCount];

            for (int cellIndex = 0; cellIndex < candidateRegionMap.Count; cellIndex++)
            {
                int regionId = candidateRegionMap[cellIndex];

                if (regionId < 0 || regionId >= SudokuDefine.RegionCount)
                {
                    return false;
                }

                regionCellCounts[regionId]++;
            }

            for (int regionId = 0; regionId < SudokuDefine.RegionCount; regionId++)
            {
                if (regionCellCounts[regionId] != SudokuDefine.CellsPerRegion)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 외부 Region Map을 런타임 전용 배열로 복사합니다.
        /// </summary>
        /// <param name="sourceRegionMap">복사할 셀별 Region ID 목록입니다.</param>
        /// <returns>복사된 Region Map입니다.</returns>
        private static int[] CopyRegionMap(IReadOnlyList<int> sourceRegionMap)
        {
            int[] copiedRegionMap = new int[SudokuDefine.CellCount];

            for (int cellIndex = 0; cellIndex < copiedRegionMap.Length; cellIndex++)
            {
                copiedRegionMap[cellIndex] = sourceRegionMap[cellIndex];
            }

            return copiedRegionMap;
        }

        /// <summary>
        /// 셀별 Region Map을 Region별 셀 인덱스 캐시로 변환합니다.
        /// </summary>
        /// <param name="sourceRegionMap">캐시를 생성할 Region Map입니다.</param>
        /// <returns>Region ID로 즉시 접근할 수 있는 셀 인덱스 목록 배열입니다.</returns>
        private static List<int>[] BuildRegionCache(IReadOnlyList<int> sourceRegionMap)
        {
            List<int>[] cache = new List<int>[SudokuDefine.RegionCount];

            for (int regionId = 0; regionId < cache.Length; regionId++)
            {
                cache[regionId] = new List<int>(SudokuDefine.CellsPerRegion);
            }

            for (int cellIndex = 0; cellIndex < sourceRegionMap.Count; cellIndex++)
            {
                cache[sourceRegionMap[cellIndex]].Add(cellIndex);
            }

            return cache;
        }
    }
}
