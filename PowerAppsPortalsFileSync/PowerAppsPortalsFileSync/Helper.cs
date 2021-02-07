using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync
{
    public static class Helper
    {
        public static string CreateFolder(string path, string folder)
        {
            path = Path.Combine(path.Trim(), folder.Trim());

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        // https://stackoverflow.com/a/23182807
        public static string ReplaceInvalidChars(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty", nameof(filename));
            }

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars())).Trim();
        }

        // https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net
        public static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}
