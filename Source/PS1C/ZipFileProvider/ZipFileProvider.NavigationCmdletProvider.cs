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
        internal ZipFileItemInfo GetItemHelper(string path)
		{
            path = NormalizePath(path);

            return new ZipFileItemInfo(PSDriveInfo, path);
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

        
    }
}