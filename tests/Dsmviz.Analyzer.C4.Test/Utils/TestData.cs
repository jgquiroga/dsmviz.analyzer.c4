namespace Dsmviz.Analyzer.C4.Test.Utils
{
    class TestData
    {
        public static string RootDirectory
        {
            get
            {
                string testData = "Examples";
                string pathExecutingAssembly = AppDomain.CurrentDomain.BaseDirectory;
                return Path.GetFullPath(Path.Combine(pathExecutingAssembly, testData));
            }
        }

        public const string TestWorkspace1 = "01_simple/workspace.json";

        public const string TestWorkspace2 = "workspace2.json";
    }
}
