using System;
using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.IO;

using System.IO.Compression;

using System.Management.Automation;
using System.Management.Automation.Provider;

using PS1C.Archive;


namespace PS1C
{
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        #region NavigationCmdletProvider


		protected override bool IsItemContainer(string path)
		{
            path = NormalizePath(path);
            // WriteWarning($"[TODO] IsItemContainer(string path) '{path}'");

            if ( path == "" )
            {
                return true;
            }
			return false;
		}

		protected override void GetChildItems(string path, bool recurse)
		{
            path = NormalizePath(path);

            if (String.IsNullOrEmpty(path))
            {
                path = "*";
            }
            // WriteWarning($"[TODO] GetChildItems(string path) '{path}'");            

            List<ZipFileItemInfo> results = ZipFileItemInfo.GetFileItemInfo(PSDriveInfo, path);
            foreach (ZipFileItemInfo entry in results)
            {
                WriteItemObject(entry, entry.FullName, false);
            }
		}

        internal ZipFileItemInfo GetItemHelper(string path)
		{
            path = NormalizePath(path);
            
            return new ZipFileItemInfo(PSDriveInfo, path);
		}


        internal ZipFileItemInfo NewItemHelper(string path)
        {

            try {
                using (ZipArchive zipArchive = ZipFile.Open(PSDriveInfo.Root, ZipArchiveMode.Update))
                {
                    path = NormalizePath(path);
                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(path);

                    ZipFileItemInfo zipFileItemInfo = new ZipFileItemInfo(zipArchiveEntry, PSDriveInfo);

                    return zipFileItemInfo;
                    
                }
            }
            catch (Exception e) {
                throw e;
            }
        }
        private string NormalizePath(string path)
        {


            if (path.StartsWith(PSDriveInfo.Root))
            {
                path = path.Remove(0, PSDriveInfo.Root.Length+1);
            }
            else if (path.StartsWith($"{PSDriveInfo.Name}:") )
            {
                path = path.Remove(0, PSDriveInfo.Name.Length+2);
            }
            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return path;
        }

        #endregion
        protected override bool ConvertPath(
            string path,
            string filter,
            ref string updatedPath,
            ref string updatedFilter)
        {
            // In order to support Wildcards?
            // WriteWarning($"ConvertPath ({path}, {filter})");
            
            // Don't handle full paths, paths that the user is already trying to
            // filter, or paths they are trying to escape.
            if ((!string.IsNullOrEmpty(filter)) ||
                (path.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal)) ||
                (path.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal)) ||
                (path.Contains("`"))
                //(path.Contains(StringLiterals.DefaultPathSeparator, StringComparison.Ordinal)) ||
                //(path.Contains(StringLiterals.AlternatePathSeparator, StringComparison.Ordinal)) ||
                //(path.Contains(StringLiterals.EscapeCharacter))
                )
            {
                Console.WriteLine("ConvertPath Returned False");
                return false;
            }

            // We can never actually modify the PowerShell path, as the
            // Win32 filtering support returns items that match the short
            // filename OR long filename.
            //
            // This creates tons of seemingly incorrect matches, such as:
            //
            // *~*:   Matches any file with a long filename
            // *n*:   Matches all files with a long filename, but have been
            //        mapped to a [6][~n].[3] disambiguation bucket
            // *.abc: Matches all files that have an extension that begins
            //        with ABC, since their extension is truncated in the
            //        short filename
            // *.*:   Matches all files and directories, even if they don't
            //        have a dot in their name

            // Our algorithm here is pretty simple. The filesystem can handle
            // * and ? in PowerShell wildcards, just not character ranges [a-z].
            // We replace character ranges with the single-character wildcard, '?'.
            updatedPath = path;
            updatedFilter = System.Text.RegularExpressions.Regex.Replace(path, "\\[.*?\\]", "?");
            // WriteWarning($"ConvertPath ({updatedPath}, {updatedFilter})");
            return true;
        }
        
    }
}