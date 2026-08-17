using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.OptionMenu
{
    public class OptionMenuController : MonoBehaviour
    {
        [Tooltip("Sound와 Help 설정 및 Return 버튼을 포함한 Option UI Document입니다.")]
        [SerializeField] private UIDocument optionMenuDocument;

        [Tooltip("Option Menu가 시작할 때 적용할 Sound 값입니다.")]
        [SerializeField, Range(0f, 100f)] private float soundVolume = 100f;

        [Tooltip("Cell 인접 영역 Help 기능의 초기 활성 상태입니다.")]
        [SerializeField] private bool isAreaHelpEnabled = true;

        [Tooltip("같은 값 Help 기능의 초기 활성 상태입니다.")]
        [SerializeField] private bool isSameValueHelpEnabled = true;

        private Slider soundSlider;
        private Button areaHelpButton;
        private Button sameValueHelpButton;
        private Button returnButton;
        private Label areaHelpStateLabel;
        private Label sameValueHelpStateLabel;
        private bool callbacksRegistered;
        private bool hasStarted;

        public event Action<float> SoundVolumeChanged;
        public event Action<bool> AreaHelpStateChanged;
        public event Action<bool> SameValueHelpStateChanged;
        public event Action ReturnRequested;

        public float SoundVolume => soundVolume;
        public bool IsAreaHelpEnabled => isAreaHelpEnabled;
        public bool IsSameValueHelpEnabled => isSameValueHelpEnabled;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            RefreshView();
            Hide();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
                RefreshView();
            }
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        #endregion

        /// <summary>
        /// Option Menu를 표시하고 현재 설정값을 UI에 반영합니다.
        /// </summary>
        public void Show()
        {
            RefreshView();
            SetVisibility(true);
        }

        /// <summary>
        /// Option Menu를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            SetVisibility(false);
        }

        /// <summary>
        /// Option UI의 Slider와 버튼 입력 이벤트를 연결합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheVisualElements())
            {
                return;
            }

            soundSlider.RegisterValueChangedCallback(OnSoundValueChanged);
            areaHelpButton.clicked += ToggleAreaHelp;
            sameValueHelpButton.clicked += ToggleSameValueHelp;
            returnButton.clicked += RequestReturn;
            callbacksRegistered = true;
        }

        /// <summary>
        /// Option UI의 Slider와 버튼 입력 이벤트 연결을 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            soundSlider.UnregisterValueChangedCallback(OnSoundValueChanged);
            areaHelpButton.clicked -= ToggleAreaHelp;
            sameValueHelpButton.clicked -= ToggleSameValueHelp;
            returnButton.clicked -= RequestReturn;
            callbacksRegistered = false;
        }

        /// <summary>
        /// Option UI Document에서 Slider, 상태 버튼과 Label을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 UI 요소를 찾았으면 true입니다.</returns>
        private bool CacheVisualElements()
        {
            if (optionMenuDocument == null)
            {
                Debug.LogError("Option Menu UI Document가 연결되지 않았습니다.", this);
                return false;
            }

            VisualElement root = optionMenuDocument.rootVisualElement;
            soundSlider = root.Q<Slider>("sound-slider");
            areaHelpButton = root.Q<Button>("area-help-button");
            sameValueHelpButton = root.Q<Button>("same-value-help-button");
            returnButton = root.Q<Button>("return-button");
            areaHelpStateLabel = root.Q<Label>("area-help-state-label");
            sameValueHelpStateLabel = root.Q<Label>("same-value-help-state-label");

            return soundSlider != null
                && areaHelpButton != null
                && sameValueHelpButton != null
                && returnButton != null
                && areaHelpStateLabel != null
                && sameValueHelpStateLabel != null;
        }

        /// <summary>
        /// 현재 Option 데이터를 Slider와 On/Off Label에 반영합니다.
        /// </summary>
        private void RefreshView()
        {
            if (!CacheVisualElements())
            {
                return;
            }

            soundSlider.SetValueWithoutNotify(soundVolume);
            areaHelpStateLabel.text = isAreaHelpEnabled ? "On" : "Off";
            sameValueHelpStateLabel.text = isSameValueHelpEnabled ? "On" : "Off";
        }

        /// <summary>
        /// Slider 입력을 Sound 값에 저장하고 변경 이벤트를 전달합니다.
        /// </summary>
        /// <param name="changeEvent">변경된 Slider 값을 포함한 이벤트입니다.</param>
        private void OnSoundValueChanged(ChangeEvent<float> changeEvent)
        {
            soundVolume = changeEvent.newValue;
            SoundVolumeChanged?.Invoke(soundVolume);
        }

        /// <summary>
        /// Cell 인접 영역 Help 상태를 전환하고 View와 이벤트를 갱신합니다.
        /// </summary>
        private void ToggleAreaHelp()
        {
            isAreaHelpEnabled = !isAreaHelpEnabled;
            areaHelpStateLabel.text = isAreaHelpEnabled ? "On" : "Off";
            AreaHelpStateChanged?.Invoke(isAreaHelpEnabled);
        }

        /// <summary>
        /// 같은 값 Help 상태를 전환하고 View와 이벤트를 갱신합니다.
        /// </summary>
        private void ToggleSameValueHelp()
        {
            isSameValueHelpEnabled = !isSameValueHelpEnabled;
            sameValueHelpStateLabel.text = isSameValueHelpEnabled ? "On" : "Off";
            SameValueHelpStateChanged?.Invoke(isSameValueHelpEnabled);
        }

        /// <summary>
        /// 이전 메뉴로 돌아가기 위한 요청을 전달합니다.
        /// </summary>
        private void RequestReturn()
        {
            ReturnRequested?.Invoke();
        }

        /// <summary>
        /// Option UI Document의 표시 상태를 변경합니다.
        /// </summary>
        /// <param name="shouldShow">Option Menu를 표시해야 하면 true입니다.</param>
        private void SetVisibility(bool shouldShow)
        {
            if (optionMenuDocument == null)
            {
                return;
            }

            optionMenuDocument.rootVisualElement.style.display = shouldShow
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
