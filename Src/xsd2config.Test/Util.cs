using System.IO;
using System.Reflection;

namespace xsd2config.Test
{
    internal static class Util
    {
        internal static readonly string CurrentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static string GetPathInCurrentAssembly(string fileName)
        {
            return Path.Combine(CurrentAssemblyPath, fileName);
        }
    }
}
