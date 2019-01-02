using System;
using System.IO;
using System.Linq;

namespace Semmle.Util
{
    public static class FileUtils
    {
        public static string ConvertToWindows(string path)
        {
            return path.Replace('/', '\\');
        }

        public static string ConvertToUnix(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string ConvertToNative(string path)
        {
            return Path.DirectorySeparatorChar == '/' ?
                path.Replace('\\', '/') :
                path.Replace('/', '\\');
        }

        /// <summary>
        /// Moves the source file to the destination, overwriting the destination file if
        /// it exists already.
        /// </summary>
        /// <param name="src">Source file.</param>
        /// <param name="dest">Target file.</param>
        public static void MoveOrReplace(string src, string dest)
        {
            // Potential race condition here.
            // .net offers the facility to either move a file, or to replace it.
            File.Delete(dest);
            File.Move(src, dest);
        }

        /// <summary>
        /// Attempt to delete the given file (ignoring I/O exceptions).
        /// </summary>
        /// <param name="file">The file to delete.</param>
        public static void TryDelete(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // Ignore
            }
        }

        /// <summary>
        /// Finds the path for the executable <paramref name="exe"/> based on the
        /// <code>PATH</code> environment variable, and in the case of Windows the
        /// <code>PATHEXT</code> environment variable.
        /// 
        /// Returns <code>null</code> of no path can be found.
        /// </summary>
        public static string FindExecutableOnPath(string exe)
        {
            var paths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);
            string[] exes;
            if (Win32.IsWindows())
            {
                var extensions = Environment.GetEnvironmentVariable("PATHEXT").Split(';').ToArray();
                exes = extensions.Any(exe.EndsWith) ? new[] { exe } : extensions.Select(ext => exe + ext).ToArray();
            }
            else
            {
                exes = new[] { exe };
            }
            var candidates = paths.Where(path => exes.Any(exe0 => File.Exists(Path.Combine(path, exe0))));
            return candidates.FirstOrDefault();
        }
    }
}
