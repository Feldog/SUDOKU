using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SUDOKU.UI.PanelScale
{
    public class UIMatchController : MonoBehaviour
    {
        [Tooltip("화면 비율에 따라 Match 값을 변경할 UI Document입니다. Canvas를 사용할 때는 비워둡니다.")]
        [SerializeField] private UIDocument uiDocument;

        [Tooltip("화면 비율에 따라 Match 값을 변경할 UGUI Canvas입니다. UI Document를 사용할 때는 비워둡니다.")]
        [SerializeField] private Canvas uiCanvas;

        [Tooltip("Width Match에서 Height Match로 전환할 Width / Height 기준값입니다.")]
        [SerializeField] private float heightMatchThreshold = 0.6f;

        private PanelSettings panelSettings;
        private CanvasScaler canvasScaler;
        private int previousScreenWidth = -1;
        private int previousScreenHeight = -1;

        // UI Document 또는 Canvas 참조 초기화가 정상적으로 완료되었는지 나타냅니다.
        private bool isInitialized;

        #region Unity Callbacks

        private void OnEnable()
        {
            InitializeReferences();

            if (isInitialized)
            {
                RefreshMatchIfResolutionChanged();
            }
        }

        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            RefreshMatchIfResolutionChanged();
        }

        private void OnDisable()
        {
            isInitialized = false;
        }

        #endregion

        /// <summary>
        /// 연결된 UI Document 또는 Canvas에서 Match 대상 참조를 초기화합니다.
        /// </summary>
        private void InitializeReferences()
        {
            isInitialized = false;
            panelSettings = null;
            canvasScaler = null;
            previousScreenWidth = -1;
            previousScreenHeight = -1;

            if (uiDocument != null)
            {
                panelSettings = uiDocument.panelSettings;

                if (panelSettings != null)
                {
                    isInitialized = true;
                    return;
                }

                Debug.LogError("UI Document에 Panel Settings가 연결되지 않았습니다.", this);
            }

            if (uiCanvas == null)
            {
                Debug.LogError("Match 값을 적용할 UI Document 또는 Canvas가 연결되지 않았습니다.", this);
                return;
            }

            if (!uiCanvas.TryGetComponent(out canvasScaler))
            {
                Debug.LogError("연결된 Canvas에서 Canvas Scaler를 찾을 수 없습니다.", this);
                return;
            }

            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                Debug.LogError("Canvas Scaler의 UI Scale Mode가 Scale With Screen Size가 아닙니다.", this);
                canvasScaler = null;
                return;
            }

            isInitialized = true;
        }

        /// <summary>
        /// 화면 해상도가 변경된 경우에만 현재 UI 대상의 Match 값을 다시 계산합니다.
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
        /// 현재 화면 비율에 따라 UI Document 또는 Canvas의 Width/Height Match 값을 적용합니다.
        /// </summary>
        /// <param name="screenWidth">현재 화면의 픽셀 너비입니다.</param>
        /// <param name="screenHeight">현재 화면의 픽셀 높이입니다.</param>
        private void ApplyScreenMatch(int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            float screenAspect = (float)screenWidth / screenHeight;
            float matchValue = screenAspect <= heightMatchThreshold ? 0f : 1f;

            if (panelSettings != null)
            {
                panelSettings.match = matchValue;
                return;
            }

            if (canvasScaler != null)
            {
                canvasScaler.matchWidthOrHeight = matchValue;
            }
        }
    }
}
