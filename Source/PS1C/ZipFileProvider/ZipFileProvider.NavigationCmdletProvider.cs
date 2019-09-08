using System;
using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.IO;

using System.IO.Compression;

using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Linq;
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

            if ( String.IsNullOrEmpty(path) )
            {
                return true;
            }

			return false;
		}
        internal ZipFileItemInfo GetItemHelper(string path)
		{
            path = NormalizePath(path);

            return new ZipFileItemInfo(ZipFileDriveInfo, path);
		}


        private string NormalizePath(string path)
        {

            // If PSDriveInfo is null search for relative indicators for a specific PSDrive.
            // This addresses an issue with PSDriveInfo returning null when running the following command
            // Get-Item $file | Remove-Item
            if (PSDriveInfo == null) {
                if (path.Contains(Path.VolumeSeparatorChar))
                {

                    SessionState.Drive.GetAllForProvider(ProviderName).ToList().ForEach( i => {
                        if ( (path.StartsWith(i.Root)) || (path.StartsWith(i.Name)) )
                        {
                            _psDriveInfo = i;
                        }
                    });

                }
            }
            
            if (path.StartsWith(ZipFileDriveInfo.Root))
            {
                path = path.Remove(0, ZipFileDriveInfo.Root.Length+1);
            }
            else if (path.StartsWith($"{ZipFileDriveInfo.Name}:") )
            {
                path = path.Remove(0, ZipFileDriveInfo.Name.Length+2);
            }
            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return path;
        }

        #endregion
        
    }
}