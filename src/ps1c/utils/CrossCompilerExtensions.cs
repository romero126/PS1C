using System;
using System.IO;
using PS1C;
// This is used to add or extend class objects with additional functionality to bring parity with newer .net frameworks



namespace PS1C
{
    public static class CrossFrameworkExtensions
    {
        public static char[] Slice(this char[] source, int start, int length)
        {
            char[] result = new char[length];
            Array.Copy(source, start, result, 0, length);
            return result;
        }

        public static bool IsNullOrWhiteSpace(this String value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool Contains (this String source, string value, StringComparison comparisonType)
        {
            if (source == null)
            {
                return false;
            }
            return source.IndexOf(value, comparisonType) >= 0;
        }

        public static bool Contains (this String source, char value, StringComparison comparisonType)
        {
            if (source == null)
            {
                return false;
            }
            return source.IndexOf(value) >= 0;
        }

        public static bool Contains (this String source, char[] value)
        {
            if (source == null)
            {
                return false;
            }
            return source.IndexOf(value.ToString()) >= 0;
        }

        public static bool EndsWith (this String source, char value)
        {
            if (source == null || source.Length == 0)
            {
                return false;
            }
            return source[source.Length - 1] == value;
        }
        
    }
    public static class PathUtils
    {
        public static bool EndsInDirectorySeparator(string path)
        {
            if (path.EndsWith(Path.AltDirectorySeparatorChar))
                return true;
            if (path.EndsWith(Path.DirectorySeparatorChar))
                return true;
            return false;
        }
        public static string TrimEndingDirectorySeparator(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar);
            return path;
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            if (string.IsNullOrEmpty(relativeTo))
            {
                throw new ArgumentNullException("relativeTo", "The path is empty. (Parameter 'relativeTo')");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path", "The path is empty. (Parameter 'path')");
            }

            string p = Path.GetFullPath(path);
            string r = Path.GetFullPath(relativeTo);

            if (p == r)
            {
                return ".";
            }

            if (p.StartsWith(r))
            {
                return p.Substring(r.Length);
            }

            p = p.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // Remove leading / if it exists
            if (p.StartsWith("\\") || p.StartsWith("/"))
            {
                p = p.Substring(1, p.Length - 1);
            }
            
            // Should return a ../.. for each directory in the relative path
            string[] pParts = p.Split(Path.DirectorySeparatorChar);
            
            for (int i = 0; i < pParts.Length; i++)
            {
                pParts[i] = "..";
            }

            return String.Join(Path.DirectorySeparatorChar.ToString(), pParts);
        }

        public static string GetFullPath(string path, string basePath)
        {
            return Path.GetFullPath(Path.Combine(basePath, path));
        }

        public static string Join(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }
    }

}