using Commons.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SUDOKU.Manager
{
    using Puzzle.Enum;

    public class GameManager : Singleton<GameManager>
    {
        [Tooltip("현재 게임에서 사용할 Sudoku 난이도입니다.")]
        [SerializeField] private ESudokuDifficulty difficulty = ESudokuDifficulty.Normal;

        [Tooltip("난이도 저장이 완료된 후 전환할 게임 Scene 이름입니다.")]
        [SerializeField] private string gameSceneName = "02. GameScene";

        [Tooltip("게임 종료 후 돌아갈 Main Menu Scene 이름입니다.")]
        [SerializeField] private string mainMenuSceneName = "01. MainmenuScene";

        private bool isSceneLoading;

        public ESudokuDifficulty Difficulty => difficulty;

        /// <summary>
        /// 다음 Sudoku 게임에서 사용할 난이도를 저장합니다.
        /// </summary>
        /// <param name="newDifficulty">GameManager에 저장할 Sudoku 난이도입니다.</param>
        public void SetDifficulty(ESudokuDifficulty newDifficulty)
        {
            difficulty = newDifficulty;
        }

        /// <summary>
        /// 선택한 난이도를 저장하고 설정된 게임 Scene으로 전환합니다.
        /// </summary>
        /// <param name="selectedDifficulty">새 게임에 적용할 Sudoku 난이도입니다.</param>
        /// <returns>난이도 저장과 Scene 전환 요청이 정상적으로 처리되었으면 true입니다.</returns>
        public bool StartGame(ESudokuDifficulty selectedDifficulty)
        {
            if (!CanLoadScene(gameSceneName))
            {
                return false;
            }

            SetDifficulty(selectedDifficulty);

            if (difficulty != selectedDifficulty)
            {
                Debug.LogError("선택한 Sudoku 난이도가 정상적으로 저장되지 않았습니다.", this);
                return false;
            }

            return LoadScene(gameSceneName);
        }

        /// <summary>
        /// 설정된 Main Menu Scene으로 전환합니다.
        /// </summary>
        /// <returns>Main Menu Scene 전환 요청이 정상적으로 처리되었으면 true입니다.</returns>
        public bool ReturnToMainMenu()
        {
            return CanLoadScene(mainMenuSceneName) && LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// 현재 저장된 난이도를 유지한 채 설정된 게임 Scene을 다시 불러옵니다.
        /// </summary>
        /// <returns>게임 Scene 재시작 요청이 정상적으로 처리되었으면 true입니다.</returns>
        public bool RestartGame()
        {
            return CanLoadScene(gameSceneName) && LoadScene(gameSceneName);
        }

        /// <summary>
        /// Scene 이름과 Build Settings 등록 상태를 확인합니다.
        /// </summary>
        /// <param name="sceneName">로드 가능 여부를 확인할 Scene 이름입니다.</param>
        /// <returns>Scene 전환을 요청할 수 있으면 true입니다.</returns>
        private bool CanLoadScene(string sceneName)
        {
            if (isSceneLoading)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(sceneName)
                && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return true;
            }

            Debug.LogError($"전환할 Scene을 Build Settings에서 찾을 수 없습니다: {sceneName}", this);
            return false;
        }

        /// <summary>
        /// 중복 요청을 차단한 상태로 지정한 Scene을 동기 로드합니다.
        /// </summary>
        /// <param name="sceneName">전환할 Scene 이름입니다.</param>
        /// <returns>Scene 전환 요청이 처리되었으면 true입니다.</returns>
        private bool LoadScene(string sceneName)
        {
            isSceneLoading = true;

            try
            {
                SceneManager.LoadScene(sceneName);
                return true;
            }
            finally
            {
                isSceneLoading = false;
            }
        }
    }
}
