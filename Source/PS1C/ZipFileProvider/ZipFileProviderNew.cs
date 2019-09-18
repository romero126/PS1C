using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Provider;
using System.Threading;

namespace PS1C
{
    #region ZipFileProvider
    //[CmdletProvider(ZipFileProvider.ProviderName, ProviderCapabilities.Credentials | ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess)]
    [CmdletProvider(ZipFileProvider.ProviderName, ProviderCapabilities.ShouldProcess | ProviderCapabilities.ExpandWildcards )]
    public partial class ZipFileProvider : NavigationCmdletProvider,
                                           IContentCmdletProvider
    //                                       IPropertyCmdletProvider
    //                                       ISecurityDescriptorCmdletProvider,
    //                                       ICmdletProviderSupportsHelp
    {
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public const string ProviderName = "ZipFile";


        internal ZipFilePSDriveInfo ZipFileDriveInfo {
            get {
                if (_psDriveInfo != null)
                {
                    return _psDriveInfo;
                }
                return (PSDriveInfo as ZipFilePSDriveInfo);
            }
            private set
            {
                _psDriveInfo = value;
            }
        }
        internal ZipFilePSDriveInfo _psDriveInfo;

        /// <summary>
        /// Initializes a new instance of the FileSystemProvider class. Since this
        /// object needs to be stateless, the constructor does nothing.
        /// </summary>
        public ZipFileProvider()
        {

        }

        /// <summary>
        /// Converts all / in the path to \
        /// </summary>
        ///
        /// <param name="path">
        /// The path to normalize.
        /// </param>
        ///
        /// <returns>
        /// The path with all / normalized to \
        /// and resolve the path based off of its Root/Name
        /// </returns>
        private string NormalizePath(string path)
        {

            // [Bug] PSDriveInfo sometimes does not get instantiated with the provider
            // this causes stateful issues with complex providers.
            // Example Duplication of this issue
            //
            // ./<tabkey>
            // and 
            // Get-Item $FileName | Remove-Item
            //
            // Current Workaround searches all Drives with ProviderName
            // and checks relative path and overrides the path lookup.
            
            if (PSDriveInfo == null) {
                if (path.Contains(Path.VolumeSeparatorChar))
                {
                    SessionState.Drive.GetAllForProvider(ProviderName).ToList().ForEach( i => {
                        if ( (path.StartsWith(i.Root)) || (path.StartsWith(i.Name)) )
                        {
                            ZipFileDriveInfo = (i as ZipFilePSDriveInfo);
                        }
                    });

                }
            }

            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                PSTraceSource.NewArgumentException(ZipFileProviderStrings.PathContainsInvalidCharacters);
            }

            if (path.StartsWith($"{ZipFileDriveInfo.Root}"))
            {
                path = path.Remove(0, ZipFileDriveInfo.Root.Length);
            }
            else if (path.StartsWith($"{ZipFileDriveInfo.Name}:") )
            {
                path = path.Remove(0, ZipFileDriveInfo.Name.Length+1);
            }

            path = path.TrimStart(Path.DirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar);

            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            
            // Before returning a normalized path
            return path;
        }
    }
    #endregion ZipFileProvider
}