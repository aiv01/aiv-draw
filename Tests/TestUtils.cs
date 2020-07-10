using System;
using System.IO;

namespace Tests
{
    class TestUtil
    {
        public static string FullPath(string relPath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relPath);
        }
    }
}
