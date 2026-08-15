using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using Data;
    using Define;

    public sealed class SudokuRegionController : MonoBehaviour
    {
        [Tooltip("직쏘 Region Map을 사용할지 여부입니다. 비활성화하면 기본 3×3 Region을 사용합니다.")]
        [SerializeField] private bool useJigsawRegion;

        [Tooltip("행 우선 방식으로 입력하는 81개 셀의 직쏘 Region ID입니다.")]
        [SerializeField] private int[] jigsawRegionMap = new int[SudokuDefine.CellCount];

        private SudokuRegionData regionData;

        public SudokuRegionData RegionData => regionData;

        #region Unity Callbacks

        private void Awake()
        {
            InitializeRegionData();
        }

        #endregion

        /// <summary>
        /// Inspector 설정을 기준으로 Region 데이터를 계산합니다.
        /// </summary>
        private void InitializeRegionData()
        {
            regionData = new SudokuRegionData(useJigsawRegion, jigsawRegionMap);

            if (useJigsawRegion && regionData.UsesDefaultRegion)
            {
                Debug.LogWarning("직쏘 Region Map이 유효하지 않아 기본 3×3 Region을 사용합니다.", this);
            }

        }
    }
}
