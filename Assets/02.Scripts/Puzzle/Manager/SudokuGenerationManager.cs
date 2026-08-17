using System.Collections.Generic;
using System.Threading.Tasks;
using Commons.Util;

namespace SUDOKU.Puzzle.Manager
{
    using Data;
    using Enum;
    using Generator;

    public class SudokuGenerationManager : Singleton<SudokuGenerationManager>
    {
        private readonly Dictionary<ESudokuDifficulty, SudokuPuzzleData> puzzleCache = new();
        private readonly object cacheLock = new();

        private SudokuPuzzleGenerator puzzleGenerator;

        #region Unity Callbacks

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                puzzleGenerator = new SudokuPuzzleGenerator();
            }
        }

        #endregion

        /// <summary>
        /// 요청한 난이도의 스도쿠 문제를 동기적으로 생성합니다.
        /// </summary>
        /// <param name="difficulty">생성할 스도쿠 난이도입니다.</param>
        /// <returns>생성된 문제와 정답 데이터입니다.</returns>
        public SudokuPuzzleData GeneratePuzzle(ESudokuDifficulty difficulty)
        {
            puzzleGenerator ??= new SudokuPuzzleGenerator();
            return puzzleGenerator.Generate(difficulty);
        }

        /// <summary>
        /// 요청한 난이도의 스도쿠 문제를 백그라운드 작업에서 생성합니다.
        /// </summary>
        /// <param name="difficulty">생성할 스도쿠 난이도입니다.</param>
        /// <returns>생성된 문제와 정답 데이터를 반환하는 Task입니다.</returns>
        public Task<SudokuPuzzleData> GeneratePuzzleAsync(ESudokuDifficulty difficulty)
        {
            return Task.Run(() =>
            {
                SudokuPuzzleGenerator backgroundGenerator = new();
                return backgroundGenerator.Generate(difficulty);
            });
        }

        /// <summary>
        /// 요청한 난이도의 문제를 백그라운드에서 미리 생성해 캐시에 저장합니다.
        /// </summary>
        /// <param name="difficulty">미리 생성할 스도쿠 난이도입니다.</param>
        public async Task CachePuzzleAsync(ESudokuDifficulty difficulty)
        {
            SudokuPuzzleData generatedPuzzle = await GeneratePuzzleAsync(difficulty);

            lock (cacheLock)
            {
                puzzleCache[difficulty] = generatedPuzzle;
            }
        }

        /// <summary>
        /// 캐시된 문제를 우선 반환하고 없으면 즉시 새 문제를 생성합니다.
        /// </summary>
        /// <param name="difficulty">가져올 스도쿠 난이도입니다.</param>
        /// <returns>캐시 또는 즉시 생성된 문제 데이터입니다.</returns>
        public SudokuPuzzleData GetOrGeneratePuzzle(ESudokuDifficulty difficulty)
        {
            lock (cacheLock)
            {
                if (puzzleCache.Remove(difficulty, out SudokuPuzzleData cachedPuzzle))
                {
                    return cachedPuzzle;
                }
            }

            return GeneratePuzzle(difficulty);
        }

        /// <summary>
        /// 지정한 난이도의 캐시된 문제가 있는지 확인합니다.
        /// </summary>
        /// <param name="difficulty">캐시 여부를 확인할 스도쿠 난이도입니다.</param>
        /// <returns>캐시된 문제가 있으면 true입니다.</returns>
        public bool HasCachedPuzzle(ESudokuDifficulty difficulty)
        {
            lock (cacheLock)
            {
                return puzzleCache.ContainsKey(difficulty);
            }
        }
    }
}
