namespace NetScript.Desktop
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (Path.GetDirectoryName(args.First()) is string dir)
            {
                Directory.SetCurrentDirectory(dir);
            }
            NS.Run((from a in args select File.ReadAllText(a)).ToArray());
        }
    }
}