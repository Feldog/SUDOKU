using System;
using Commons.Util;
using UnityEngine;

namespace SUDOKU.Manager
{
    public class OptionManager : Singleton<OptionManager>
    {
        public const float DefaultSoundVolume = 100f;
        public const bool DefaultAreaHelpEnabled = true;
        public const bool DefaultSameValueHelpEnabled = true;

        private const string SoundVolumeKey = "OPTION.SOUND_VOLUME";
        private const string AreaHelpEnabledKey = "OPTION.AREA_HELP_ENABLED";
        private const string SameValueHelpEnabledKey = "OPTION.SAME_VALUE_HELP_ENABLED";

        private float soundVolume = DefaultSoundVolume;
        private bool isAreaHelpEnabled = DefaultAreaHelpEnabled;
        private bool isSameValueHelpEnabled = DefaultSameValueHelpEnabled;

        public event Action<float> SoundVolumeChanged;
        public event Action<bool> AreaHelpStateChanged;
        public event Action<bool> SameValueHelpStateChanged;

        public float SoundVolume => soundVolume;
        public bool IsAreaHelpEnabled => isAreaHelpEnabled;
        public bool IsSameValueHelpEnabled => isSameValueHelpEnabled;

        #region Unity Callbacks

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                LoadOptions();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                FlushPlayerPrefs();
            }
        }

        private void OnApplicationQuit()
        {
            FlushPlayerPrefs();
        }

        #endregion

        #region Option Mutation

        /// <summary>
        /// Option UI의 현재 값을 비교해 변경된 데이터만 적용하고 PlayerPrefs에 저장합니다.
        /// </summary>
        /// <param name="newSoundVolume">저장할 0~100 Sound 값입니다.</param>
        /// <param name="newAreaHelpEnabled">Cell 인접 영역 Help 활성 상태입니다.</param>
        /// <param name="newSameValueHelpEnabled">같은 값 Help 활성 상태입니다.</param>
        /// <returns>기존 데이터와 달라 실제 저장이 실행되었으면 true입니다.</returns>
        public bool SaveOptions(
            float newSoundVolume,
            bool newAreaHelpEnabled,
            bool newSameValueHelpEnabled)
        {
            float clampedVolume = Mathf.Clamp(newSoundVolume, 0f, 100f);
            bool hasChanged = !Mathf.Approximately(soundVolume, clampedVolume)
                || isAreaHelpEnabled != newAreaHelpEnabled
                || isSameValueHelpEnabled != newSameValueHelpEnabled;

            if (!hasChanged)
            {
                return false;
            }

            SetSoundVolume(clampedVolume);
            SetAreaHelpEnabled(newAreaHelpEnabled);
            SetSameValueHelpEnabled(newSameValueHelpEnabled);
            FlushPlayerPrefs();
            return true;
        }

        /// <summary>
        /// Sound 값을 0~100 범위로 저장하고 실제 Audio Listener 음량에 적용합니다.
        /// </summary>
        /// <param name="newVolume">저장할 Sound 값입니다.</param>
        public void SetSoundVolume(float newVolume)
        {
            float clampedVolume = Mathf.Clamp(newVolume, 0f, 100f);

            if (Mathf.Approximately(soundVolume, clampedVolume))
            {
                return;
            }

            soundVolume = clampedVolume;
            PlayerPrefs.SetFloat(SoundVolumeKey, soundVolume);
            ApplySoundVolume();
            SoundVolumeChanged?.Invoke(soundVolume);
        }

        /// <summary>
        /// Cell 인접 영역 Help 활성 상태를 저장합니다.
        /// </summary>
        /// <param name="isEnabled">Help 기능을 활성화하려면 true입니다.</param>
        public void SetAreaHelpEnabled(bool isEnabled)
        {
            if (isAreaHelpEnabled == isEnabled)
            {
                return;
            }

            isAreaHelpEnabled = isEnabled;
            PlayerPrefs.SetInt(AreaHelpEnabledKey, isAreaHelpEnabled ? 1 : 0);
            AreaHelpStateChanged?.Invoke(isAreaHelpEnabled);
        }

        /// <summary>
        /// 같은 값 Help 활성 상태를 저장합니다.
        /// </summary>
        /// <param name="isEnabled">같은 값 강조 기능을 활성화하려면 true입니다.</param>
        public void SetSameValueHelpEnabled(bool isEnabled)
        {
            if (isSameValueHelpEnabled == isEnabled)
            {
                return;
            }

            isSameValueHelpEnabled = isEnabled;
            PlayerPrefs.SetInt(SameValueHelpEnabledKey, isSameValueHelpEnabled ? 1 : 0);
            SameValueHelpStateChanged?.Invoke(isSameValueHelpEnabled);
        }

        #endregion

        #region Persistence

        /// <summary>
        /// PlayerPrefs에서 Option 데이터를 불러오고 저장된 값이 없으면 기본값을 사용합니다.
        /// </summary>
        private void LoadOptions()
        {
            soundVolume = Mathf.Clamp(
                PlayerPrefs.GetFloat(SoundVolumeKey, DefaultSoundVolume),
                0f,
                100f);
            isAreaHelpEnabled = PlayerPrefs.GetInt(
                AreaHelpEnabledKey,
                DefaultAreaHelpEnabled ? 1 : 0) != 0;
            isSameValueHelpEnabled = PlayerPrefs.GetInt(
                SameValueHelpEnabledKey,
                DefaultSameValueHelpEnabled ? 1 : 0) != 0;

            ApplySoundVolume();
        }

        /// <summary>
        /// 메모리에 변경된 PlayerPrefs Option 데이터를 저장 장치에 기록합니다.
        /// </summary>
        private static void FlushPlayerPrefs()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 0~100 Sound 값을 Unity의 0~1 전체 음량 값으로 변환해 적용합니다.
        /// </summary>
        private void ApplySoundVolume()
        {
            AudioListener.volume = soundVolume / 100f;
        }

        #endregion
    }
}
