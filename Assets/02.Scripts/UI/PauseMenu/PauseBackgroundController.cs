using UnityEngine;

namespace SUDOKU.UI.PauseMenu
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseBackgroundController : MonoBehaviour
    {
        [Tooltip("Pause 상태에서 배경에 적용할 Alpha 값입니다.")]
        [SerializeField, Range(0f, 1f)] private float pausedAlpha = 0.3f;

        private CanvasGroup backgroundCanvasGroup;

        #region Unity Callbacks

        private void Awake()
        {
            TryGetComponent(out backgroundCanvasGroup);
        }

        private void Start()
        {
            SetPauseState(false);
        }

        private void OnDisable()
        {
            SetPauseState(false);
        }

        private void OnValidate()
        {
            pausedAlpha = Mathf.Clamp01(pausedAlpha);
        }

        #endregion

        /// <summary>
        /// Pause 상태에 따라 CanvasGroup Alpha와 입력 차단 상태를 적용합니다.
        /// </summary>
        /// <param name="isPaused">배경을 표시하고 뒤쪽 입력을 차단해야 하면 true입니다.</param>
        public void SetPauseState(bool isPaused)
        {
            if (backgroundCanvasGroup == null)
            {
                TryGetComponent(out backgroundCanvasGroup);
            }

            if (backgroundCanvasGroup == null)
            {
                Debug.LogError("Pause Background에 CanvasGroup이 필요합니다.", this);
                return;
            }

            backgroundCanvasGroup.alpha = isPaused ? pausedAlpha : 0f;
            backgroundCanvasGroup.interactable = isPaused;
            backgroundCanvasGroup.blocksRaycasts = isPaused;
        }
    }
}
