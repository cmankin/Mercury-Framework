using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StubsTutorial
{
    public static class FileSystem
    {
        public static string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }
}