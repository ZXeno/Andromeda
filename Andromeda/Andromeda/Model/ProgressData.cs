namespace Andromeda.Model
{
    public class ProgressData
    {
        public delegate void SetProgressBarTotal(int total);
        public static event SetProgressBarTotal SetTotal;
        public static void OnStartProgressBar(int total)
        {
            if (SetTotal != null)
            {
                SetTotal(total);
            }
        }

        public delegate void SetProgressBarCurrent(int current);
        public static event SetProgressBarCurrent SetChange;
        public static void OnUpdateProgressBar(int change)
        {
            if (SetChange != null)
            {
                SetChange(change);
            }
        }

        public int TotalCount { get; set; }
        public int Current { get; set; }

        public void Reset()
        {
            TotalCount = 0;
            Current = 0;
        }
    }
}