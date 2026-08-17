using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.GameStartMenu
{
    using Manager;
    using Puzzle.Enum;

    public class GameStartMenuController : MonoBehaviour
    {
        [Tooltip("난이도 버튼과 Return 버튼을 포함한 Game Start UI Document입니다.")]
        [SerializeField] private UIDocument gameStartMenuDocument;

        private Button easyButton;
        private Button normalButton;
        private Button hardButton;
        private Button extremeButton;
        private Button returnButton;
        private bool callbacksRegistered;
        private bool hasStarted;

        public event Action ReturnRequested;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            Hide();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
            }
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        #endregion

        /// <summary>
        /// Game Start Menu를 표시합니다.
        /// </summary>
        public void Show()
        {
            SetVisibility(true);
        }

        /// <summary>
        /// Game Start Menu를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            SetVisibility(false);
        }

        /// <summary>
        /// 난이도 버튼과 Return 버튼의 입력 이벤트를 연결합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheButtons())
            {
                return;
            }

            easyButton.clicked += SelectEasy;
            normalButton.clicked += SelectNormal;
            hardButton.clicked += SelectHard;
            extremeButton.clicked += SelectExtreme;
            returnButton.clicked += RequestReturn;
            callbacksRegistered = true;
        }

        /// <summary>
        /// 난이도 버튼과 Return 버튼의 입력 이벤트 연결을 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            easyButton.clicked -= SelectEasy;
            normalButton.clicked -= SelectNormal;
            hardButton.clicked -= SelectHard;
            extremeButton.clicked -= SelectExtreme;
            returnButton.clicked -= RequestReturn;
            callbacksRegistered = false;
        }

        /// <summary>
        /// Game Start UI Document에서 난이도와 Return 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 버튼을 찾았으면 true입니다.</returns>
        private bool CacheButtons()
        {
            if (gameStartMenuDocument == null)
            {
                Debug.LogError("Game Start Menu UI Document가 연결되지 않았습니다.", this);
                return false;
            }

            VisualElement root = gameStartMenuDocument.rootVisualElement;
            easyButton = root.Q<Button>("easy-button");
            normalButton = root.Q<Button>("normal-button");
            hardButton = root.Q<Button>("hard-button");
            extremeButton = root.Q<Button>("extreme-button");
            returnButton = root.Q<Button>("return-button");

            return easyButton != null
                && normalButton != null
                && hardButton != null
                && extremeButton != null
                && returnButton != null;
        }

        /// <summary>
        /// Easy 난이도를 선택합니다.
        /// </summary>
        private void SelectEasy()
        {
            SelectDifficulty(ESudokuDifficulty.Easy);
        }

        /// <summary>
        /// Normal 난이도를 선택합니다.
        /// </summary>
        private void SelectNormal()
        {
            SelectDifficulty(ESudokuDifficulty.Normal);
        }

        /// <summary>
        /// Hard 난이도를 선택합니다.
        /// </summary>
        private void SelectHard()
        {
            SelectDifficulty(ESudokuDifficulty.Hard);
        }

        /// <summary>
        /// Extreme 난이도를 선택합니다.
        /// </summary>
        private void SelectExtreme()
        {
            SelectDifficulty(ESudokuDifficulty.Extreme);
        }

        /// <summary>
        /// 난이도를 GameManager에 저장하고 게임 시작을 요청합니다.
        /// </summary>
        /// <param name="difficulty">플레이어가 선택한 Sudoku 난이도입니다.</param>
        private void SelectDifficulty(ESudokuDifficulty difficulty)
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                Debug.LogError("선택한 난이도를 저장할 GameManager를 찾을 수 없습니다.", this);
                return;
            }

            gameManager.StartGame(difficulty);
        }

        /// <summary>
        /// Main Menu로 돌아가기 위한 요청을 전달합니다.
        /// </summary>
        private void RequestReturn()
        {
            ReturnRequested?.Invoke();
        }

        /// <summary>
        /// Game Start UI Document의 표시 상태를 변경합니다.
        /// </summary>
        /// <param name="shouldShow">Game Start Menu를 표시해야 하면 true입니다.</param>
        private void SetVisibility(bool shouldShow)
        {
            if (gameStartMenuDocument == null)
            {
                return;
            }

            gameStartMenuDocument.rootVisualElement.style.display = shouldShow
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
