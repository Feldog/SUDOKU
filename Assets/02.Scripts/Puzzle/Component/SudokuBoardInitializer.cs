using UnityEngine;

namespace SUDOKU.Puzzle.Component
{
    using View;

    public sealed class SudokuBoardInitializer : MonoBehaviour
    {
        [Tooltip("초기 보드 틀에 사용할 Region 데이터를 제공하는 Controller입니다.")]
        [SerializeField] private SudokuRegionController regionController;

        [Tooltip("계산된 Region에 맞춰 초기 Border를 적용할 View입니다.")]
        [SerializeField] private SudokuRegionBorderView regionBorderView;

        #region Unity Callbacks

        private void Start()
        {
            InitializeBoardFrame();
        }

        #endregion

        /// <summary>
        /// 게임 시작 시 준비된 Region 데이터를 사용해 보드의 초기 Border를 한 번 적용합니다.
        /// </summary>
        private void InitializeBoardFrame()
        {
            if (regionController == null || regionController.RegionData == null)
            {
                Debug.LogError("초기 보드 틀에 사용할 Region Controller가 준비되지 않았습니다.", this);
                return;
            }

            if (regionBorderView == null)
            {
                Debug.LogError("초기 보드 틀에 사용할 Region Border View가 연결되지 않았습니다.", this);
                return;
            }

            regionBorderView.ApplyRegionBorders(regionController.RegionData.RegionMap);
        }
    }
}
