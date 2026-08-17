using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.PanelScale
{
    public class PanelMatchController : MonoBehaviour
    {
        [Tooltip("화면 비율에 따라 Match 값을 변경할 UI Document입니다.")]
        [SerializeField] private UIDocument uiDocument;

        [Tooltip("Width Match에서 Height Match로 전환할 Width / Height 기준값입니다.")]
        [SerializeField] private float heightMatchThreshold = 0.6f;

        private PanelSettings panelSettings;
        private int previousScreenWidth = -1;
        private int previousScreenHeight = -1;

        #region Unity Callbacks

        private void OnEnable()
        {
            CachePanelSettings();
            RefreshMatchIfResolutionChanged();
        }

        private void Update()
        {
            RefreshMatchIfResolutionChanged();
        }

        #endregion

        /// <summary>
        /// 연결된 UI Document에서 Panel Settings 참조를 가져옵니다.
        /// </summary>
        private void CachePanelSettings()
        {
            panelSettings = uiDocument != null ? uiDocument.panelSettings : null;
        }

        /// <summary>
        /// 화면 해상도가 변경된 경우에만 Panel Settings의 Match 값을 다시 계산합니다.
        /// </summary>
        private void RefreshMatchIfResolutionChanged()
        {
            int currentWidth = Screen.width;
            int currentHeight = Screen.height;

            if (previousScreenWidth == currentWidth && previousScreenHeight == currentHeight)
            {
                return;
            }

            previousScreenWidth = currentWidth;
            previousScreenHeight = currentHeight;

            ApplyScreenMatch(currentWidth, currentHeight);
        }

        /// <summary>
        /// 현재 화면 비율에 따라 Width Match 또는 Height Match를 적용합니다.
        /// </summary>
        /// <param name="screenWidth">현재 화면의 픽셀 너비입니다.</param>
        /// <param name="screenHeight">현재 화면의 픽셀 높이입니다.</param>
        private void ApplyScreenMatch(int screenWidth, int screenHeight)
        {
            if (panelSettings == null || screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            float screenAspect = (float)screenWidth / screenHeight;
            panelSettings.match = screenAspect <= heightMatchThreshold ? 0f : 1f;
        }
    }
}
