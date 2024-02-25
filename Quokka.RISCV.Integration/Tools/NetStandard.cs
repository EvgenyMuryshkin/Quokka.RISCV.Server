using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Linq
{
    internal static class NetStandardExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static String[] Split(this string source, string separator)
        {
            return source.Split(separator.ToArray());
        }
    }
}

namespace System
{
    internal static class StringExtensions
    {
        public static void DeleteFileIfExists(this string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        public static void DeleteDirectoryIfExists(this string directoryName)
        {
            if (Directory.Exists(directoryName))
                Directory.Delete(directoryName, true);
        }
    }
}

namespace System
{
    using ICSharpCode.SharpZipLib.Zip;
    using System.Threading.Tasks;

    internal static class ZipExtensions
    {
        public static void CreateFromStream(this string fileName, Stream source)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                source.CopyTo(fs);
            }
        }

        public static async Task CreateFromStreamAsync(this string fileName, Stream source)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                await source.CopyToAsync(fs);
            }
        }

        public static void ExtractZip(this string zipFilePath, string extractTo)
        {
            if (!Directory.Exists(extractTo))
                Directory.CreateDirectory(extractTo);

            Exception ex = null;

            for (var i = 1; i < 5; i++)
            {
                try
                {
                    var zip = new FastZip();
                    zip.ExtractZip(zipFilePath, extractTo, ".*");
                    return;
                }
                catch (IOException ioex)
                {
                    ex = ioex;
                    Task.Delay(TimeSpan.FromMilliseconds(500 * i)).Wait();
                }
            }

            if (ex != null)
                throw ex;
        }

        public static void CompressFolder(this string directoryPath, string zipFileName)
        {
            var zfe = new ZipEntryFactory { IsUnicodeText = true };
            var fastZip = new FastZip() { EntryFactory = zfe };
            fastZip.CreateZip(zipFileName, directoryPath, true, null);
        }
    }
}