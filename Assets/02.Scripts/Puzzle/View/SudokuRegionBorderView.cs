using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.Puzzle.View
{
    using Define;

    public class SudokuRegionBorderView : MonoBehaviour
    {
        private const string RightBorderClass = "border-right-region";
        private const string BottomBorderClass = "border-bottom-region";

        [Tooltip("Region Border를 적용할 게임 보드 UI Document입니다.")]
        [SerializeField] private UIDocument gameBoardDocument;

        private readonly VisualElement[] regionBorderElements = new VisualElement[SudokuDefine.CellCount];

        /// <summary>
        /// 셀별 Region ID를 비교해 게임 보드의 Region Border를 적용합니다.
        /// </summary>
        /// <param name="regionMap">행 우선 방식으로 정렬된 셀별 Region ID 목록입니다.</param>
        public void ApplyRegionBorders(IReadOnlyList<int> regionMap)
        {
            if (regionMap == null || regionMap.Count != SudokuDefine.CellCount)
            {
                Debug.LogError("Region Border를 적용하려면 유효한 81칸 Region Map이 필요합니다.", this);
                return;
            }

            if (!CacheBorderElements())
            {
                return;
            }

            for (int row = 0; row < SudokuDefine.BoardSize; row++)
            {
                for (int column = 0; column < SudokuDefine.BoardSize; column++)
                {
                    int cellIndex = row * SudokuDefine.BoardSize + column;
                    VisualElement borderElement = regionBorderElements[cellIndex];

                    bool hasRightBorder = column < SudokuDefine.BoardSize - 1
                        && regionMap[cellIndex] != regionMap[cellIndex + 1];
                    bool hasBottomBorder = row < SudokuDefine.BoardSize - 1
                        && regionMap[cellIndex] != regionMap[cellIndex + SudokuDefine.BoardSize];

                    SetBorderClass(borderElement, RightBorderClass, hasRightBorder);
                    SetBorderClass(borderElement, BottomBorderClass, hasBottomBorder);
                }
            }
        }

        /// <summary>
        /// UI Document에서 81개 Region Border Overlay를 이름으로 찾아 캐싱합니다.
        /// </summary>
        /// <returns>모든 셀을 찾았으면 true입니다.</returns>
        private bool CacheBorderElements()
        {
            if (gameBoardDocument == null)
            {
                Debug.LogError("게임 보드 UI Document가 연결되지 않았습니다.", this);
                return false;
            }

            VisualElement root = gameBoardDocument.rootVisualElement;

            for (int row = 0; row < SudokuDefine.BoardSize; row++)
            {
                for (int column = 0; column < SudokuDefine.BoardSize; column++)
                {
                    int cellIndex = row * SudokuDefine.BoardSize + column;

                    if (regionBorderElements[cellIndex] == null)
                    {
                        regionBorderElements[cellIndex] = root.Q<VisualElement>($"region-border-{cellIndex}");
                    }

                    if (regionBorderElements[cellIndex] == null)
                    {
                        Debug.LogError($"게임 보드에서 region-border-{cellIndex} 요소를 찾을 수 없습니다.", this);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 지정한 Border 클래스의 적용 여부를 갱신합니다.
        /// </summary>
        /// <param name="borderElement">Border를 변경할 Overlay 요소입니다.</param>
        /// <param name="className">적용하거나 제거할 USS 클래스 이름입니다.</param>
        /// <param name="shouldApply">클래스를 적용해야 하면 true입니다.</param>
        private static void SetBorderClass(VisualElement borderElement, string className, bool shouldApply)
        {
            if (shouldApply)
            {
                borderElement.AddToClassList(className);
                return;
            }

            borderElement.RemoveFromClassList(className);
        }
    }
}
