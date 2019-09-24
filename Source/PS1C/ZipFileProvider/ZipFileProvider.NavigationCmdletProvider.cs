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

        protected bool IsItemContainerContainsItems(string path)
        {
            bool result = false;

            if (!Path.EndsInDirectorySeparator(path))
            {
                path += Path.DirectorySeparatorChar;
            }
            path += "*";
            
            ZipFileItemInfo[] items = ZipFileDriveInfo.GetItem(path).ToArray();

            if (items.Length > 0)
            {
                result = true;
            }

            return result;
        }

		protected override bool IsItemContainer(string path)
		{
            path = NormalizePath(path);
            
            if ( String.IsNullOrEmpty(path) )
            {
                return true;
            }
            else if ( path == "\\" || path == "/")
            {
                return true;
            }

            return ZipFileDriveInfo.IsItemContainer(path);
		}
        internal ZipFileItemInfo GetItemHelper(string path)
		{
            path = NormalizePath(path);

            return new ZipFileItemInfo(ZipFileDriveInfo, path);
		}

        #endregion
        
    }
}