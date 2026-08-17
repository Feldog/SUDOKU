namespace SUDOKU.Puzzle.Define
{
    public static class SudokuDifficultyDefine
    {
        public const int EasyTargetClueCount = 40;
        public const int NormalTargetClueCount = 34;
        public const int HardTargetClueCount = 28;
        public const int ExtremeTargetClueCount = 24;
        public const int MaxGenerationAttempts = 8;

        public const int NakedSingleScore = 1;
        public const int HiddenSingleScore = 3;
        public const int LockedCandidateScore = 5;
        public const int NakedPairScore = 8;
        public const int GuessingScore = 100;
        public const int EasyMaxScore = 45;
        public const int NormalMaxScore = 90;
    }
}
