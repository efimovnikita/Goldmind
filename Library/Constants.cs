namespace Library
{
    public static class Constants
    {
        public static string DbPath => Path.Combine(Path.GetTempPath(), "goldmind.db");
        public static string CollectionName => "headlists";
        public static int DaysBetweenDistillations = 15;
        public static int ExpressionsPerHeadlist = 14;
    }
}