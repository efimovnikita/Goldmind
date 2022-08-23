namespace Library
{
    public static class Constants
    {
        public const string DbFilename = "goldmind.db";
        public static string DbPath => Path.Combine(Path.GetTempPath(), DbFilename);
        public static string CollectionName => "headlists";
        public static int DaysBetweenDistillations = 15;
        public static int ExpressionsPerHeadlist = 14;
    }
}