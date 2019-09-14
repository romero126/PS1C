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
            bool result = false;
            
            if ( String.IsNullOrEmpty(path) )
            {
                result = true;
            }
            else if ( path == "\\" || path == "/")
            {
                result = true;
            }


            // We need to fix this..

            if (!Path.EndsInDirectorySeparator(path))
            {
                path += Path.AltDirectorySeparatorChar;
            }

            bool itemExists = ItemExists(path);
            if (itemExists)
            {
                result = true;
            }

			return result;
		}
        internal ZipFileItemInfo GetItemHelper(string path)
		{
            path = NormalizePath(path);

            return new ZipFileItemInfo(ZipFileDriveInfo, path);
		}

        #endregion
        
    }
}