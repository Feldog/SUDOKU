using UnityEngine;
using UnityEngine.UIElements;

namespace SUDOKU.UI.MainMenu
{
    using GameStartMenu;
    using OptionMenu;

    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("Game Start, Option, Exit 버튼을 포함한 Main Menu UI Document입니다.")]
        [SerializeField] private UIDocument mainMenuDocument;

        [Tooltip("난이도 선택 창의 기능과 표시 상태를 관리할 Controller입니다.")]
        [SerializeField] private GameStartMenuController gameStartMenuController;

        [Tooltip("Option 창의 기능과 표시 상태를 관리할 Controller입니다.")]
        [SerializeField] private OptionMenuController optionMenuController;

        private Button gameStartButton;
        private Button optionButton;
        private Button exitButton;
        private bool callbacksRegistered;
        private bool hasStarted;

        #region Unity Callbacks

        private void Start()
        {
            hasStarted = true;
            RegisterCallbacks();
            ShowMainMenu();
        }

        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterCallbacks();
                ShowMainMenu();
            }
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        #endregion

        /// <summary>
        /// Main Menu 버튼과 하위 메뉴 이벤트를 연결합니다.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (callbacksRegistered || !CacheButtons())
            {
                return;
            }

            gameStartButton.clicked += ShowGameStartMenu;
            optionButton.clicked += ShowOptionMenu;
            exitButton.clicked += ExitGame;
            gameStartMenuController.ReturnRequested += ShowMainMenu;
            optionMenuController.ReturnRequested += ShowMainMenu;
            callbacksRegistered = true;
        }

        /// <summary>
        /// Main Menu 버튼과 하위 메뉴 이벤트 연결을 해제합니다.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            gameStartButton.clicked -= ShowGameStartMenu;
            optionButton.clicked -= ShowOptionMenu;
            exitButton.clicked -= ExitGame;
            gameStartMenuController.ReturnRequested -= ShowMainMenu;
            optionMenuController.ReturnRequested -= ShowMainMenu;
            callbacksRegistered = false;
        }

        /// <summary>
        /// Main Menu UI Document에서 세 버튼을 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 참조와 버튼을 모두 찾았으면 true입니다.</returns>
        private bool CacheButtons()
        {
            if (mainMenuDocument == null || gameStartMenuController == null || optionMenuController == null)
            {
                Debug.LogError("Main Menu Controller에 UI Document와 하위 메뉴 Controller를 연결해야 합니다.", this);
                return false;
            }

            VisualElement root = mainMenuDocument.rootVisualElement;
            gameStartButton = root.Q<Button>("game-start-button");
            optionButton = root.Q<Button>("option-button");
            exitButton = root.Q<Button>("exit-button");

            if (gameStartButton == null || optionButton == null || exitButton == null)
            {
                Debug.LogError("Main Menu에서 Game Start, Option 또는 Exit 버튼을 찾을 수 없습니다.", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Main Menu를 표시하고 두 하위 메뉴를 숨깁니다.
        /// </summary>
        public void ShowMainMenu()
        {
            SetMainMenuVisibility(true);
            gameStartMenuController?.Hide();
            optionMenuController?.Hide();
        }

        /// <summary>
        /// Main Menu를 숨기고 난이도 선택 창을 표시합니다.
        /// </summary>
        private void ShowGameStartMenu()
        {
            SetMainMenuVisibility(false);
            optionMenuController.Hide();
            gameStartMenuController.Show();
        }

        /// <summary>
        /// Main Menu를 숨기고 Option 창을 표시합니다.
        /// </summary>
        private void ShowOptionMenu()
        {
            SetMainMenuVisibility(false);
            gameStartMenuController.Hide();
            optionMenuController.Show();
        }

        /// <summary>
        /// 실행 중인 애플리케이션의 종료를 요청합니다.
        /// </summary>
        private void ExitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Main Menu UI Document의 표시 상태를 변경합니다.
        /// </summary>
        /// <param name="shouldShow">Main Menu를 표시해야 하면 true입니다.</param>
        private void SetMainMenuVisibility(bool shouldShow)
        {
            if (mainMenuDocument == null)
            {
                return;
            }

            mainMenuDocument.rootVisualElement.style.display = shouldShow
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
