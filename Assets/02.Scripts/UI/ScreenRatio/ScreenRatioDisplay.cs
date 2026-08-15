using System.Globalization;
using TMPro;
using UnityEngine;

namespace SUDOKU.UI.ScreenRatio
{
    using Enum;

    public sealed class ScreenRatioDisplay : MonoBehaviour
    {
        [Tooltip("화면 비율의 계산 방향을 선택합니다.")]
        [SerializeField] private EScreenRatioMode ratioMode = EScreenRatioMode.WidthHeight;

        [Tooltip("비율 계산식을 표시할 TextMeshProUGUI입니다.")]
        [SerializeField] private TextMeshProUGUI ratioTypeText;

        [Tooltip("계산된 화면 비율을 표시할 TextMeshProUGUI입니다.")]
        [SerializeField] private TextMeshProUGUI ratioValueText;

        #region Unity Callbacks

        private void OnEnable()
        {
            if (ratioValueText != null)
            {
                ratioValueText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void Update()
        {
            UpdateRatioDisplay();
        }

        #endregion

        /// <summary>
        /// 현재 화면 해상도와 선택한 계산 방향을 두 TMP에 표시합니다.
        /// </summary>
        private void UpdateRatioDisplay()
        {
            if (ratioTypeText == null || ratioValueText == null)
            {
                return;
            }

            ratioTypeText.text = GetRatioTypeText();
            ratioValueText.text = GetScreenRatio().ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 선택한 계산 방향에 대응하는 표시 문자열을 반환합니다.
        /// </summary>
        /// <returns>화면 비율 계산식을 나타내는 문자열입니다.</returns>
        private string GetRatioTypeText()
        {
            return ratioMode == EScreenRatioMode.WidthHeight
                ? "width / height"
                : "height / width";
        }

        /// <summary>
        /// 현재 화면 너비와 높이를 사용해 선택된 방향의 비율을 계산합니다.
        /// </summary>
        /// <returns>선택된 방향으로 계산한 화면 해상도 비율입니다.</returns>
        private float GetScreenRatio()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return 0f;
            }

            return ratioMode == EScreenRatioMode.WidthHeight
                ? (float)Screen.width / Screen.height
                : (float)Screen.height / Screen.width;
        }
    }
}
